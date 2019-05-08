using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoMoveToSpecs
    {
        private const string DefaultContents = "ABC";

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_directory_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\some\folder";
            const string destinationPath = @"c:\other\renamed";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(@"c:\some\folder\file.txt", DefaultContents, attributes: FileAttributes.ReadOnly)
                .IncludingDirectory(@"c:\other")
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(sourcePath);

            // Act
            dirInfo.MoveTo(destinationPath);

            // Assert
            fileSystem.Directory.Exists(sourcePath).Should().BeFalse();
            fileSystem.Directory.Exists(destinationPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_renaming_directory_it_must_update_properties()
        {
            // Arrange
            const string sourcePath = @"c:\some\folder";
            const string destinationPath = @"c:\some\renamed";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(sourcePath);

            string beforeName = dirInfo.Name;

            // Act
            dirInfo.MoveTo(destinationPath);

            // Assert
            beforeName.Should().Be("folder");
            dirInfo.Exists.Should().BeTrue();
            dirInfo.Name.Should().Be("renamed");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(destinationPath);
            dirInfo.ToString().Should().Be(destinationPath);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"c:\some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"c:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_directory_it_must_update_properties()
        {
            // Arrange
            const string sourcePath = @"c:\some\folder";
            const string destinationPath = @"c:\other\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .IncludingDirectory(@"c:\other")
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(sourcePath);

            string beforeDirectoryName = dirInfo.Parent.ShouldNotBeNull().FullName;

            // Act
            dirInfo.MoveTo(destinationPath);

            // Assert
            beforeDirectoryName.Should().Be(@"c:\some");
            dirInfo.Exists.Should().BeTrue();
            dirInfo.Name.Should().Be("folder");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(destinationPath);
            dirInfo.ToString().Should().Be(destinationPath);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"c:\other");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"c:\");
        }
    }
}
