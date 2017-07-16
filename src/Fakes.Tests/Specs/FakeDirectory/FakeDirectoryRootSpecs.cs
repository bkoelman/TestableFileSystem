using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class FakeDirectoryRootSpecs
    {
        [Fact]
        private void When_getting_directory_root_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.GetDirectoryRoot(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_getting_directory_root_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetDirectoryRoot(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_getting_directory_root_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetDirectoryRoot(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_getting_directory_root_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetDirectoryRoot("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_getting_directory_root_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetDirectoryRoot(@"c:\dir?i");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.");
        }

        [Fact]
        private void When_getting_directory_root_for_missing_local_drive_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"e:\");

            // Assert
            root.Should().Be(@"e:\");
        }

        [Fact]
        private void When_getting_directory_root_for_missing_local_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"d:\some\folder\path");

            // Assert
            root.Should().Be(@"d:\");
        }

        [Fact]
        private void When_getting_directory_root_for_missing_local_path_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"f:\some\folder\file.txt  ");

            // Assert
            root.Should().Be(@"f:\");
        }

        [Fact]
        private void When_getting_directory_root_for_relative_local_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"x:\folder\to\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"x:\folder\to");

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot("file.txt");

            // Assert
            root.Should().Be(@"x:\");
        }

        [Fact]
        private void When_getting_directory_root_for_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"\\server\share\file.txt");

            // Assert
            root.Should().Be(@"\\server\share");
        }

        [Fact]
        private void When_getting_directory_root_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetDirectoryRoot("COM1");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_getting_directory_root_for_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\folder")
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"\\?\C:\folder\file.txt");

            // Assert
            root.Should().Be(@"\\?\C:\");
        }

        [Fact]
        private void When_getting_directory_root_for_extended_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"\\?\UNC\server\share\folder\file.txt");

            // Assert
            root.Should().Be(@"\\?\UNC\server\share");
        }
    }
}
