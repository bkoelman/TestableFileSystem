using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryMoveSpecs
    {
        [Fact]
        private void When_moving_directory_for_null_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.Move(null, @"c:\newdir");

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        private void When_moving_directory_for_null_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.Move(@"c:\missing", null);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        private void When_moving_directory_for_empty_string_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(string.Empty, @"c:\newdir");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("Empty file name is not legal.*");
        }

        [Fact]
        private void When_moving_directory_for_empty_string_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"c:\missing", string.Empty);

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("Empty file name is not legal.*");
        }

        [Fact]
        private void When_moving_directory_for_whitespace_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(" ", @"c:\newdir");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_moving_directory_for_whitespace_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"c:\missing", " ");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_moving_directory_for_invalid_root_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move("::", @"c:\newdir");

            // Assert
            action.Should().Throw<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_moving_directory_for_invalid_root_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"c:\missing", "::");

            // Assert
            action.Should().Throw<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_moving_directory_for_invalid_characters_in_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"c:\dir?i", @"c:\newdir");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_moving_directory_for_invalid_characters_in_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"c:\missing", @"c:\dir?i");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_moving_directory_to_same_location_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"c:\existing-folder";
            const string destinationPath = @"c:\existing-folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            action.Should().Throw<IOException>().WithMessage(@"Source and destination path must be different.");
        }

        [Fact]
        private void When_moving_directory_to_same_location_with_different_casing_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"c:\existing-folder";
            const string destinationPath = @"c:\existing-FOLDER";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            action.Should().Throw<IOException>().WithMessage(@"Source and destination path must be different.");
        }

        [Fact]
        private void When_moving_directory_to_different_name_in_same_directory_it_must_rename()
        {
            // Arrange
            const string sourcePath = @"c:\some\existing-folder";
            const string destinationPath = @"c:\some\new-folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            // Act
            fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.Directory.Exists(sourcePath).Should().BeFalse();
            fileSystem.Directory.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_directory_that_contains_subtree_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\sourceFolder";
            const string destinationPath = @"c:\destinationFolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath + @"\file.txt")
                .IncludingEmptyFile(sourcePath + @"\nested\other.txt")
                .IncludingDirectory(sourcePath + @"\more\deeper")
                .Build();

            // Act
            fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.Directory.Exists(sourcePath).Should().BeFalse();
            fileSystem.Directory.Exists(destinationPath).Should().BeTrue();
            fileSystem.File.Exists(destinationPath + @"\file.txt").Should().BeTrue();
            fileSystem.File.Exists(destinationPath + @"\nested\other.txt").Should().BeTrue();
            fileSystem.Directory.Exists(destinationPath + @"\more\deeper").Should().BeTrue();
        }

        [Fact]
        private void When_moving_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\existing-folder";
            const string destinationPath = @"c:\new-folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            // Act
            fileSystem.Directory.Move(sourcePath + "  ", destinationPath + "  ");

            // Assert
            fileSystem.Directory.Exists(sourcePath).Should().BeFalse();
            fileSystem.Directory.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_directory_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"C:\source")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.Directory.Move(@"\source", @"\moved");

            // Assert
            fileSystem.Directory.Exists(@"C:\source").Should().BeFalse();
            fileSystem.Directory.Exists(@"C:\moved").Should().BeTrue();
        }

        [Fact]
        private void When_moving_directory_from_relative_path_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\some\folder";
            const string destinationPath = @"c:\other\folder\newname";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .IncludingDirectory(@"c:\other\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\");

            // Act
            fileSystem.Directory.Move(@"some\folder", destinationPath);

            // Assert
            fileSystem.Directory.Exists(sourcePath).Should().BeFalse();
            fileSystem.Directory.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_directory_to_relative_path_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"d:\some\folder";
            const string destinationPath = @"D:\other\node\newname";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .IncludingDirectory(@"D:\other\node")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            fileSystem.Directory.Move(sourcePath, @"node\newname");

            // Assert
            fileSystem.Directory.Exists(sourcePath).Should().BeFalse();
            fileSystem.Directory.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_relative_directory_on_different_drive_in_root_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\")
                .IncludingDirectory(@"D:\source")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            fileSystem.Directory.Move("D:source", "D:destination");

            // Assert
            fileSystem.Directory.Exists(@"D:\destination").Should().BeTrue();
        }

        [Fact]
        private void When_moving_relative_directory_on_same_drive_in_subdirectory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"D:\other\source")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            fileSystem.Directory.Move("D:source", "D:destination");

            // Assert
            fileSystem.Directory.Exists(@"d:\other\destination").Should().BeTrue();
        }

        [Fact]
        private void When_moving_directory_from_directory_that_exists_as_file_to_same_location_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(path, @"C:\SOME\FILE.TXT");

            // Assert
            action.Should().Throw<IOException>().WithMessage("Source and destination path must be different.");
        }

        [Fact]
        private void When_moving_directory_from_directory_that_exists_as_file_to_same_directory_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            fileSystem.Directory.Move(sourcePath, @"c:\some\newname");

            // Assert
            fileSystem.File.Exists(@"c:\some\newname").Should().BeTrue();
        }

        [Fact]
        private void When_moving_directory_from_directory_that_exists_as_file_to_other_directory_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            fileSystem.Directory.Move(sourcePath, @"c:\newname");

            // Assert
            fileSystem.File.Exists(@"c:\newname").Should().BeTrue();
        }

        [Fact]
        private void When_moving_directory_from_directory_that_exists_as_file_to_existing_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\other.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingEmptyFile(destinationPath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            action.Should().Throw<IOException>().WithMessage("Cannot create a file when that file already exists");
        }

        [Fact]
        private void When_moving_directory_from_directory_that_exists_as_file_to_missing_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\other\document.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            action.Should().Throw<DirectoryNotFoundException>().WithMessage("Could not find a part of the path.");
        }

        [Fact]
        private void When_moving_directory_from_directory_that_exists_as_open_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\other.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            using (fileSystem.File.Open(sourcePath, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

                // Assert
                action.Should().Throw<IOException>()
                    .WithMessage("The process cannot access the file because it is being used by another process.");
            }
        }

        [Fact]
        private void When_moving_directory_to_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string sourceDirectory = @"C:\some\folder";
            const string destinationFile = @"C:\other\file";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourceDirectory)
                .IncludingEmptyFile(destinationFile)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourceDirectory, destinationFile);

            // Assert
            action.Should().Throw<IOException>().WithMessage("Cannot create a file when that file already exists");
        }

        [Fact]
        private void When_moving_directory_from_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"C:\some\file.txt\other", "newname.doc");

            // Assert
            action.Should().Throw<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path.");
        }

        [Fact]
        private void When_moving_directory_to_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .IncludingEmptyFile(@"C:\some\newname.doc")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"C:\some\folder", @"C:\some\newname.doc\other");

            // Assert
            action.Should().Throw<IOException>().WithMessage("The parameter is incorrect");
        }

        [Fact]
        private void When_moving_directory_from_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"C:\some\file.txt\other.txt\missing", @"c:\newname");

            // Assert
            action.Should().Throw<DirectoryNotFoundException>().WithMessage("Could not find a part of the path.");
        }

        [Fact]
        private void When_moving_directory_to_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .IncludingEmptyFile(@"C:\some\newname.doc")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"C:\some\folder", @"C:\some\newname.doc\other.doc\more");

            // Assert
            action.Should().Throw<DirectoryNotFoundException>().WithMessage("Could not find a part of the path.");
        }

        [Fact]
        private void When_moving_directory_to_existing_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"c:\existing-folder";
            const string destinationPath = @"c:\new-folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .IncludingDirectory(destinationPath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            action.Should().Throw<IOException>().WithMessage(@"Cannot create a file when that file already exists");
        }

        [Fact]
        private void When_moving_directory_from_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"c:\source\missing-folder", @"c:\new-folder");

            // Assert
            action.Should().Throw<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\source\missing-folder'.");
        }

        [Fact]
        private void When_moving_directory_to_missing_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\folder";
            const string destinationPath = @"C:\missing\newname";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            action.Should().Throw<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path.");
        }

        [Fact]
        private void When_moving_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"C:\some\folder", @"c:\newname");

            // Assert
            action.Should().Throw<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\folder'.");
        }

        [Fact]
        private void When_moving_directory_to_parent_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\level\folder";
            const string destinationPath = @"C:\some\level";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            action.Should().Throw<IOException>().WithMessage(@"Cannot create a file when that file already exists");
        }

        [Fact]
        private void When_moving_directory_to_subdirectory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\folder";
            const string destinationPath = @"C:\some\folder\nested";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);
            action.Should().Throw<IOException>()
                .WithMessage("The process cannot access the file because it is being used by another process.");
        }

        [Fact]
        private void When_moving_directory_to_different_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"c:\existing-folder";
            const string destinationPath = @"e:\new-name";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            action.Should().Throw<IOException>()
                .WithMessage(@"Source and destination path must have identical roots. Move will not work across volumes.");
        }

        [Fact]
        private void When_moving_directory_that_is_root_of_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"e:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, @"e:\new-folder");

            // Assert
            action.Should().Throw<IOException>().WithMessage(@"Access to the path 'e:\' is denied.");
        }

        [Fact]
        private void When_moving_directory_to_root_of_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"c:\existing-folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, @"c:\");

            // Assert
            action.Should().Throw<IOException>()
                .WithMessage(@"The filename, directory name, or volume label syntax is incorrect");
        }

        [Fact]
        private void When_moving_directory_from_below_current_directory_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\store\current\folder";
            const string destinationPath = @"c:\new-folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\store\current");

            // Act
            fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.Directory.Exists(sourcePath).Should().BeFalse();
            fileSystem.Directory.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_directory_from_current_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\store\current\folder";
            const string destinationPath = @"c:\new-folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(sourcePath);

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            action.Should().Throw<IOException>()
                .WithMessage(@"The process cannot access the file because it is being used by another process.");
        }

        [Fact]
        private void When_moving_directory_from_remote_current_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"\\server\share\documents\teamA";
            const string destinationPath = @"\\server\share\new-folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(sourcePath);

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            action.Should().Throw<IOException>()
                .WithMessage(@"The process cannot access the file because it is being used by another process.");
        }

        [Fact]
        private void When_moving_directory_from_above_current_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"c:\store\current\folder";
            const string destinationPath = @"c:\new-folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(sourcePath);

            // Act
            Action action = () => fileSystem.Directory.Move(@"c:\store\current", destinationPath);

            // Assert
            action.Should().Throw<IOException>()
                .WithMessage(@"The process cannot access the file because it is being used by another process.");
        }

        [Fact]
        private void When_moving_directory_from_above_remote_current_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"\\server\share\documents\teamA";
            const string destinationPath = @"\\server\share\new-folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(sourcePath);

            // Act
            Action action = () => fileSystem.Directory.Move(@"\\server\share\documents", destinationPath);

            // Assert
            action.Should().Throw<IOException>()
                .WithMessage(@"The process cannot access the file because it is being used by another process.");
        }

        [Fact]
        private void When_moving_directory_with_readonly_file_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\existing-folder";
            const string destinationPath = @"c:\new-folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath + @"\file.txt", FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.Directory.Move(sourcePath.ToUpperInvariant(), destinationPath);

            // Assert
            fileSystem.Directory.Exists(sourcePath).Should().BeFalse();
            fileSystem.Directory.Exists(destinationPath).Should().BeTrue();
            fileSystem.File.Exists(destinationPath + @"\file.txt").Should().BeTrue();
            fileSystem.File.GetAttributes(destinationPath + @"\file.txt").Should().Be(FileAttributes.ReadOnly);
        }

        [Fact]
        private void When_moving_directory_that_contains_open_file_it_must_fail()
        {
            // Arrange
            const string sourceDirectory = @"c:\existing-folder";
            const string sourceFile = @"c:\existing-folder\sub\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourceFile)
                .Build();

            using (fileSystem.File.OpenRead(sourceFile))
            {
                // Act
                Action action = () => fileSystem.Directory.Move(sourceDirectory, @"c:\new-folder");

                // Assert
                action.Should().Throw<IOException>().WithMessage(@"Access to the path 'c:\existing-folder' is denied.");
            }
        }

        [Fact]
        private void When_moving_directory_on_missing_network_share_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"\\teamserver\documents\folder";
            const string destinationPath = @"\\teamserver\documents\moved";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            action.Should().Throw<IOException>().WithMessage("The network path was not found");
        }

        [Fact]
        private void When_moving_directory_on_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"\\teamserver\documents\folder";
            const string destinationPath = @"\\teamserver\documents\moved";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .IncludingDirectory(@"\\teamserver\documents")
                .Build();

            // Act
            fileSystem.Directory.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.Directory.Exists(sourcePath).Should().BeFalse();
            fileSystem.Directory.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_directory_from_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move("com1", @"c:\moved");

            // Assert
            action.Should().Throw<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_moving_directory_to_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Move(@"c:\src", "com1");

            // Assert
            action.Should().Throw<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_moving_directory_from_extended_path_to_extended_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\source")
                .Build();

            // Act
            fileSystem.Directory.Move(@"\\?\c:\source", @"\\?\c:\destination");

            // Assert
            fileSystem.Directory.Exists(@"c:\source").Should().BeFalse();
            fileSystem.Directory.Exists(@"c:\destination").Should().BeTrue();
        }
    }
}
