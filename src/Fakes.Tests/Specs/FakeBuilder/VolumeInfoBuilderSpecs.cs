using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeBuilder
{
    public sealed class VolumeInfoBuilderSpecs
    {
        [Fact]
        private void When_building_with_defaults_it_must_succeed()
        {
            // Arrange
            var builder = new FakeVolumeInfoBuilder();

            // Act
            FakeVolumeInfo volume = builder.Build();

            // Assert
            volume.CapacityInBytes.Should().Be(1073741824);
            volume.FreeSpaceInBytes.Should().Be(1073741824);
            volume.Type.Should().Be(DriveType.Fixed);
            volume.Format.Should().Be("NTFS");
            volume.Label.Should().BeEmpty();
        }

        [Fact]
        private void When_setting_properties_with_free_space_it_must_succeed()
        {
            // Arrange
            var builder = new FakeVolumeInfoBuilder();

            // Act
            FakeVolumeInfo volume = builder
                .OfCapacity(2048)
                .WithFreeSpace(512)
                .OfType(DriveType.Ram)
                .InFormat("FAT16")
                .Labeled("DataDisk")
                .Build();

            // Assert
            volume.CapacityInBytes.Should().Be(2048);
            volume.FreeSpaceInBytes.Should().Be(512);
            volume.Type.Should().Be(DriveType.Ram);
            volume.Format.Should().Be("FAT16");
            volume.Label.Should().Be("DataDisk");
        }

        [Fact]
        private void When_setting_properties_with_used_space_it_must_succeed()
        {
            // Arrange
            var builder = new FakeVolumeInfoBuilder();

            // Act
            FakeVolumeInfo volume = builder
                .OfCapacity(2048)
                .WithUsedSpace(512)
                .OfType(DriveType.Ram)
                .InFormat("FAT16")
                .Labeled("DataDisk")
                .Build();

            // Assert
            volume.CapacityInBytes.Should().Be(2048);
            volume.FreeSpaceInBytes.Should().Be(1536);
            volume.Type.Should().Be(DriveType.Ram);
            volume.Format.Should().Be("FAT16");
            volume.Label.Should().Be("DataDisk");
        }

        [Fact]
        private void When_setting_free_space_it_must_override()
        {
            // Arrange
            var builder = new FakeVolumeInfoBuilder();

            // Act
            FakeVolumeInfo volume = builder
                .OfCapacity(2048)
                .WithUsedSpace(12345678)
                .WithFreeSpace(512)
                .Build();

            // Assert
            volume.CapacityInBytes.Should().Be(2048);
            volume.FreeSpaceInBytes.Should().Be(512);
        }

        [Fact]
        private void When_setting_used_space_it_must_override()
        {
            // Arrange
            var builder = new FakeVolumeInfoBuilder();

            // Act
            FakeVolumeInfo volume = builder
                .OfCapacity(2048)
                .WithFreeSpace(12345678)
                .WithUsedSpace(512)
                .Build();

            // Assert
            volume.CapacityInBytes.Should().Be(2048);
            volume.FreeSpaceInBytes.Should().Be(1536);
        }

        [Fact]
        private void When_setting_only_capacity_it_must_succeed()
        {
            // Arrange
            var builder = new FakeVolumeInfoBuilder();

            // Act
            FakeVolumeInfo volume = builder
                .OfCapacity(2048)
                .Build();

            // Assert
            volume.CapacityInBytes.Should().Be(2048);
            volume.FreeSpaceInBytes.Should().Be(2048);
        }

        [Fact]
        private void When_setting_null_format_it_must_fail()
        {
            // Arrange
            var builder = new FakeVolumeInfoBuilder();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => builder.InFormat(null);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        private void When_setting_null_label_it_must_fail()
        {
            // Arrange
            var builder = new FakeVolumeInfoBuilder();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => builder.Labeled(null);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        private void When_setting_negative_capacity_it_must_fail()
        {
            // Arrange
            FakeVolumeInfoBuilder builder = new FakeVolumeInfoBuilder()
                .OfCapacity(-1);

            // Act
            Action action = () => builder.Build();

            // Assert
            action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage("Volume capacity cannot be negative.*");
        }

        [Fact]
        private void When_setting_negative_free_space_it_must_fail()
        {
            // Arrange
            FakeVolumeInfoBuilder builder = new FakeVolumeInfoBuilder()
                .WithFreeSpace(-1);

            // Act
            Action action = () => builder.Build();

            // Assert
            action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage(
                "Available space cannot be negative or exceed volume capacity.*");
        }

        [Fact]
        private void When_setting_free_space_higher_than_available_space_it_must_fail()
        {
            // Arrange
            FakeVolumeInfoBuilder builder = new FakeVolumeInfoBuilder()
                .OfCapacity(1000)
                .WithFreeSpace(1001);

            // Act
            Action action = () => builder.Build();

            // Assert
            action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage(
                "Available space cannot be negative or exceed volume capacity.*");
        }

        [Fact]
        private void When_setting_negative_used_space_it_must_fail()
        {
            // Arrange
            FakeVolumeInfoBuilder builder = new FakeVolumeInfoBuilder()
                .WithUsedSpace(-1);

            // Act
            Action action = () => builder.Build();

            // Assert
            action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage(
                "Used space cannot be negative or exceed volume capacity.*");
        }

        [Fact]
        private void When_setting_used_space_higher_than_available_space_it_must_fail()
        {
            // Arrange
            FakeVolumeInfoBuilder builder = new FakeVolumeInfoBuilder()
                .OfCapacity(1000)
                .WithUsedSpace(1001);

            // Act
            Action action = () => builder.Build();

            // Assert
            action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage(
                "Used space cannot be negative or exceed volume capacity.*");
        }
    }
}
