using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoAttributeSpecs
    {
        [Fact]
        private void When_getting_file_attributes_it_must_lazy_load()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.Hidden)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Act
            FileAttributes attributes = fileInfo.Attributes;

            // Assert
            attributes.Should().Be(FileAttributes.ReadOnly);
        }

        [Fact]
        private void When_getting_file_attributes_it_must_cache()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.Hidden)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            FileAttributes beforeAttributes = fileInfo.Attributes;

            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Act
            FileAttributes afterAttributes = fileInfo.Attributes;

            // Assert
            beforeAttributes.Should().Be(FileAttributes.Hidden);
            afterAttributes.Should().Be(FileAttributes.Hidden);
        }

        [Fact]
        private void When_getting_file_attributes_after_external_change_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.Hidden)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            FileAttributes beforeAttributes = fileInfo.Attributes;

            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Act
            fileInfo.Refresh();

            // Assert
            FileAttributes afterAttributes = fileInfo.Attributes;

            beforeAttributes.Should().Be(FileAttributes.Hidden);
            afterAttributes.Should().Be(FileAttributes.ReadOnly);
        }

        [Fact]
        private void When_changing_file_attributes_it_must_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.Hidden)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            FileAttributes beforeAttributes = fileInfo.Attributes;

            // Act
            fileInfo.Attributes = FileAttributes.ReadOnly;

            // Assert
            FileAttributes afterAttributes = fileInfo.Attributes;

            beforeAttributes.Should().Be(FileAttributes.Hidden);
            afterAttributes.Should().Be(FileAttributes.ReadOnly);
        }
    }
}
