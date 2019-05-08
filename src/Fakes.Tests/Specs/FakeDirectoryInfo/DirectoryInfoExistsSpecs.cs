using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoExistsSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_existence_it_must_lazy_load()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            fileSystem.Directory.CreateDirectory(path);

            // Act
            bool found = dirInfo.Exists;

            // Assert
            found.Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_existence_it_must_cache()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            bool beforeFound = dirInfo.Exists;

            fileSystem.Directory.CreateDirectory(path);

            // Act
            bool afterFound = dirInfo.Exists;

            // Assert
            beforeFound.Should().BeFalse();
            afterFound.Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_existence_after_external_change_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            bool beforeFound = dirInfo.Exists;

            fileSystem.Directory.CreateDirectory(path);

            // Act
            dirInfo.Refresh();

            // Assert
            bool afterFound = dirInfo.Exists;

            beforeFound.Should().BeFalse();
            afterFound.Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_changing_directory_existence_it_must_not_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
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
