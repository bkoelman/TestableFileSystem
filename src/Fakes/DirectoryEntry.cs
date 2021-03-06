﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal class DirectoryEntry : BaseEntry
    {
        private const FileAttributes DirectoryAttributesToDiscard = FileAttributes.Device | FileAttributes.Normal |
            FileAttributes.SparseFile | FileAttributes.Compressed | FileAttributes.Encrypted | FileAttributes.IntegrityStream;

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

        [NotNull]
        public VolumeEntry Root { get; }

        public bool IsEmpty => contents.IsEmpty;

        [NotNull]
        internal SystemClock SystemClock { get; }

        internal override IPathFormatter PathFormatter { get; }

        protected DirectoryEntry([NotNull] string name, FileAttributes attributes, [CanBeNull] DirectoryEntry parent,
            [CanBeNull] VolumeEntry root, [NotNull] FakeFileSystemChangeTracker changeTracker, [NotNull] SystemClock systemClock,
            [NotNull] ILoggedOnUserAccount loggedOnAccount)
            : base(name, attributes, changeTracker, loggedOnAccount)
        {
            Guard.NotNull(systemClock, nameof(systemClock));

            Parent = parent;
            Root = root ?? (VolumeEntry)this;
            SystemClock = systemClock;
            PathFormatter = new DirectoryEntryPathFormatter(this);

            if (parent?.IsEncrypted == true)
            {
                SetEncrypted();
            }

            CreationTimeUtc = systemClock.UtcNow();
            UpdateLastWriteLastAccessTime();
        }

        private void HandleDirectoryContentsChanged(bool skipNotifyLastAccess)
        {
            UpdateLastWriteLastAccessTime();

            if (skipNotifyLastAccess)
            {
                ChangeTracker.NotifyContentsAccessed(PathFormatter, FileAccessKinds.Write);
            }
            else
            {
                ChangeTracker.NotifyContentsAccessed(PathFormatter, FileAccessKinds.WriteRead);
            }
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
        [ItemNotNull]
        public IEnumerable<BaseEntry> EnumerateEntries(EnumerationFilter filter)
        {
            UpdateLastAccessTime();

            return contents.GetEntries(filter);
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

        public void DeleteFile([NotNull] string fileName, bool notifyTracker)
        {
            Guard.NotNullNorWhiteSpace(fileName, nameof(fileName));

            FileEntry fileEntry = contents.RemoveFile(fileName);

            if (!Root.TryAllocateSpace(-fileEntry.Size))
            {
                throw ErrorFactory.Internal.UnknownError(
                    $"Disk space on volume '{Root.Name}' ({Root.FreeSpaceInBytes} bytes) would become negative.");
            }

            UpdateLastWriteLastAccessTime();

            if (notifyTracker)
            {
                ChangeTracker.NotifyFileDeleted(fileEntry.PathFormatter);
            }
        }

        public void RenameFile([NotNull] string sourceFileName, [NotNull] string destinationFileName,
            [NotNull] IPathFormatter sourcePathFormatter, bool skipNotifyLastAccess, bool skipNotifyAttributes)
        {
            Guard.NotNullNorWhiteSpace(sourceFileName, nameof(sourceFileName));
            Guard.NotNullNorWhiteSpace(destinationFileName, nameof(destinationFileName));
            Guard.NotNull(sourcePathFormatter, nameof(sourcePathFormatter));

            FileEntry file = contents.RemoveFile(sourceFileName);
            file.MoveTo(destinationFileName, this);
            contents.Add(file);

            ChangeTracker.NotifyFileRenamed(sourcePathFormatter, file.PathFormatter);

            HandleDirectoryContentsChanged(skipNotifyLastAccess);
            NotifyAttributesForFileMove(file, skipNotifyAttributes);
        }

        public void MoveFileToHere([NotNull] FileEntry file, [NotNull] string newFileName, bool skipNotifyLastAccess,
            bool skipNotifyAttributes)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNullNorWhiteSpace(newFileName, nameof(newFileName));

            if (!Root.TryAllocateSpace(file.Size))
            {
                throw ErrorFactory.Internal.UnknownError(
                    $"Disk space on volume '{Root.Name}' ({Root.FreeSpaceInBytes} bytes) would become negative.");
            }

            file.MoveTo(newFileName, this);
            contents.Add(file);

            ChangeTracker.NotifyFileCreated(file.PathFormatter);
            HandleDirectoryContentsChanged(skipNotifyLastAccess);
            NotifyAttributesForFileMove(file, skipNotifyAttributes);
        }

        private void NotifyAttributesForFileMove([NotNull] FileEntry file, bool skipNotifyAttributes)
        {
            if (!skipNotifyAttributes && !file.Attributes.HasFlag(FileAttributes.Archive))
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

            var directoryEntry = new DirectoryEntry(directoryName, FileAttributes.Directory, this, Root, ChangeTracker,
                SystemClock, LoggedOnAccount);
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
            HandleDirectoryContentsChanged(false);
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
            HandleDirectoryContentsChanged(false);
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

            return (attributes & ~DirectoryAttributesToDiscard) | FileAttributes.Directory;
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
                while (directory != null)
                {
                    componentStack.Push(directory.Name);
                    directory = directory.Parent;
                }

                return string.Join(PathFacts.PrimaryDirectorySeparatorString, componentStack);
            }
        }
    }
}
