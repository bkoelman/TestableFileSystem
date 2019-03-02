using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeBuilder
{
    public sealed class VolumeBuilderSpecs
    {
        [Fact]
        private void When_building_with_defaults_it_must_succeed()
        {
            // Arrange
            var builder = new FakeVolumeBuilder();

            // Act
            FakeVolume volume = builder.Build();

            // Assert
            volume.CapacityInBytes.Should().Be(1073741824);
            volume.FreeSpaceInBytes.Should().Be(1073741824);
            volume.Type.Should().Be(DriveType.Fixed);
            volume.Format.Should().Be("NTFS");
            volume.Label.Should().BeEmpty();
        }

        [Fact]
        private void When_setting_properties_it_must_succeed()
        {
            // Arrange
            var builder = new FakeVolumeBuilder();

            // Act
            FakeVolume volume = builder
                .OfCapacity(1024)
                .WithFreeSpace(512)
                .OfType(DriveType.Ram)
                .InFormat("FAT16")
                .Labeled("DataDisk")
                .Build();

            // Assert
            volume.CapacityInBytes.Should().Be(1024);
            volume.FreeSpaceInBytes.Should().Be(512);
            volume.Type.Should().Be(DriveType.Ram);
            volume.Format.Should().Be("FAT16");
            volume.Label.Should().Be("DataDisk");
        }

        [Fact]
        private void When_setting_null_format_it_must_fail()
        {
            // Arrange
            var builder = new FakeVolumeBuilder();

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
            var builder = new FakeVolumeBuilder();

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
            FakeVolumeBuilder builder = new FakeVolumeBuilder()
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
            FakeVolumeBuilder builder = new FakeVolumeBuilder()
                .WithFreeSpace(-1);

            // Act
            Action action = () => builder.Build();

            // Assert
            action.Should().ThrowExactly<ArgumentOutOfRangeException>()
                .WithMessage("Available space cannot be negative or exceed volume capacity.*");
        }

        [Fact]
        private void When_setting_free_space_higher_than_available_space_it_must_fail()
        {
            // Arrange
            FakeVolumeBuilder builder = new FakeVolumeBuilder()
                .OfCapacity(1000)
                .WithFreeSpace(1001);

            // Act
            Action action = () => builder.Build();

            // Assert
            action.Should().ThrowExactly<ArgumentOutOfRangeException>()
                .WithMessage("Available space cannot be negative or exceed volume capacity.*");
        }
    }
}
