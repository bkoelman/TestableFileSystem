using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoOpenSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_opening_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";
            const string fileContents = "DEMO";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, fileContents)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            string contentsRead;

            // Act
            using (StreamReader reader = fileInfo.OpenText())
            {
                contentsRead = reader.ReadToEnd();
            }

            // Assert
            contentsRead.Should().Be(fileContents);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_opening_file_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            bool beforeFound = fileInfo.Exists;

            using (fileInfo.OpenWrite())
            {
            }

            // Act
            fileInfo.Refresh();

            // Assert
            bool afterFound = fileInfo.Exists;

            beforeFound.Should().BeFalse();
            afterFound.Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_opening_file_it_must_not_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            bool beforeFound = fileInfo.Exists;

            // Act
            using (fileInfo.OpenWrite())
            {
            }

            // Assert
            bool afterFound = fileInfo.Exists;

            beforeFound.Should().BeFalse();
            afterFound.Should().BeFalse();
        }
    }
}
