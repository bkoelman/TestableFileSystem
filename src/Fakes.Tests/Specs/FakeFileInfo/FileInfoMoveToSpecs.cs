using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoMoveToSpecs
    {
        private const string DefaultContents = "ABC";

        [Fact]
        private void When_moving_file_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\some\file.txt";
            const string destinationPath = @"c:\other\renamed.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, DefaultContents, attributes: FileAttributes.ReadOnly)
                .IncludingDirectory(@"c:\other")
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(sourcePath);

            // Act
            fileInfo.MoveTo(destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.ReadAllText(destinationPath).Should().Be(DefaultContents);
            fileSystem.File.GetAttributes(destinationPath).Should().Be(FileAttributes.ReadOnly);
        }

        [Fact]
        private void When_renaming_file_it_must_update_properties()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            string beforeName = fileInfo.Name;

            // Act
            fileInfo.MoveTo(@"c:\some\renamed.md");

            // Assert
            beforeName.Should().Be("file.txt");
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Name.Should().Be("renamed.md");
            fileInfo.Extension.Should().Be(".md");
            fileInfo.DirectoryName.Should().Be(@"c:\some");
            fileInfo.Directory.ShouldNotBeNull().FullName.Should().Be(@"c:\some");
            fileInfo.FullName.Should().Be(@"c:\some\renamed.md");
            fileInfo.ToString().Should().Be(@"c:\some\renamed.md");
        }

        [Fact]
        private void When_moving_file_it_must_update_properties()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .IncludingDirectory(@"c:\other")
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            string beforeDirectoryName = fileInfo.DirectoryName;

            // Act
            fileInfo.MoveTo(@"c:\other\file.txt");

            // Assert
            beforeDirectoryName.Should().Be(@"c:\some");
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Name.Should().Be("file.txt");
            fileInfo.Extension.Should().Be(".txt");
            fileInfo.DirectoryName.Should().Be(@"c:\other");
            fileInfo.Directory.ShouldNotBeNull().FullName.Should().Be(@"c:\other");
            fileInfo.FullName.Should().Be(@"c:\other\file.txt");
            fileInfo.ToString().Should().Be(@"c:\other\file.txt");
        }
    }
}
