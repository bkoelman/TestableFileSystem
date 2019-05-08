using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoCopyToSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_copying_file_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\some\file.txt";
            const string destinationPath = @"c:\other\newname.txt";
            const string fileContents = "ABC";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, fileContents, attributes: FileAttributes.Hidden)
                .IncludingDirectory(@"c:\other")
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(sourcePath);

            // Act
            IFileInfo copyFileInfo = fileInfo.CopyTo(destinationPath);

            // Assert
            copyFileInfo.FullName.Should().Be(destinationPath);
            copyFileInfo.Exists.Should().BeTrue();

            fileSystem.File.ReadAllText(destinationPath).Should().Be(fileContents);
            fileSystem.File.GetAttributes(destinationPath).Should().Be(FileAttributes.Hidden);
        }
    }
}
