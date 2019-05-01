using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class VolumeEntry : DirectoryEntry
    {
        private const FileAttributes DriveAttributesToDiscard = FileAttributes.Device | FileAttributes.Normal |
            FileAttributes.SparseFile | FileAttributes.Compressed | FileAttributes.Encrypted | FileAttributes.IntegrityStream;

        private const FileAttributes MinimumDriveAttributes =
            FileAttributes.Directory | FileAttributes.System | FileAttributes.Hidden;

        internal override IPathFormatter PathFormatter { get; }

        [NotNull]
        private readonly FileSystemLock fileSystemLock;

        [NotNull]
        private string label;

        private long freeSpaceInBytes;

        public long CapacityInBytes { get; }

        public long FreeSpaceInBytes => fileSystemLock.ExecuteInLock(() => freeSpaceInBytes);

        public DriveType Type { get; }

        [NotNull]
        public string Format { get; }

        [NotNull]
        public string Label
        {
            get => fileSystemLock.ExecuteInLock(() => label);
            internal set
            {
                fileSystemLock.ExecuteInLock(() => { label = string.IsNullOrEmpty(value) ? string.Empty : value; });
            }
        }

        public VolumeEntry([NotNull] string name, long capacityInBytes, long freeSpaceInBytes, DriveType type,
            [NotNull] string format, [NotNull] string label, [NotNull] FileSystemLock fileSystemLock,
            [NotNull] SystemClock systemClock, [NotNull] FakeFileSystemChangeTracker changeTracker,
            [NotNull] ILoggedOnUserAccount loggedOnAccount)
            : base(name, AbsolutePath.IsDriveLetter(name) ? MinimumDriveAttributes : FileAttributes.Directory, null, null,
                changeTracker, systemClock, loggedOnAccount)
        {
            Guard.NotNull(format, nameof(format));
            Guard.NotNull(label, nameof(label));
            Guard.NotNull(fileSystemLock, nameof(fileSystemLock));

            AssertCapacityIsNotNegative(capacityInBytes);
            AssertFreeSpaceIsNotNegativeAndInRange(capacityInBytes, freeSpaceInBytes);

            CapacityInBytes = capacityInBytes;
            this.freeSpaceInBytes = freeSpaceInBytes;
            Type = type;
            Format = format;
            this.label = label;
            this.fileSystemLock = fileSystemLock;

            PathFormatter = new VolumeEntryPathFormatter(this);
        }

        [AssertionMethod]
        private static void AssertCapacityIsNotNegative(long capacityInBytes)
        {
            if (capacityInBytes < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(capacityInBytes), "Volume capacity cannot be negative.");
            }
        }

        [AssertionMethod]
        private static void AssertFreeSpaceIsNotNegativeAndInRange(long capacityInBytes, long freeSpaceInBytes)
        {
            if (freeSpaceInBytes < 0L || freeSpaceInBytes > capacityInBytes)
            {
                throw ErrorFactory.Internal.UnknownError(
                    $"Free space ({freeSpaceInBytes} bytes) cannot be negative or exceed volume capacity ({capacityInBytes} bytes).");
            }
        }

        protected override FileAttributes FilterAttributes(FileAttributes attributes)
        {
            if (attributes.HasFlag(FileAttributes.Temporary))
            {
                throw new ArgumentException("Invalid File or Directory attributes value.", nameof(attributes));
            }

            return (attributes & ~DriveAttributesToDiscard) | MinimumDriveAttributes;
        }

        internal bool TryAllocateSpace(long sizeInBytes)
        {
            return fileSystemLock.ExecuteInLock(() =>
            {
                long newFreeSpace = freeSpaceInBytes - sizeInBytes;
                if (newFreeSpace >= 0L)
                {
                    AssertFreeSpaceIsNotNegativeAndInRange(CapacityInBytes, newFreeSpace);

                    freeSpaceInBytes = newFreeSpace;
                    return true;
                }

                return false;
            });
        }

        public override string ToString()
        {
            return base.ToString().Replace("Directory:", "Volume:");
        }

        [DebuggerDisplay("{GetPath().GetText()}")]
        private sealed class VolumeEntryPathFormatter : IPathFormatter
        {
            [NotNull]
            private readonly VolumeEntry volumeEntry;

            public VolumeEntryPathFormatter([NotNull] VolumeEntry volumeEntry)
            {
                Guard.NotNull(volumeEntry, nameof(volumeEntry));
                this.volumeEntry = volumeEntry;
            }

            public AbsolutePath GetPath()
            {
                return new AbsolutePath(volumeEntry.Name);
            }
        }
    }
}
