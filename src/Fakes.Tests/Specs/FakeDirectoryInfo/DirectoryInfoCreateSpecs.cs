using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoCreateSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\some\folder\nested";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"d:\")
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            dirInfo.Create();

            // Assert
            fileSystem.Directory.Exists(path).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_directory_it_must_not_update_properties()
        {
            // Arrange
            const string path = @"d:\some\folder\nested";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"d:\")
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            bool beforeExists = dirInfo.Exists;

            // Act
            dirInfo.Create();

            // Assert
            beforeExists.Should().BeFalse();
            dirInfo.Exists.Should().BeFalse();
        }
    }
}
