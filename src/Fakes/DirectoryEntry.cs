using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly DirectoryContents contents = new DirectoryContents();

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<FileEntry> Files => contents.Files;

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<DirectoryEntry> Directories => contents.Directories;

        [CanBeNull]
        public DirectoryEntry Parent { get; private set; }

        public bool IsEmpty => contents.IsEmpty;

        [NotNull]
        internal SystemClock SystemClock { get; }

        internal override IPathFormatter PathFormatter { get; }

        private DirectoryEntry([NotNull] string name, [CanBeNull] DirectoryEntry parent,
            [NotNull] FakeFileSystemChangeTracker changeTracker, [NotNull] SystemClock systemClock,
            [NotNull] ILoggedOnUserAccount loggedOnAccount)
            : base(name, AbsolutePath.IsDriveLetter(name) ? MinimumDriveAttributes : FileAttributes.Directory, changeTracker,
                loggedOnAccount)
        {
            Parent = parent;
            PathFormatter = new DirectoryEntryPathFormatter(this);
            SystemClock = systemClock;

            if (parent?.IsEncrypted == true)
            {
                SetEncrypted();
            }

            CreationTimeUtc = systemClock.UtcNow();
            UpdateLastWriteLastAccessTime();
        }

        private void HandleDirectoryContentsChanged()
        {
            UpdateLastWriteLastAccessTime();
            ChangeTracker.NotifyContentsAccessed(PathFormatter, FileAccessKinds.Write | FileAccessKinds.Read);
        }

        private void HandleDirectoryContentsAccessed()
        {
            UpdateLastAccessTime();
            ChangeTracker.NotifyContentsAccessed(PathFormatter, FileAccessKinds.Read);
        }

        private void UpdateLastWriteLastAccessTime()
        {
            UpdateLastAccessTime();
            LastWriteTimeUtc = LastAccessTimeUtc;
        }

        private void UpdateLastAccessTime()
        {
            LastAccessTimeUtc = SystemClock.UtcNow();
        }

        [NotNull]
        public static DirectoryEntry CreateRoot([NotNull] FakeFileSystemChangeTracker changeTracker,
            [NotNull] SystemClock systemClock, [NotNull] ILoggedOnUserAccount loggedOnAccount)
        {
            Guard.NotNull(changeTracker, nameof(changeTracker));
            Guard.NotNull(systemClock, nameof(systemClock));

            return new DirectoryEntry("My Computer", null, changeTracker, systemClock, loggedOnAccount);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<BaseEntry> EnumerateEntries(EnumerationFilter filter)
        {
            UpdateLastAccessTime();

            return contents.GetEntries(filter);
        }

        [NotNull]
        [ItemNotNull]
        public ICollection<DirectoryEntry> FilterDrives()
        {
            return Directories.Where(x => AbsolutePath.IsDriveLetter(x.Name)).ToArray();
        }

        public bool ContainsFile([NotNull] string fileName)
        {
            Guard.NotNullNorWhiteSpace(fileName, nameof(fileName));

            return contents.ContainsFile(fileName);
        }

        [NotNull]
        public FileEntry GetFile([NotNull] string fileName)
        {
            Guard.NotNullNorWhiteSpace(fileName, nameof(fileName));

            return contents.GetFile(fileName);
        }

        [NotNull]
        public FileEntry CreateFile([NotNull] string fileName)
        {
            Guard.NotNullNorWhiteSpace(fileName, nameof(fileName));

            var fileEntry = new FileEntry(fileName, this);
            contents.Add(fileEntry);

            UpdateLastWriteLastAccessTime();
            ChangeTracker.NotifyFileCreated(fileEntry.PathFormatter);

            return fileEntry;
        }

        public void DeleteFile([NotNull] string fileName)
        {
            Guard.NotNullNorWhiteSpace(fileName, nameof(fileName));

            FileEntry fileEntry = contents.RemoveFile(fileName);

            UpdateLastWriteLastAccessTime();
            ChangeTracker.NotifyFileDeleted(fileEntry.PathFormatter);
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

            ChangeTracker.NotifyFileRenamed(sourcePathFormatter, file.PathFormatter);
            HandleDirectoryContentsChanged();
            NotifyFileMoveForAttributes(file);
        }

        public void MoveFileToHere([NotNull] FileEntry file, [NotNull] string newFileName)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNullNorWhiteSpace(newFileName, nameof(newFileName));

            file.MoveTo(newFileName, this);
            contents.Add(file);

            ChangeTracker.NotifyFileCreated(file.PathFormatter);
            HandleDirectoryContentsChanged();
            NotifyFileMoveForAttributes(file);
        }

        private void NotifyFileMoveForAttributes([NotNull] FileEntry file)
        {
            if (!file.Attributes.HasFlag(FileAttributes.Archive))
            {
                ChangeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Attributes);
            }
        }

        public bool ContainsDirectory([NotNull] string directoryName)
        {
            Guard.NotNullNorWhiteSpace(directoryName, nameof(directoryName));

            return contents.ContainsDirectory(directoryName);
        }

        [NotNull]
        public DirectoryEntry GetDirectory([NotNull] string directoryName)
        {
            Guard.NotNullNorWhiteSpace(directoryName, nameof(directoryName));

            return contents.GetDirectory(directoryName);
        }

        [NotNull]
        public DirectoryEntry CreateDirectory([NotNull] string directoryName)
        {
            Guard.NotNullNorWhiteSpace(directoryName, nameof(directoryName));

            var directoryEntry = new DirectoryEntry(directoryName, this, ChangeTracker, SystemClock, LoggedOnAccount);
            contents.Add(directoryEntry);

            UpdateLastWriteLastAccessTime();
            ChangeTracker.NotifyDirectoryCreated(directoryEntry.PathFormatter);

            return directoryEntry;
        }

        public void DeleteDirectory([NotNull] string directoryName)
        {
            Guard.NotNullNorWhiteSpace(directoryName, nameof(directoryName));

            DirectoryEntry directoryEntry = contents.RemoveDirectory(directoryName);

            UpdateLastWriteLastAccessTime();
            ChangeTracker.NotifyDirectoryDeleted(directoryEntry.PathFormatter);
        }

        public void RenameDirectory([NotNull] string sourceDirectoryName, [NotNull] string destinationDirectoryName,
            [NotNull] IPathFormatter sourcePathFormatter)
        {
            Guard.NotNullNorWhiteSpace(sourceDirectoryName, nameof(sourceDirectoryName));
            Guard.NotNullNorWhiteSpace(destinationDirectoryName, nameof(destinationDirectoryName));
            Guard.NotNull(sourcePathFormatter, nameof(sourcePathFormatter));

            DirectoryEntry directory = contents.RemoveDirectory(sourceDirectoryName);
            directory.Name = destinationDirectoryName;
            contents.Add(directory);

            HandleDirectoryContentsAccessed();
            ChangeTracker.NotifyDirectoryRenamed(sourcePathFormatter, directory.PathFormatter);
            HandleDirectoryContentsChanged();
        }

        public void MoveDirectoryToHere([NotNull] DirectoryEntry directory, [NotNull] string newDirectoryName)
        {
            Guard.NotNull(directory, nameof(directory));
            Guard.NotNullNorWhiteSpace(newDirectoryName, nameof(newDirectoryName));

            directory.Name = newDirectoryName;
            directory.Parent = this;

            contents.Add(directory);

            UpdateLastWriteLastAccessTime();
            ChangeTracker.NotifyDirectoryCreated(directory.PathFormatter);
            HandleDirectoryContentsChanged();
        }

        public override string ToString()
        {
            return $"Directory: {Name} ({contents})";
        }

        protected override FileAttributes FilterAttributes(FileAttributes attributes)
        {
            if (attributes.HasFlag(FileAttributes.Temporary))
            {
                throw new ArgumentException("Invalid File or Directory attributes value.", nameof(attributes));
            }

            FileAttributes minimumAttributes = Parent?.Parent == null ? MinimumDriveAttributes : FileAttributes.Directory;
            return (attributes & ~DirectoryAttributesToDiscard) | minimumAttributes;
        }

        [DebuggerDisplay("{GetPath().GetText()}")]
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
