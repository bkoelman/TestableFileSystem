using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeVolume
    {
        internal const string NtFs = "NTFS";

        [NotNull]
        private string label;

        public long CapacityInBytes { get; }
        public long FreeSpaceInBytes { get; }
        public DriveType Type { get; }

        [NotNull]
        public string Format { get; }

        [NotNull]
        public string Label
        {
            get => label;
            internal set => label = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        internal FakeVolume(long capacityInBytes, long freeSpaceInBytes, DriveType type, [NotNull] string format,
            [NotNull] string label)
        {
            Guard.NotNull(format, nameof(format));
            Guard.NotNull(label, nameof(label));
            AssertNotNegativeAndInRange(capacityInBytes, freeSpaceInBytes);

            CapacityInBytes = capacityInBytes;
            FreeSpaceInBytes = freeSpaceInBytes;
            Type = type;
            Format = format;
            this.label = label;
        }

        private static void AssertNotNegativeAndInRange(long capacityInBytes, long freeSpaceInBytes)
        {
            if (capacityInBytes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacityInBytes), "Volume capacity cannot be negative.");
            }

            if (freeSpaceInBytes < 0 || freeSpaceInBytes > capacityInBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(freeSpaceInBytes),
                    "Available space cannot be negative or exceed volume capacity.");
            }
        }
    }
}
