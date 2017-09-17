using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoCreateSpecs
    {
        [Fact]
        private void When_creating_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Act
            using (fileInfo.CreateText())
            {
            }

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
            fileSystem.File.ReadAllText(path).Should().Be(string.Empty);
        }

        [Fact]
        private void When_creating_file_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            bool beforeFound = fileInfo.Exists;

            using (fileInfo.CreateText())
            {
            }

            // Act
            fileInfo.Refresh();

            // Assert
            bool afterFound = fileInfo.Exists;

            beforeFound.Should().BeFalse();
            afterFound.Should().BeTrue();
        }

        [Fact]
        private void When_creating_file_it_must_not_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            bool beforeFound = fileInfo.Exists;

            // Act
            using (fileInfo.CreateText())
            {
            }

            // Assert
            bool afterFound = fileInfo.Exists;

            beforeFound.Should().BeFalse();
            afterFound.Should().BeFalse();
        }
    }
}
