using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Builders
{
    public sealed class FakeVolumeInfoBuilder : ITestDataBuilder<FakeVolumeInfo>
    {
        private const long OneGigabyte = 1024 * 1024 * 1024;

        private long capacityInBytes = OneGigabyte;

        [CanBeNull]
        private long? freeSpaceInBytes;

        [CanBeNull]
        private long? usedSpaceInBytes;

        private DriveType driveType = DriveType.Fixed;

        [NotNull]
        private string volumeLabel = string.Empty;

        [NotNull]
        private string driveFormat = FakeVolumeInfo.NtFs;

        public FakeVolumeInfo Build()
        {
            long effectiveFreeSpaceInBytes = CalculateFreeSpaceInBytes();
            return new FakeVolumeInfo(capacityInBytes, effectiveFreeSpaceInBytes, driveType, driveFormat, volumeLabel);
        }

        private long CalculateFreeSpaceInBytes()
        {
            AssertCapacityIsNotNegative(capacityInBytes);

            if (freeSpaceInBytes != null)
            {
                AssertFreeSpaceIsNotNegativeAndInRange(freeSpaceInBytes.Value, capacityInBytes);

                return freeSpaceInBytes.Value;
            }

            if (usedSpaceInBytes != null)
            {
                AssertUsedSpaceIsNotNegativeAndInRange(usedSpaceInBytes.Value, capacityInBytes);

                return capacityInBytes - usedSpaceInBytes.Value;
            }

            return capacityInBytes;
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
        private static void AssertFreeSpaceIsNotNegativeAndInRange(long freeSpaceInBytes, long capacityInBytes)
        {
            if (freeSpaceInBytes < 0L || freeSpaceInBytes > capacityInBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(freeSpaceInBytes),
                    "Free space cannot be negative or exceed volume capacity.");
            }
        }

        [AssertionMethod]
        private static void AssertUsedSpaceIsNotNegativeAndInRange(long usedSpaceInBytes, long capacityInBytes)
        {
            if (usedSpaceInBytes < 0L || usedSpaceInBytes > capacityInBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(usedSpaceInBytes),
                    "Used space cannot be negative or exceed volume capacity.");
            }
        }

        [NotNull]
        public FakeVolumeInfoBuilder OfCapacity(long bytes)
        {
            capacityInBytes = bytes;
            return this;
        }

        [NotNull]
        public FakeVolumeInfoBuilder WithFreeSpace(long bytes)
        {
            usedSpaceInBytes = null;
            freeSpaceInBytes = bytes;
            return this;
        }

        [NotNull]
        public FakeVolumeInfoBuilder WithUsedSpace(long bytes)
        {
            freeSpaceInBytes = null;
            usedSpaceInBytes = bytes;
            return this;
        }

        [NotNull]
        public FakeVolumeInfoBuilder OfType(DriveType type)
        {
            driveType = type;
            return this;
        }

        [NotNull]
        public FakeVolumeInfoBuilder InFormat([NotNull] string format)
        {
            Guard.NotNull(format, nameof(format));
            driveFormat = format;
            return this;
        }

        [NotNull]
        public FakeVolumeInfoBuilder Labeled([NotNull] string label)
        {
            Guard.NotNull(label, nameof(label));
            volumeLabel = label;
            return this;
        }
    }
}
