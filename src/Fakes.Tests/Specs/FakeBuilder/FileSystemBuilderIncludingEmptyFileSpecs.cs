﻿using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeBuilder
{
    public sealed class FileSystemBuilderIncludingEmptyFileSpecs
    {
        [Fact]
        private void When_including_empty_file_for_null_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => builder.IncludingEmptyFile(null);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        private void When_including_empty_file_for_empty_string_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingEmptyFile(string.Empty);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("'path' cannot be empty.*");
        }

        [Fact]
        private void When_including_empty_file_for_whitespace_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingEmptyFile(" ");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("'path' cannot contain only whitespace.*");
        }

        [Fact]
        private void When_including_empty_file_for_invalid_drive_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingEmptyFile("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_including_empty_file_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingEmptyFile("some?.txt");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_including_local_empty_file_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingEmptyFile(path)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Archive);
        }

        [Fact]
        private void When_including_local_empty_file_with_attributes_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingEmptyFile(path, FileAttributes.Hidden)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Hidden);
        }

        [Fact]
        private void When_including_existing_local_empty_file_it_must_overwrite()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "XYZ");

            // Act
            IFileSystem fileSystem = builder
                .IncludingEmptyFile(path)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Length.Should().Be(0);
        }

        [Fact]
        private void When_including_existing_local_empty_file_with_different_casing_it_must_overwrite()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingTextFile(@"C:\some\FILE.txt", "XYZ");

            // Act
            IFileSystem fileSystem = builder
                .IncludingEmptyFile(@"c:\SOME\file.TXT")
                .Build();

            // Assert
            fileSystem.File.Exists(@"C:\some\FILE.txt").Should().BeTrue();

            IFileInfo info = fileSystem.ConstructFileInfo(@"C:\some\FILE.txt");
            info.Length.Should().Be(0);
        }

        [Fact]
        private void When_including_local_empty_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingEmptyFile(@"d:\path\to\folder\readme.txt  ")
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_including_relative_local_empty_file_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingEmptyFile(@"some\file.txt");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_including_local_empty_file_that_is_drive_letter_it_must_fail()
        {
            // Arrange
            const string path = @"C:\";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingDirectory(path);

            // Act
            Action action = () => builder.IncludingEmptyFile(path);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'C:\'.");
        }

        [Fact]
        private void When_including_local_empty_file_for_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingDirectory(path);

            // Act
            Action action = () => builder.IncludingEmptyFile(path);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                @"Cannot create 'C:\some\subfolder' because a file or directory with the same name already exists.");
        }

        [Fact]
        private void When_including_local_empty_file_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt");

            // Act
            Action action = () => builder.IncludingEmptyFile(@"c:\some\file.txt\nested.txt");

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                @"Cannot create 'C:\some\file.txt' because a file or directory with the same name already exists.");
        }

        [Fact]
        private void When_including_local_empty_file_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt");

            // Act
            Action action = () => builder.IncludingEmptyFile(@"c:\some\file.txt\nested.txt\deeper.txt");

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                @"Cannot create 'C:\some\file.txt' because a file or directory with the same name already exists.");
        }

        [Fact]
        private void When_including_remote_empty_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\folder\file.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingEmptyFile(path)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_including_existing_remote_empty_file_it_must_overwrite()
        {
            // Arrange
            const string path = @"\\server\share\folder\file.txt";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "XYZ");

            // Act
            IFileSystem fileSystem = builder
                .IncludingEmptyFile(path)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Length.Should().Be(0);
        }

        [Fact]
        private void When_including_empty_file_for_reserved_name_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingEmptyFile("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_including_extended_local_empty_file_it_must_succeed()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingEmptyFile(@"\\?\d:\path\to\folder\readme.txt")
                .Build();

            // Assert
            fileSystem.File.Exists(@"D:\path\to\folder\readme.txt").Should().BeTrue();
            fileSystem.File.GetAttributes(@"D:\path\to\folder\readme.txt").Should().Be(FileAttributes.Archive);
        }
    }
}
