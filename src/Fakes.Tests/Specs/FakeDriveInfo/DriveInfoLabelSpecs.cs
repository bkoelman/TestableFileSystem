#if !NETCOREAPP1_1
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDriveInfo
{
    public sealed class DriveInfoLabelSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_volume_label_it_must_store_value()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("x:", new FakeVolumeInfoBuilder()
                    .Labeled("StartName"))
                .Build();

            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("x:");

            // Act
            driveInfo.VolumeLabel = "NextName";

            // Assert
            fileSystem.ConstructDriveInfo("x:").VolumeLabel.Should().Be("NextName");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_volume_label_to_null_it_must_store_empty_string()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("x:", new FakeVolumeInfoBuilder()
                    .Labeled("StartName"))
                .Build();

            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("x:");

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            driveInfo.VolumeLabel = null;

            // Assert
            fileSystem.ConstructDriveInfo("x:").VolumeLabel.Should().BeEmpty();
        }
    }
}
#endif
