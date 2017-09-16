using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoExistsSpecs
    {
        [Fact]
        private void When_getting_existence_it_must_lazy_load()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            fileSystem.File.WriteAllText(path, string.Empty);

            // Act
            bool found = fileInfo.Exists;

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_existence_it_must_cache()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            bool beforeFound = fileInfo.Exists;

            fileSystem.File.WriteAllText(path, string.Empty);

            // Act
            bool afterFound = fileInfo.Exists;

            // Assert
            beforeFound.Should().BeFalse();
            afterFound.Should().BeFalse();
        }

        [Fact]
        private void When_getting_existence_after_external_change_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            bool beforeFound = fileInfo.Exists;

            fileSystem.File.WriteAllText(path, string.Empty);

            // Act
            fileInfo.Refresh();

            // Assert
            bool afterFound = fileInfo.Exists;

            beforeFound.Should().BeFalse();
            afterFound.Should().BeTrue();
        }

        [Fact]
        private void When_changing_existence_it_must_not_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            bool beforeFound = fileInfo.Exists;

            // Act
            fileInfo.Delete();

            // Assert
            bool afterFound = fileInfo.Exists;

            beforeFound.Should().BeTrue();
            afterFound.Should().BeTrue();
        }
    }
}
