using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryCurrentSpecs
    {
        [Fact]
        private void When_setting_current_directory_to_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.SetCurrentDirectory(null);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        private void When_setting_current_directory_to_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(string.Empty);

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("Path cannot be the empty string or all whitespace.*");
        }

        [Fact]
        private void When_setting_current_directory_to_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(" ");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_setting_current_directory_to_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory("::");

            // Assert
            action.Should().Throw<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_setting_current_directory_to_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"c:\dir?i");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_setting_current_directory_to_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"C:\other\folder");

            // Assert
            action.Should().Throw<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'C:\other\folder'.");
        }

        [Fact]
        private void When_setting_current_directory_to_existing_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"c:\some");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"c:\some");
        }

        [Fact]
        private void When_setting_current_directory_to_existing_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"C:\some  ");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\some");
        }

        [Fact]
        private void When_setting_current_directory_to_missing_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"E:\");

            // Assert
            action.Should().Throw<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'E:\'.");
        }

        [Fact]
        private void When_setting_current_directory_to_existing_drive_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"C:");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\");
        }

        [Fact]
        private void When_setting_current_directory_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"c:\other")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\some");

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"\other");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"c:\other");
        }

        [Fact]
        private void When_setting_current_directory_to_relative_subdirectory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\some\");

            // Act
            fileSystem.Directory.SetCurrentDirectory(@".\folder");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"c:\some\folder");
        }

        [Fact]
        private void When_setting_current_directory_to_existing_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\SOME\folder")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"C:\some\FOLDER");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\some\FOLDER");
        }

        [Fact]
        private void When_setting_current_directory_below_missing_network_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"\\server\share\team");

            // Assert
            action.Should().Throw<IOException>().WithMessage(@"The network path was not found");
        }

        [Fact]
        private void When_setting_current_directory_to_existing_remote_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\docserver\teams")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"\\docserver\teams");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"\\docserver\teams");
        }

        [Fact]
        private void When_setting_current_directory_for_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(path);

            // Assert
            action.Should().Throw<IOException>().WithMessage("The directory name is invalid.");
        }

        [Fact]
        private void When_setting_current_directory_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"c:\some\file.txt\subfolder");

            // Assert
            action.Should().Throw<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\some\file.txt\subfolder'.");
        }

        [Fact]
        private void When_setting_current_directory_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"c:\some\file.txt\subfolder\deeper");

            // Assert
            action.Should().Throw<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\some\file.txt\subfolder\deeper'.");
        }

        [Fact]
        private void When_setting_current_directory_to_extended_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"\\?\C:\some\folder");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"\\?\C:\some\folder");
        }
    }
}
