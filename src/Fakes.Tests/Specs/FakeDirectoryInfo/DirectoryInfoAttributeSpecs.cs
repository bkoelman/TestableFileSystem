using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoAttributeSpecs
    {
        [Fact]
        private void When_getting_directory_attributes_it_must_lazy_load()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path, FileAttributes.Hidden)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Act
            FileAttributes attributes = dirInfo.Attributes;

            // Assert
            attributes.Should().Be(FileAttributes.Directory | FileAttributes.ReadOnly);
        }

        [Fact]
        private void When_getting_directory_attributes_it_must_cache()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path, FileAttributes.Hidden)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            FileAttributes beforeAttributes = dirInfo.Attributes;

            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Act
            FileAttributes afterAttributes = dirInfo.Attributes;

            // Assert
            beforeAttributes.Should().Be(FileAttributes.Directory | FileAttributes.Hidden);
            afterAttributes.Should().Be(FileAttributes.Directory | FileAttributes.Hidden);
        }

        [Fact]
        private void When_getting_directory_attributes_after_external_change_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path, FileAttributes.Hidden)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            FileAttributes beforeAttributes = dirInfo.Attributes;

            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Act
            dirInfo.Refresh();

            // Assert
            FileAttributes afterAttributes = dirInfo.Attributes;

            beforeAttributes.Should().Be(FileAttributes.Directory | FileAttributes.Hidden);
            afterAttributes.Should().Be(FileAttributes.Directory | FileAttributes.ReadOnly);
        }

        [Fact]
        private void When_changing_directory_attributes_it_must_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path, FileAttributes.Hidden)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            FileAttributes beforeAttributes = dirInfo.Attributes;

            // Act
            dirInfo.Attributes = FileAttributes.ReadOnly;

            // Assert
            FileAttributes afterAttributes = dirInfo.Attributes;

            beforeAttributes.Should().Be(FileAttributes.Directory | FileAttributes.Hidden);
            afterAttributes.Should().Be(FileAttributes.Directory | FileAttributes.ReadOnly);
        }
    }
}
