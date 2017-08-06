using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryDeleteSpecs
    {
        [Fact]
        private void When_deleting_directory_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.Delete(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_deleting_directory_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_deleting_directory_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_deleting_directory_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_deleting_directory_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"c:\dir?i");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_deleting_missing_local_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\some\folder");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'C:\some\folder'.");
        }

        [Fact]
        private void When_deleting_local_empty_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.Delete(path);

            // Assert
            fileSystem.Directory.Exists(path).Should().BeFalse();
        }

        [Fact]
        private void When_deleting_local_empty_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"C:\some\folder ");

            // Assert
            fileSystem.Directory.Exists(@"C:\some\folder").Should().BeFalse();
        }

        [Fact]
        private void When_deleting_local_nonempty_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\folder\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\some\folder");

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The directory is not empty.");
        }

        [Fact]
        private void When_deleting_local_readonly_directory_it_must_fail()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(path);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"Access to the path 'c:\some\folder' is denied.");
        }

        [Fact]
        private void When_deleting_local_nonempty_directory_recursively_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder\with\children")
                .IncludingEmptyFile(@"C:\some\folder\file.txt")
                .IncludingEmptyFile(@"C:\some\folder\child\other.txt")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"C:\some\folder", true);

            // Assert
            fileSystem.Directory.Exists(@"C:\some\folder").Should().BeFalse();
        }

        [Fact]
        private void When_deleting_local_nonempty_directory_recursively_that_contains_open_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\folder\deeper\file.txt")
                .Build();

            using (fileSystem.File.OpenRead(@"C:\some\folder\deeper\file.txt"))
            {
                // Act
                Action action = () => fileSystem.Directory.Delete(@"C:\some\folder", true);

                // Assert
                action.ShouldThrow<IOException>()
                    .WithMessage(
                        @"The process cannot access the file 'C:\some\folder\deeper\file.txt' because it is being used by another process.");
            }
        }

        [Fact]
        private void When_deleting_local_nonempty_directory_recursively_that_contains_readonly_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\folder\deeper\file.txt", FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\some\folder", true);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'file.txt' is denied.");
        }

        [Fact]
        private void When_deleting_local_nonempty_directory_recursively_that_contains_readonly_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder\deeper\subfolder", FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\some\folder", true);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"Access to the path 'C:\some\folder\deeper\subfolder' is denied.");
        }

        [Fact]
        private void When_deleting_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\")
                .IncludingDirectory(@"D:\")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            Action action = () => fileSystem.Directory.Delete(@"D:\");

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The directory is not empty.");
        }

        [Fact]
        private void When_deleting_drive_recursively_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\")
                .IncludingDirectory(@"D:\")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            Action action = () => fileSystem.Directory.Delete(@"D:\", true);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'd:\'.");
        }

        [Fact]
        private void When_deleting_below_current_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\store\current\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\store");

            // Act
            fileSystem.Directory.Delete(@"C:\store\current", true);

            // Assert
            fileSystem.Directory.Exists(@"C:\store\current").Should().BeFalse();
        }

        [Fact]
        private void When_deleting_current_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\store";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(path);

            // Act
            Action action = () => fileSystem.Directory.Delete(path, true);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"The process cannot access the file 'C:\store' because it is being used by another process.");
        }

        [Fact]
        private void When_deleting_remote_current_directory_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\documents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(path);

            // Act
            Action action = () => fileSystem.Directory.Delete(path, true);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(
                    @"The process cannot access the file '\\server\share\documents' because it is being used by another process.");
        }

        [Fact]
        private void When_deleting_above_current_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\store\current\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\store\current\folder");

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\store", true);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"The process cannot access the file 'C:\store' because it is being used by another process.");
        }

        [Fact]
        private void When_deleting_above_remote_current_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share\documents\teamA")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"\\server\share\documents\teamA");

            // Act
            Action action = () => fileSystem.Directory.Delete(@"\\server\share\documents", true);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(
                    @"The process cannot access the file '\\server\share\documents' because it is being used by another process.");
        }

        [Fact]
        private void When_deleting_above_root_of_drive_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\store")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"C:\..\..\store");

            // Assert
            fileSystem.Directory.Exists(@"C:\store").Should().BeFalse();
        }

        [Fact]
        private void When_deleting_directory_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"c:\other")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.Directory.Delete(@"\other");

            // Assert
            fileSystem.Directory.Exists(@"C:\other").Should().BeFalse();
        }

        [Fact]
        private void When_deleting_local_relative_empty_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\store\data")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\store");

            // Act
            fileSystem.Directory.Delete(@"data");

            // Assert
            fileSystem.Directory.Exists(@"c:\store\data").Should().BeFalse();
        }

        [Fact]
        private void When_deleting_local_empty_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\store\DATA")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"c:\STORE\data");

            // Assert
            fileSystem.Directory.Exists(@"c:\store\DATA").Should().BeFalse();
        }

        [Fact]
        private void When_deleting_local_directory_for_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(path);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The directory name is invalid.");
        }

        [Fact]
        private void When_deleting_local_directory_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"c:\some\file.txt\subfolder");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\some\file.txt\subfolder'.");
        }

        [Fact]
        private void When_deleting_local_directory_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"c:\some\file.txt\subfolder\deeper");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\some\file.txt\subfolder\deeper'.");
        }

        [Fact]
        private void When_deleting_remote_empty_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"\\teamshare\folder\documents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.Delete(path);

            // Assert
            fileSystem.Directory.Exists(path).Should().BeFalse();
        }

        [Fact]
        private void When_deleting_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\teamshare\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(path);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(
                    @"The process cannot access the file '\\teamshare\folder' because it is being used by another process.");
        }

        [Fact]
        private void When_deleting_network_share_recursively_it_must_fail()
        {
            // Arrange
            const string path = @"\\teamshare\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(path, true);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(
                    @"The process cannot access the file '\\teamshare\folder' because it is being used by another process.");
        }

        [Fact]
        private void When_deleting_directory_below_missing_network_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"\\server\share\team");

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The network path was not found");
        }

        [Fact]
        private void When_deleting_extended_local_empty_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"\\?\C:\some\folder");

            // Assert
            fileSystem.Directory.Exists(@"C:\some\folder").Should().BeFalse();
        }
    }
}
