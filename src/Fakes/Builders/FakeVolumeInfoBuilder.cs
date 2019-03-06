using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Builders
{
    public sealed class FakeVolumeInfoBuilder : ITestDataBuilder<FakeVolumeInfo>
    {
        private const long OneGigabyte = 1024 * 1024 * 1024;

        private long capacityInBytes = OneGigabyte;
        private long freeSpaceInBytes = OneGigabyte;
        private DriveType driveType = DriveType.Fixed;

        [NotNull]
        private string volumeLabel = string.Empty;

        [NotNull]
        private string driveFormat = FakeVolumeInfo.NtFs;

        public FakeVolumeInfo Build()
        {
            return new FakeVolumeInfo(capacityInBytes, freeSpaceInBytes, driveType, driveFormat, volumeLabel);
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
            freeSpaceInBytes = bytes;
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
