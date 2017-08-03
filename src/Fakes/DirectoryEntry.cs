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
        private const FileAttributes DirectoryAttributesToDiscard =
            FileAttributes.Device | FileAttributes.Normal | FileAttributes.SparseFile | FileAttributes.Compressed |
            FileAttributes.Encrypted | FileAttributes.IntegrityStream;

        private const FileAttributes MinimumDriveAttributes =
            FileAttributes.Directory | FileAttributes.System | FileAttributes.Hidden;

        [NotNull]
        internal readonly SystemClock SystemClock;

        [NotNull]
        private readonly DirectoryContents contents = new DirectoryContents();

        // TODO: Refactor to prevent making copies.
        [NotNull]
        public IReadOnlyDictionary<string, FileEntry> Files => contents.GetEntries(EnumerationFilter.Files).Cast<FileEntry>()
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        // TODO: Refactor to prevent making copies.
        [NotNull]
        public IReadOnlyDictionary<string, DirectoryEntry> Directories => contents.GetEntries(EnumerationFilter.Directories)
            .Cast<DirectoryEntry>().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        [CanBeNull]
        public DirectoryEntry Parent { get; }

        public bool IsEmpty => contents.IsEmpty;

        public override DateTime CreationTime
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DateTime CreationTimeUtc
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DateTime LastWriteTime
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DateTime LastWriteTimeUtc
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DateTime LastAccessTime
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DateTime LastAccessTimeUtc
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        private DirectoryEntry([NotNull] string name, [CanBeNull] DirectoryEntry parent, [NotNull] SystemClock systemClock)
            : base(name)
        {
            Parent = parent;
            Attributes = IsDriveLetter(name) ? MinimumDriveAttributes : FileAttributes.Directory;
            SystemClock = systemClock;
        }

        private static bool IsDriveLetter([NotNull] string name)
        {
            if (name.Length == 2 && name[1] == Path.VolumeSeparatorChar)
            {
                char driveLetter = char.ToUpperInvariant(name[0]);
                if (driveLetter >= 'A' && driveLetter <= 'Z')
                {
                    return true;
                }
            }

            return false;
        }

        [NotNull]
        public static DirectoryEntry CreateRoot([NotNull] SystemClock systemClock)
        {
            Guard.NotNull(systemClock, nameof(systemClock));

            return new DirectoryEntry("My Computer", null, systemClock);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<BaseEntry> EnumerateEntries(EnumerationFilter filter) => contents.GetEntries(filter);

        [NotNull]
        [ItemNotNull]
        public ICollection<DirectoryEntry> FilterDrives()
        {
            return Directories.Where(x => x.Key.IndexOf(Path.VolumeSeparatorChar) != -1).Select(x => x.Value).ToArray();
        }

        public void AttachFile([NotNull] FileEntry file)
        {
            Guard.NotNull(file, nameof(file));

            contents.Add(file);
        }

        [NotNull]
        public FileEntry CreateFile([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            var fileEntry = new FileEntry(fileName, this);
            return contents.Add(fileEntry);
        }

        public void DeleteFile([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            contents.RemoveFile(fileName);
        }

        [NotNull]
        public DirectoryEntry CreateSingleDirectory([NotNull] string directoryName)
        {
            Guard.NotNull(directoryName, nameof(directoryName));

            var directoryEntry = new DirectoryEntry(directoryName, this, SystemClock);
            return contents.Add(directoryEntry);
        }

        public void DeleteDirectory([NotNull] string directoryName)
        {
            Guard.NotNull(directoryName, nameof(directoryName));

            contents.RemoveDirectory(directoryName);
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
