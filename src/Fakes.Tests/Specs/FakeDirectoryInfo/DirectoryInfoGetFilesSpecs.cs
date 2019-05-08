using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoGetFilesSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_files_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\folder\file.txt")
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            IFileInfo[] infos = dirInfo.GetFiles();

            // Assert
            infos.Should().ContainSingle(x => x.FullName == @"c:\some\folder\file.txt");
        }
    }
}
