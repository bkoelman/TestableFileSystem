using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FakeFileMoveSpecs
    {
        [Fact]
        private void When_moving_file_to_same_location_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.Move(path, path);

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_moving_file_to_same_location_with_different_casing_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\FILE1.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            // TODO: Write assertion that checks for casing change. Requires enumeration support, which is not yet implemented.
        }

        [Fact]
        private void When_moving_file_to_different_name_in_same_folder_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_file_to_parent_folder_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\level\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.Move(sourcePath, "renamed.nfo");

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(@"C:\some\renamed.nfo").Should().BeTrue();
        }

        [Fact]
        private void When_moving_file_to_different_drive_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"D:\other\folder\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"D:\other\folder")
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_file_to_root_of_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, @"C:\");

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The filename, directory name, or volume label syntax is incorrect.");
        }

        [Fact]
        private void When_moving_file_to_network_share_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\docs\mine.txt";
            const string destinationPath = @"\\teamserver\documents\for-all.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"\\teamserver\documents")
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_file_that_does_not_exist_it_must_fail()
        {
            // Arrange

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt", "newname.doc");

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
        }

        [Fact]
        private void When_moving_file_from_directory_that_does_not_exist_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt", @"C:\some\newname.doc");

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
        }

        [Fact]
        private void When_moving_from_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\", @"C:\some\newname.doc");

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\'.");
        }

        [Fact]
        private void When_moving_file_to_directory_that_does_not_exist_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\other\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path.");
        }

        [Fact]
        private void When_moving_file_to_existing_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingEmptyFile(destinationPath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            action.ShouldThrow<IOException>().WithMessage("Cannot create a file when that file already exists.");
        }

        [Fact]
        private void When_moving_file_to_existing_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationDirectory = @"C:\some\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(destinationDirectory)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationDirectory);

            // Assert
            action.ShouldThrow<IOException>().WithMessage("Cannot create a file when that file already exists.");
        }

        [Fact(Skip = "TODO")]
        private void When_moving_an_open_file_it_must_copy_and_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            using (fileSystem.File.Open(sourcePath, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

                // Assert
                action.ShouldThrow<IOException>()
                    .WithMessage("The process cannot access the file because it is being used by another process.");
                fileSystem.File.Exists(sourcePath).Should().BeTrue();
                fileSystem.File.Exists(destinationPath).Should().BeTrue();
            }
        }

        [Fact]
        private void When_moving_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\subfolder")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\subfolder", "newname.doc");

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\subfolder'.");
        }

        // TODO: Add missing specs.
    }
}
