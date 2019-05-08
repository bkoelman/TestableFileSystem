using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoDeleteSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            dirInfo.Delete();

            // Assert
            fileSystem.Directory.Exists(path).Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_directory_recursively_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            dirInfo.Delete(true);

            // Assert
            fileSystem.Directory.Exists(path).Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_directory_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            bool beforeFound = dirInfo.Exists;

            dirInfo.Delete();

            // Act
            dirInfo.Refresh();

            // Assert
            bool afterFound = dirInfo.Exists;

            beforeFound.Should().BeTrue();
            afterFound.Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_directory_it_must_not_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path, FileAttributes.Hidden)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            bool beforeFound = dirInfo.Exists;

            // Act
            dirInfo.Delete();

            // Assert
            bool afterFound = dirInfo.Exists;

            beforeFound.Should().BeTrue();
            afterFound.Should().BeTrue();
        }
    }
}
