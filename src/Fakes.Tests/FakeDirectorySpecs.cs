using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests
{
    public sealed class FakeDirectorySpecs
    {
        [Fact]
        private void When_getting_directory_that_exists_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"C:\some\folder");

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_directory_that_does_not_exist_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"C:\other\folder");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_deleting_empty_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"C:\some\folder");

            // Assert
            fileSystem.Directory.Exists(@"C:\some\folder").Should().BeFalse();
        }

        [Fact]
        private void When_deleting_nonempty_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(@"C:\some\folder\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\some\folder");

            // Assert
            action.ShouldThrow<Exception>().WithMessage("The directory is not empty.");
        }

        [Fact]
        private void When_deleting_readonly_directory_it_must_fail()
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
        private void When_deleting_nonempty_directory_recursively_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder\with\children")
                .IncludingFile(@"C:\some\folder\file.txt")
                .IncludingFile(@"C:\some\folder\child\other.txt")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"C:\some\folder", true);

            // Assert
            fileSystem.Directory.Exists(@"C:\some\folder").Should().BeFalse();
        }

        [Fact]
        private void When_deleting_nonempty_directory_recursively_that_contains_open_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(@"C:\some\folder\deeper\file.txt")
                .Build();

            using (fileSystem.File.OpenRead(@"C:\some\folder\deeper\file.txt"))
            {
                // Act
                Action action = () => fileSystem.Directory.Delete(@"C:\some\folder", true);

                // Assert
                action.ShouldThrow<IOException>().WithMessage(@"The process cannot access the file 'C:\some\folder\deeper\file.txt' because it is being used by another process.");
            }
        }

        [Fact]
        private void When_deleting_nonempty_directory_recursively_that_contains_readonly_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(@"C:\some\folder\deeper\file.txt", null, FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\some\folder", true);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'file.txt' is denied.");
        }

        [Fact]
        private void When_deleting_nonempty_directory_recursively_that_contains_readonly_directory_it_must_fail()
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
        private void When_deleting_current_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\store")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\store");

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\store", true);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The process cannot access the file 'C:\store' because it is being used by another process.");
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
            action.ShouldThrow<IOException>().WithMessage(@"The process cannot access the file 'C:\store' because it is being used by another process.");
        }

        [Fact]
        private void When_setting_current_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\some");
        }

        [Fact]
        private void When_setting_relative_current_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some\");

            // Act
            fileSystem.Directory.SetCurrentDirectory(@".\folder");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\some\folder");
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
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'E:\'.");
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
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'C:\other\folder'.");
        }

        [Fact]
        private void When_setting_current_directory_to_network_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\docserver\teams")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"\\docserver\teams");

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"The specified path is invalid.");
        }
    }
}
