using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class DirectoryEntry : BaseEntry
    {
        private const FileAttributes DirectoryAttributesToDiscard = FileAttributes.Device | FileAttributes.Normal |
            FileAttributes.SparseFile | FileAttributes.Compressed | FileAttributes.Encrypted | FileAttributes.IntegrityStream;

        private const FileAttributes MinimumDriveAttributes =
            FileAttributes.Directory | FileAttributes.System | FileAttributes.Hidden;

        [NotNull]
        internal readonly SystemClock SystemClock;

        [NotNull]
        private readonly DirectoryContents contents = new DirectoryContents();

        [NotNull]
        public IReadOnlyDictionary<string, FileEntry> Files => contents.Files;

        [NotNull]
        public IReadOnlyDictionary<string, DirectoryEntry> Directories => contents.Directories;

        internal override IPathFormatter PathFormatter { get; }

        [CanBeNull]
        public DirectoryEntry Parent { get; private set; }

        public bool IsEmpty => contents.IsEmpty;

        private long creationTimeStampUtc;
        private long lastWriteTimeStampUtc;
        private long lastAccessTimeStampUtc;

        public override DateTime CreationTime
        {
            get => DateTime.FromFileTime(creationTimeStampUtc);
            set => creationTimeStampUtc = value.ToFileTime();
        }

        public override DateTime CreationTimeUtc
        {
            get => DateTime.FromFileTimeUtc(creationTimeStampUtc);
            set => creationTimeStampUtc = value.ToFileTimeUtc();
        }

        public override DateTime LastAccessTime
        {
            get => DateTime.FromFileTime(lastAccessTimeStampUtc);
            set => lastAccessTimeStampUtc = value.ToFileTime();
        }

        public override DateTime LastAccessTimeUtc
        {
            get => DateTime.FromFileTimeUtc(lastAccessTimeStampUtc);
            set => lastAccessTimeStampUtc = value.ToFileTimeUtc();
        }

        public override DateTime LastWriteTime
        {
            get => DateTime.FromFileTime(lastWriteTimeStampUtc);
            set => lastWriteTimeStampUtc = value.ToFileTime();
        }

        public override DateTime LastWriteTimeUtc
        {
            get => DateTime.FromFileTimeUtc(lastWriteTimeStampUtc);
            set => lastWriteTimeStampUtc = value.ToFileTimeUtc();
        }

        private DirectoryEntry([NotNull] string name, [CanBeNull] DirectoryEntry parent,
            [NotNull] FakeFileSystemChangeTracker changeTracker, [NotNull] SystemClock systemClock)
            : base(name, AbsolutePath.IsDriveLetter(name) ? MinimumDriveAttributes : FileAttributes.Directory, changeTracker)
        {
            Parent = parent;
            PathFormatter = new DirectoryEntryPathFormatter(this);
            SystemClock = systemClock;

            CreationTimeUtc = systemClock.UtcNow();
            HandleDirectoryChanged();
        }

        private void HandleDirectoryChanged()
        {
            HandleDirectoryAccessed();
            LastWriteTimeUtc = LastAccessTimeUtc;
        }

        private void HandleDirectoryAccessed()
        {
            LastAccessTimeUtc = SystemClock.UtcNow();
        }

        [NotNull]
        public static DirectoryEntry CreateRoot([NotNull] FakeFileSystemChangeTracker changeTracker,
            [NotNull] SystemClock systemClock)
        {
            Guard.NotNull(changeTracker, nameof(changeTracker));
            Guard.NotNull(systemClock, nameof(systemClock));

            return new DirectoryEntry("My Computer", null, changeTracker, systemClock);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<BaseEntry> EnumerateEntries(EnumerationFilter filter)
        {
            HandleDirectoryAccessed();

            return contents.GetEntries(filter);
        }

        [NotNull]
        [ItemNotNull]
        public ICollection<DirectoryEntry> FilterDrives()
        {
            return Directories.Where(x => x.Key.IndexOf(Path.VolumeSeparatorChar) != -1).Select(x => x.Value).ToArray();
        }

        [NotNull]
        public FileEntry CreateFile([NotNull] string fileName)
        {
            Guard.NotNullNorWhiteSpace(fileName, nameof(fileName));

            var fileEntry = new FileEntry(fileName, this, ChangeTracker);
            contents.Add(fileEntry);

            ChangeTracker.NotifyFileCreated(fileEntry.PathFormatter);

            HandleDirectoryChanged();

            return fileEntry;
        }

        public void DeleteFile([NotNull] string fileName)
        {
            Guard.NotNullNorWhiteSpace(fileName, nameof(fileName));

            FileEntry fileEntry = contents.RemoveFile(fileName);

            ChangeTracker.NotifyFileDeleted(fileEntry.PathFormatter);
            HandleDirectoryChanged();
        }

        public void RenameFile([NotNull] string sourceFileName, [NotNull] string destinationFileName,
            [NotNull] IPathFormatter sourcePathFormatter)
        {
            Guard.NotNullNorWhiteSpace(sourceFileName, nameof(sourceFileName));
            Guard.NotNullNorWhiteSpace(destinationFileName, nameof(destinationFileName));
            Guard.NotNull(sourcePathFormatter, nameof(sourcePathFormatter));

            FileEntry file = contents.RemoveFile(sourceFileName);
            file.MoveTo(destinationFileName, this);
            contents.Add(file);

            ChangeTracker.NotifyFileMoved(sourcePathFormatter, file.PathFormatter);
            NotifyFileMoveForAttributes(file);
            HandleDirectoryChanged();
        }

        public void MoveFileToHere([NotNull] FileEntry file, [NotNull] string newFileName)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNullNorWhiteSpace(newFileName, nameof(newFileName));

            file.MoveTo(newFileName, this);
            contents.Add(file);

            ChangeTracker.NotifyFileCreated(file.PathFormatter);
            ChangeTracker.NotifyContentsAccessed(PathFormatter, FileAccessKinds.Write | FileAccessKinds.Read);
            NotifyFileMoveForAttributes(file);
            HandleDirectoryChanged();
        }

        private void NotifyFileMoveForAttributes([NotNull] FileEntry file)
        {
            if (!file.Attributes.HasFlag(FileAttributes.Archive))
            {
                ChangeTracker.NotifyAttributesChanged(file.PathFormatter);
            }
        }

        [NotNull]
        public DirectoryEntry CreateDirectory([NotNull] string directoryName)
        {
            Guard.NotNullNorWhiteSpace(directoryName, nameof(directoryName));

            var directoryEntry = new DirectoryEntry(directoryName, this, ChangeTracker, SystemClock);
            contents.Add(directoryEntry);

            HandleDirectoryChanged();

            return directoryEntry;
        }

        public void DeleteDirectory([NotNull] string directoryName)
        {
            Guard.NotNullNorWhiteSpace(directoryName, nameof(directoryName));

            contents.RemoveDirectory(directoryName);

            HandleDirectoryChanged();
        }

        public void MoveDirectoryToHere([NotNull] DirectoryEntry directory, [NotNull] string newDirectoryName)
        {
            Guard.NotNull(directory, nameof(directory));
            Guard.NotNullNorWhiteSpace(newDirectoryName, nameof(newDirectoryName));

            directory.Name = newDirectoryName;
            directory.Parent = this;

            contents.Add(directory);

            HandleDirectoryChanged();
        }

        public override string ToString()
        {
            return $"Directory: {Name} ({contents})";
        }

        protected override FileAttributes FilterAttributes(FileAttributes attributes)
        {
            if ((attributes & FileAttributes.Temporary) != 0)
            {
                throw new ArgumentException("Invalid File or Directory attributes value.", nameof(attributes));
            }

            FileAttributes minimumAttributes = Parent?.Parent == null ? MinimumDriveAttributes : FileAttributes.Directory;
            return (attributes & ~DirectoryAttributesToDiscard) | minimumAttributes;
        }

        private sealed class DirectoryEntryPathFormatter : IPathFormatter
        {
            [NotNull]
            private readonly DirectoryEntry directoryEntry;

            public DirectoryEntryPathFormatter([NotNull] DirectoryEntry directoryEntry)
            {
                Guard.NotNull(directoryEntry, nameof(directoryEntry));
                this.directoryEntry = directoryEntry;
            }

            public AbsolutePath GetPath()
            {
                string text = GetText();
                return new AbsolutePath(text);
            }

            [NotNull]
            private string GetText()
            {
                var componentStack = new Stack<string>();

                DirectoryEntry directory = directoryEntry;
                while (directory.Parent != null)
                {
                    componentStack.Push(directory.Name);

                    directory = directory.Parent;
                }

                return string.Join("\\", componentStack);
            }
        }
    }
}
