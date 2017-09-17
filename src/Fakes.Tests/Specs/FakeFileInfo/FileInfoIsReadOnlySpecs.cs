using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoIsReadOnlySpecs
    {
        [Fact]
        private void When_getting_file_readonly_state_it_must_lazy_load()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Act
            bool isReadOnly = fileInfo.IsReadOnly;

            // Assert
            isReadOnly.Should().BeTrue();
        }

        [Fact]
        private void When_getting_file_readonly_state_it_must_cache()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            bool beforeIsReadOnly = fileInfo.IsReadOnly;

            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Act
            bool afterIsReadOnly = fileInfo.IsReadOnly;

            // Assert
            beforeIsReadOnly.Should().BeFalse();
            afterIsReadOnly.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_readonly_state_after_external_change_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            bool beforeIsReadOnly = fileInfo.IsReadOnly;

            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Act
            fileInfo.Refresh();

            // Assert
            bool afterIsReadOnly = fileInfo.IsReadOnly;

            beforeIsReadOnly.Should().BeFalse();
            afterIsReadOnly.Should().BeTrue();
        }

        [Fact]
        private void When_changing_file_readonly_state_it_must_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            bool beforeIsReadOnly = fileInfo.IsReadOnly;

            // Act
            fileInfo.Attributes = FileAttributes.ReadOnly;

            // Assert
            bool afterIsReadOnly = fileInfo.IsReadOnly;

            beforeIsReadOnly.Should().BeFalse();
            afterIsReadOnly.Should().BeTrue();
        }
    }
}
