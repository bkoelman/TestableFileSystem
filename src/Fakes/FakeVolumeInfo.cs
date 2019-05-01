using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeVolumeInfo
    {
        internal const string NtFs = "NTFS";

        public long CapacityInBytes { get; }
        public long FreeSpaceInBytes { get; }
        public DriveType Type { get; }

        [NotNull]
        public string Format { get; }

        [NotNull]
        public string Label { get; }

        internal FakeVolumeInfo(long capacityInBytes, long freeSpaceInBytes, DriveType type, [NotNull] string format,
            [NotNull] string label)
        {
            Guard.NotNull(format, nameof(format));
            Guard.NotNull(label, nameof(label));

            CapacityInBytes = capacityInBytes;
            FreeSpaceInBytes = freeSpaceInBytes;
            Type = type;
            Format = format;
            Label = label;
        }
    }
}
