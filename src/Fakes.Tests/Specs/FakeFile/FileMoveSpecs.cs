using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileMoveSpecs
    {
        [Fact]
        private void When_moving_file_for_null_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.Move(null, @"c:\destination.txt");

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_moving_file_for_null_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.Move(@"c:\source.txt", null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_moving_file_for_empty_string_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(string.Empty, @"c:\destination.txt");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Empty file name is not legal.*");
        }

        [Fact]
        private void When_moving_file_for_empty_string_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"c:\source.txt", string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Empty file name is not legal.*");
        }

        [Fact]
        private void When_moving_file_for_whitespace_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(" ", @"c:\destination.txt");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_moving_file_for_whitespace_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"c:\source.txt", " ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_moving_file_for_invalid_source_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move("::", @"c:\destination.txt");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_moving_file_for_invalid_destination_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"c:\source.txt", "::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_moving_file_for_invalid_characters_in_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move("some?.txt", @"c:\destination.txt");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_moving_file_for_invalid_characters_in_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"c:\source.txt", "some?.txt");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

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
        private void When_moving_file_to_same_location_with_different_casing_it_must_rename()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\FILE.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            string[] files = fileSystem.Directory.GetFiles(@"C:\some");
            files.Should().ContainSingle(x => x == destinationPath);
        }

        [Fact]
        private void When_moving_file_to_different_name_in_same_directory_it_must_rename()
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
        private void When_moving_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\old.txt")
                .Build();

            // Act
            fileSystem.File.Move(@"C:\some\old.txt  ", @"C:\some\new.txt  ");

            // Assert
            fileSystem.File.Exists(@"C:\some\old.txt").Should().BeFalse();
            fileSystem.File.Exists(@"C:\some\new.txt").Should().BeTrue();
        }

        [Fact]
        private void When_moving_file_from_relative_path_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"D:\other\folder\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"D:\other\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            fileSystem.File.Move(@"some\file.txt", destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_file_to_relative_path_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"D:\other\folder\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"D:\other\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            fileSystem.File.Move(sourcePath, @"folder\newname.doc");

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_relative_file_on_different_drive_in_root_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\")
                .IncludingDirectory(@"D:\other")
                .IncludingEmptyFile(@"D:\source.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            fileSystem.File.Move("D:source.txt", "D:destination.txt");

            // Assert
            fileSystem.File.Exists(@"D:\destination.txt").Should().BeTrue();
        }

        [Fact]
        private void When_moving_relative_file_on_same_drive_in_subfolder_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"D:\other\source.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            fileSystem.File.Move("D:source.txt", "D:destination.txt");

            // Assert
            fileSystem.File.Exists(@"d:\other\destination.txt").Should().BeTrue();
        }

        [Fact]
        private void When_moving_file_from_file_that_exists_as_directory_it_must_fail()
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

        [Fact]
        private void When_moving_file_to_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationDirectory = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(destinationDirectory)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationDirectory);

            // Assert
            action.ShouldThrow<IOException>().WithMessage("Cannot create a file when that file already exists.");
        }

        [Fact]
        private void When_moving_file_from_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt\other.txt", "newname.doc");

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt\other.txt'.");
        }

        [Fact]
        private void When_moving_file_to_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .IncludingEmptyFile(@"C:\some\newname.doc")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt", @"C:\some\newname.doc\other.doc");

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The parameter is incorrect");
        }

        [Fact]
        private void When_moving_file_from_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt\other.txt\missing.txt", "newname.doc");

            // Assert
            action.ShouldThrow<FileNotFoundException>()
                .WithMessage(@"Could not find file 'C:\some\file.txt\other.txt\missing.txt'.");
        }

        [Fact]
        private void When_moving_file_to_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .IncludingEmptyFile(@"C:\some\newname.doc")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt", @"C:\some\newname.doc\other.doc\more.doc");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage("Could not find a part of the path.");
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
        private void When_moving_file_from_missing_directory_it_must_fail()
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
        private void When_moving_file_to_missing_directory_it_must_fail()
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
        private void When_moving_missing_file_it_must_fail()
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
        private void When_moving_file_to_parent_directory_it_must_succeed()
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
        private void When_moving_file_to_subdirectory_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\sub\newname.txt";

            var creationTimeUtc = 7.October(2015);
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"c:\some\sub")
                .Build();

            var lastWriteTimeUtc = 8.October(2015);
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(sourcePath, "ABC");

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();

            var destinationInfo = fileSystem.ConstructFileInfo(destinationPath);
            destinationInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            destinationInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);
            destinationInfo.LastAccessTimeUtc.Should().Be(lastWriteTimeUtc);
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
        private void When_moving_file_that_is_root_of_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"d:\")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\", @"d:\newname.doc");

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\'.");
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
            action.ShouldThrow<IOException>().WithMessage("The filename, directory name, or volume label syntax is incorrect");
        }

        [Fact]
        private void When_moving_readonly_file_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"D:\other\folder\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath, FileAttributes.ReadOnly)
                .IncludingDirectory(@"D:\other\folder")
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_an_open_file_to_same_drive_it_must_fail()
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
                fileSystem.File.Exists(destinationPath).Should().BeFalse();
            }
        }

        [Fact]
        private void When_moving_an_open_file_to_different_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"X:\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"X:\")
                .Build();

            using (fileSystem.File.Open(sourcePath, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

                // Assert
                action.ShouldThrow<IOException>()
                    .WithMessage("The process cannot access the file because it is being used by another process.");
                fileSystem.File.Exists(sourcePath).Should().BeTrue();
                fileSystem.File.Exists(destinationPath).Should().BeFalse();
            }
        }

        [Fact]
        private void When_moving_file_from_missing_network_share_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"\\teamserver\documents\for-all.txt";
            const string destinationPath = @"C:\docs\mine.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\docs")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file '\\teamserver\documents\for-all.txt'.");
        }

        [Fact]
        private void When_moving_file_to_missing_network_share_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\docs\mine.txt";
            const string destinationPath = @"\\teamserver\documents\for-all.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The network path was not found");
        }

        [Fact]
        private void When_moving_file_from_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"\\teamserver\documents\for-all.txt";
            const string destinationPath = @"C:\docs\mine.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"C:\docs")
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_moving_file_to_existing_network_share_it_must_succeed()
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
        private void When_moving_file_from_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move("com1", @"c:\new.txt");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_moving_file_to_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"c:\old.txt", "com1");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_moving_file_from_extended_path_to_extended_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"\\server\share\summary.doc")
                .IncludingDirectory(@"c:\work")
                .Build();

            // Act
            fileSystem.File.Move(@"\\?\UNC\server\share\summary.doc", @"\\?\c:\work\summary.doc");

            // Assert
            fileSystem.File.Exists(@"\\server\share\summary.doc").Should().BeFalse();
            fileSystem.File.Exists(@"c:\work\summary.doc").Should().BeTrue();
        }
    }
}
