using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoGetEntriesSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_entries_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some\folder\sub")
                .IncludingEmptyFile(@"c:\some\folder\file.txt")
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            IFileSystemInfo[] infos = dirInfo.GetFileSystemInfos();

            // Assert
            IFileSystemInfo[] array = infos.Should().HaveCount(2).And.Subject.ToArray();
            array[0].FullName.Should().Be(@"c:\some\folder\file.txt");
            array[1].FullName.Should().Be(@"c:\some\folder\sub");
        }
    }
}
