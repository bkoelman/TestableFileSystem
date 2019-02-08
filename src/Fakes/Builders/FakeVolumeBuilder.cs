﻿using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Builders
{
    public sealed class FakeVolumeBuilder : ITestDataBuilder<FakeVolume>
    {
        private const long OneGigabyte = 1024 * 1024 * 1024;

        private long capacityInBytes = OneGigabyte;
        private long freeSpaceInBytes = OneGigabyte;
        private DriveType driveType = DriveType.Fixed;

        [NotNull]
        private string volumeLabel = string.Empty;

        [NotNull]
        private string driveFormat = "NTFS";

        public FakeVolume Build()
        {
            return new FakeVolume(capacityInBytes, freeSpaceInBytes, driveType, driveFormat, volumeLabel);
        }

        [NotNull]
        public FakeVolumeBuilder OfCapacity(long bytes)
        {
            capacityInBytes = bytes;
            return this;
        }

        [NotNull]
        public FakeVolumeBuilder WithFreeSpace(long bytes)
        {
            freeSpaceInBytes = bytes;
            return this;
        }

        [NotNull]
        public FakeVolumeBuilder OfType(DriveType type)
        {
            driveType = type;
            return this;
        }

        [NotNull]
        public FakeVolumeBuilder InFormat([NotNull] string format)
        {
            Guard.NotNull(format, nameof(format));
            driveFormat = format;
            return this;
        }

        [NotNull]
        public FakeVolumeBuilder Labeled([NotNull] string label)
        {
            Guard.NotNull(label, nameof(label));
            volumeLabel = label;
            return this;
        }
    }
}