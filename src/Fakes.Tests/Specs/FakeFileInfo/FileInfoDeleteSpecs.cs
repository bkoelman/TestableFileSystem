using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoDeleteSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Act
            fileInfo.Delete();

            // Assert
            fileSystem.File.Exists(path).Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_file_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            bool beforeFound = fileInfo.Exists;

            fileInfo.Delete();

            // Act
            fileInfo.Refresh();

            // Assert
            bool afterFound = fileInfo.Exists;

            beforeFound.Should().BeTrue();
            afterFound.Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_file_it_must_not_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.Hidden)
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
