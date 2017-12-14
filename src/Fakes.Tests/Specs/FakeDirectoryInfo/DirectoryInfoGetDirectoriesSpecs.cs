using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoGetDirectoriesSpecs
    {
        [Fact]
        private void When_getting_directories_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some\folder\sub")
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            IDirectoryInfo[] infos = dirInfo.GetDirectories();

            // Assert
            infos.Should().ContainSingle(x => x.FullName == @"c:\some\folder\sub");
        }
    }
}
