using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests
{
    public sealed class MemoryFileSpecs
    {
        [Fact]
        private void When_getting_file_that_exists_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingFile(@"C:\some\file.txt")
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"C:\some\file.txt");

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_file_that_does_not_exist_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingFile(@"C:\some\file.txt")
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"C:\some\other.txt");

            // Assert
            found.Should().BeFalse();
        }

        // TODO: Add tests for Create

        // TODO: Add tests for Open

        [Fact]
        private void When_deleting_file_that_exists_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingFile(@"C:\some\file.txt")
                .Build();

            // Act
            fileSystem.File.Delete(@"C:\some\other.txt");

            // Assert
            fileSystem.File.Exists(@"C:\some\other.txt").Should().BeFalse();
        }

        [Fact]
        private void When_deleting_file_that_does_not_exist_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingFile(@"C:\some\file.txt")
                .Build();

            // Act
            fileSystem.File.Delete(@"C:\some\other.txt");

            fileSystem.File.Exists(@"C:\some\file.txt").Should().BeTrue();
        }

        [Fact]
        private void When_deleting_file_from_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(@"C:\other\file.txt");

            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\other\file.txt'.");
        }

        [Fact]
        private void When_deleting_file_that_is_in_use_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Delete(path);

                action.ShouldThrow<IOException>()
                    .WithMessage(@"The process cannot access the file 'C:\some\file.txt' because it is being used by another process.");
            }
        }
    }
}
