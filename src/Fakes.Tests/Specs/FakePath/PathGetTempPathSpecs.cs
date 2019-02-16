using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakePath
{
    public sealed class PathGetTempPathSpecs
    {
        [Fact]
        private void When_getting_default_temp_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder().Build();

            // Act
            string tempPath = fileSystem.Path.GetTempPath();

            // Assert
            tempPath.Should().Be(@"C:\Temp");
            fileSystem.Directory.Exists(@"c:\Temp").Should().BeTrue();
        }

        [Fact]
        private void When_getting_default_temp_path_without_drive_C_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithoutDefaultDriveC()
                .IncludingDirectory(@"x:\data")
                .Build();

            // Act
            string tempPath = fileSystem.Path.GetTempPath();

            // Assert
            tempPath.Should().Be(@"X:\Temp");
            fileSystem.Directory.Exists(@"x:\Temp").Should().BeTrue();
        }

        [Fact]
        private void When_getting_custom_temp_path_it_must_succeed()
        {
            // Arrange
            const string directory = @"x:\users\joe\temp";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithTempDirectory(directory)
                .Build();

            // Act
            string tempPath = fileSystem.Path.GetTempPath();

            // Assert
            tempPath.Should().Be(directory);
            fileSystem.Directory.Exists(directory).Should().BeTrue();
        }

        [Fact]
        private void When_getting_custom_missing_temp_path_it_must_succeed()
        {
            // Arrange
            const string directory = @"x:\users\joe\temp";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithTempDirectory(directory)
                .Build();

            fileSystem.Directory.Delete(directory);

            // Act
            string tempPath = fileSystem.Path.GetTempPath();

            // Assert
            tempPath.Should().Be(directory);
            fileSystem.Directory.Exists(directory).Should().BeFalse();
        }
    }
}
