﻿using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FakeFileCreateSpecs
    {
        [Fact]
        private void When_creating_file_for_random_access_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            using (fileSystem.File.Create(@"c:\doc.txt", 1, FileOptions.RandomAccess))
            {
                // Assert
                fileSystem.File.Exists(@"c:\doc.txt").Should().BeTrue();
            }
        }

        [Fact]
        private void When_creating_file_with_encryption_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(@"c:\doc.txt", 1, FileOptions.Encrypted);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'c:\doc.txt' is denied.");
        }

        [Fact]
        private void When_creating_file_with_delete_on_close_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\doc.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            using (fileSystem.File.Create(path, 1, FileOptions.DeleteOnClose))
            {
                // Assert
                fileSystem.File.Exists(path).Should().BeTrue();
            }

            fileSystem.File.Exists(path).Should().BeFalse();
        }

        [Fact]
        private void When_creating_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            const string path = @"C:\some\file.txt";

            // Act
            using (fileSystem.File.Create(path))
            {
            }

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_creating_file_it_must_overwrite_existing()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path, "existing data")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(path))
            {
                // Assert
                stream.Length.Should().Be(0);
            }
        }

        [Fact]
        private void When_creating_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(path);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\subfolder' is denied.");
        }

        // TODO: Add missing specs.
    }
}
