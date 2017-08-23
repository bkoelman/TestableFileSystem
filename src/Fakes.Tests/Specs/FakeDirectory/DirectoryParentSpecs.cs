using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryParentSpecs
    {
        [Fact]
        private void When_getting_directory_parent_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.GetParent(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_getting_directory_parent_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetParent(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Path cannot be the empty string or all whitespace.*");
        }

        [Fact]
        private void When_getting_directory_parent_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetParent(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_getting_directory_parent_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetParent("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_getting_directory_parent_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetParent(@"c:\dir?i");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_getting_directory_parent_for_missing_local_drive_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"e:\");

            // Assert
            parent.Should().BeNull();
        }

        [Fact]
        private void When_getting_directory_parent_for_missing_local_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"d:\some\folder\path");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"d:\some\folder");
        }

        [Fact]
        private void When_getting_directory_parent_for_missing_local_path_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"f:\some\folder\file.txt  ");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"f:\some\folder");
        }

        [Fact]
        private void When_getting_directory_parent_for_directory_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"C:\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\some");

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\folder");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"c:\");
        }

        [Fact]
        private void When_getting_directory_parent_for_relative_local_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"X:\folder\to\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"x:\folder\to");

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent("file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"x:\folder\to");
        }

        [Fact]
        private void When_getting_directory_parent_for_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(path);

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"C:\some");
        }

        [Fact]
        private void When_getting_directory_parent_for_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"C:\some\file.txt\deeper");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"C:\some\file.txt");
        }

        [Fact]
        private void When_getting_directory_parent_for_parent_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"C:\some\file.txt\deeper\more");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"C:\some\file.txt\deeper");
        }

        [Fact]
        private void When_getting_directory_parent_on_missing_network_share_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\\server\share\file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"\\server\share");
        }

        [Fact]
        private void When_getting_directory_parent_for_missing_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\\server\share\file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"\\server\share");
        }

        [Fact]
        private void When_getting_directory_parent_for_existing_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\\server\share\file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"\\server\share");
        }

        [Fact]
        private void When_getting_directory_parent_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetParent("COM1");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_getting_directory_parent_for_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\folder")
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\\?\C:\folder\file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"\\?\C:\folder");
        }

        [Fact]
        private void When_getting_directory_parent_for_extended_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\\?\UNC\server\share\folder\file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"\\?\UNC\server\share\folder");
        }
    }
}
