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
        private readonly object volumeLock = new object();

        [NotNull]
        private string label;

        private long freeSpaceInBytes;

        public long CapacityInBytes { get; }

        public long FreeSpaceInBytes
        {
            get
            {
                lock (volumeLock)
                {
                    return freeSpaceInBytes;
                }
            }
        }

        public DriveType Type { get; }

        [NotNull]
        public string Format { get; }

        [NotNull]
        public string Label
        {
            get
            {
                lock (volumeLock)
                {
                    return label;
                }
            }
            internal set
            {
                lock (volumeLock)
                {
                    label = string.IsNullOrEmpty(value) ? string.Empty : value;
                }
            }
        }

        public VolumeEntry([NotNull] string name, long capacityInBytes, long freeSpaceInBytes, DriveType type,
            [NotNull] string format, [NotNull] string label, [NotNull] FakeFileSystemChangeTracker changeTracker,
            [NotNull] SystemClock systemClock, [NotNull] ILoggedOnUserAccount loggedOnAccount)
            : base(name, AbsolutePath.IsDriveLetter(name) ? MinimumDriveAttributes : FileAttributes.Directory, null, null,
                changeTracker, systemClock, loggedOnAccount)
        {
            Guard.NotNull(format, nameof(format));
            Guard.NotNull(label, nameof(label));
            AssertCapacityIsNotNegative(capacityInBytes);
            AssertFreeSpaceIsNotNegativeAndInRange(capacityInBytes, freeSpaceInBytes);

            CapacityInBytes = capacityInBytes;
            this.freeSpaceInBytes = freeSpaceInBytes;
            Type = type;
            Format = format;
            this.label = label;

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
                throw new ArgumentOutOfRangeException(nameof(freeSpaceInBytes),
                    "Free space cannot be negative or exceed volume capacity.");
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
            lock (volumeLock)
            {
                long newFreeSpace = freeSpaceInBytes - sizeInBytes;
                if (newFreeSpace >= 0L)
                {
                    AssertFreeSpaceIsNotNegativeAndInRange(CapacityInBytes, newFreeSpace);

                    freeSpaceInBytes = newFreeSpace;
                    return true;
                }

                return false;
            }
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
