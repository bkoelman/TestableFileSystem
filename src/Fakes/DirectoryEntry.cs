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

        private DirectoryEntry([NotNull] string name, [CanBeNull] DirectoryEntry parent, [NotNull] SystemClock systemClock)
            : base(name)
        {
            Parent = parent;
            Attributes = AbsolutePath.IsDriveLetter(name) ? MinimumDriveAttributes : FileAttributes.Directory;
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
        public static DirectoryEntry CreateRoot([NotNull] SystemClock systemClock)
        {
            Guard.NotNull(systemClock, nameof(systemClock));

            return new DirectoryEntry("My Computer", null, systemClock);
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
            Guard.NotNull(fileName, nameof(fileName));

            var fileEntry = new FileEntry(fileName, this);
            contents.Add(fileEntry);

            HandleDirectoryChanged();

            return fileEntry;
        }

        public void DeleteFile([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            contents.RemoveFile(fileName);

            HandleDirectoryChanged();
        }

        public void MoveFileToHere([NotNull] FileEntry file, [NotNull] string newFileName)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNullNorWhiteSpace(newFileName, nameof(newFileName));

            file.MoveTo(newFileName, this);
            contents.Add(file);

            HandleDirectoryChanged();
        }

        [NotNull]
        public DirectoryEntry CreateDirectory([NotNull] string directoryName)
        {
            Guard.NotNull(directoryName, nameof(directoryName));

            var directoryEntry = new DirectoryEntry(directoryName, this, SystemClock);
            contents.Add(directoryEntry);

            HandleDirectoryChanged();

            return directoryEntry;
        }

        public void DeleteDirectory([NotNull] string directoryName)
        {
            Guard.NotNull(directoryName, nameof(directoryName));

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
    }
}
