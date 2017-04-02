using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FakeFileDeleteSpecs
    {
        [Fact]
        private void When_deleting_file_that_exists_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.Delete(path);

            // Assert
            fileSystem.File.Exists(path).Should().BeFalse();
        }

        [Fact]
        private void When_deleting_file_that_does_not_exist_it_must_succeed()
        {
            // Arrange
            const string existingPath = @"C:\some\file.txt";
            const string missingPath = @"C:\some\other.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(existingPath)
                .Build();

            // Act
            fileSystem.File.Delete(missingPath);

            // Assert
            fileSystem.File.Exists(existingPath).Should().BeTrue();
        }

        [Fact]
        private void When_deleting_file_from_missing_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\other\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(path);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\other\file.txt'.");
        }

        [Fact]
        private void When_deleting_file_that_is_in_use_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Delete(path);

                // Assert
                action.ShouldThrow<IOException>()
                    .WithMessage(
                        @"The process cannot access the file 'C:\some\file.txt' because it is being used by another process.");
            }
        }

        [Fact]
        private void When_deleting_readonly_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path, null, FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(path);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\file.txt' is denied.");
        }

        [Fact]
        private void When_deleting_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(path);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\subfolder' is denied.");
        }
    }
}
