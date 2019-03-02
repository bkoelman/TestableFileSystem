using System;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeBuilder
{
    public sealed class FileSystemBuilderIncludingVolumeSpecs
    {
        [Fact]
        private void When_including_volume_for_null_name_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => builder.IncludingVolume(null, new FakeVolumeBuilder());

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        private void When_including_volume_for_null_volume_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => builder.IncludingVolume("c:", (FakeVolume)null);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        private void When_including_volume_for_null_builder_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => builder.IncludingVolume("c:", (FakeVolumeBuilder)null);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        private void When_including_volume_for_empty_string_name_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingVolume(string.Empty, new FakeVolumeBuilder());

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("'name' cannot be empty or contain only whitespace.*");
        }

        [Fact]
        private void When_including_volume_for_whitespace_name_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingVolume(" ", new FakeVolumeBuilder());

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("'name' cannot be empty or contain only whitespace.*");
        }

        [Theory]
        [InlineData("C")]
        [InlineData("_:")]
        [InlineData(@"?:\")]
        [InlineData(@"c:\some")]
        [InlineData(@"some\folder")]
        [InlineData(@"\\server\share\folder")]
        private void When_including_volume_for_invalid_name_it_must_fail([NotNull] string driveName)
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingVolume(driveName, new FakeVolumeBuilder());

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Theory]
        [InlineData("F:")]
        [InlineData(@"F:\")]
        [InlineData(@"\\?\F:\")]
        [InlineData(@"\\server\share")]
        [InlineData(@"\\?\UNC\server\share")]
        private void When_including_volume_for_drive_or_network_share_it_must_succeed([NotNull] string driveName)
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingVolume(driveName, new FakeVolumeBuilder())
                .Build();

            // Assert
            fileSystem.Directory.Exists(driveName).Should().BeTrue();
        }

        [Fact]
        private void When_including_volume_for_only_server_part_of_network_share_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingVolume(@"\\fileserver", new FakeVolumeBuilder());

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(@"The UNC path should be of the form \\server\share.");
        }

        [Fact]
        private void When_including_volume_for_reserved_name_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingVolume(@"COM1", new FakeVolumeBuilder());

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }
    }
}
