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
            Action action = () => builder.IncludingVolume(null, new FakeVolumeInfoBuilder());

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
            Action action = () => builder.IncludingVolume("c:", (FakeVolumeInfo)null);

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
            Action action = () => builder.IncludingVolume("c:", (FakeVolumeInfoBuilder)null);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        private void When_including_volume_for_empty_string_name_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingVolume(string.Empty, new FakeVolumeInfoBuilder());

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("'name' cannot be empty.*");
        }

        [Fact]
        private void When_including_volume_for_whitespace_name_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingVolume(" ", new FakeVolumeInfoBuilder());

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("'name' cannot contain only whitespace.*");
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
            Action action = () => builder.IncludingVolume(driveName, new FakeVolumeInfoBuilder());

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
                .IncludingVolume(driveName, new FakeVolumeInfoBuilder())
                .Build();

            // Assert
            fileSystem.Directory.Exists(driveName).Should().BeTrue();
        }

        [Fact]
        private void When_including_volume_for_network_host_without_share_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingVolume(@"\\fileserver", new FakeVolumeInfoBuilder());

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(@"The UNC path should be of the form \\server\share.");
        }

        [Fact]
        private void When_including_volume_for_reserved_name_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingVolume(@"COM1", new FakeVolumeInfoBuilder());

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_overwriting_volume_it_must_fail()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder());

            // Act
            Action action = () => builder.IncludingVolume("c:", new FakeVolumeInfoBuilder());

            // Assert
            action.Should().ThrowExactly<InvalidOperationException>().WithMessage("Volume 'c:' has already been created.");
        }

        [Fact]
        private void When_overwriting_implicit_volume_it_must_fail()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\folder\file.txt");

            // Act
            Action action = () => builder.IncludingVolume("c:", new FakeVolumeInfoBuilder());

            // Assert
            action.Should().ThrowExactly<InvalidOperationException>().WithMessage("Volume 'c:' has already been created.");
        }
    }
}
