using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoEnumerateFilesSpecs
    {
        [Fact]
        private void When_enumerating_files_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\folder\file.txt")
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            IEnumerable<IFileInfo> infos = dirInfo.EnumerateFiles();

            // Assert
            infos.Should().ContainSingle(x => x.FullName == @"c:\some\folder\file.txt");
        }
    }
}
