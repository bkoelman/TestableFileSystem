#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileEncryptSpecs
    {
        [Fact]
        private void When_encrypting_file_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.Encrypt(null);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        private void When_encrypting_file_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt(string.Empty);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_encrypting_file_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt(" ");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_encrypting_file_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_encrypting_file_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt("some?.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_encrypting_missing_local_file_it_must_fail()
        {
            // Arrange
            const string path = @"c:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt(path);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'c:\file.txt'.");
        }

        [Fact]
        private void When_encrypting_file_on_missing_drive_it_must_fail()
        {
            // Arrange
            const string path = @"z:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt(path);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'z:\folder\file.txt'.");
        }

        [Fact]
        private void When_encrypting_existing_local_file_on_fat16_volume_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("c:", new FakeVolumeInfoBuilder()
                    .InFormat("FAT16"))
                .IncludingTextFile(path, "Contents")
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt(path);

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage(
                "File encryption support only works on NTFS partitions.");
        }

        [Fact]
        private void When_encrypting_existing_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "Contents")
                .Build();

            // Act
            fileSystem.File.Encrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void When_encrypting_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\FILE.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "Contents")
                .Build();

            // Act
            fileSystem.File.Encrypt(@"c:\FOLDER\file.TXT");

            // Assert
            fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void When_encrypting_existing_local_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "Contents")
                .Build();

            // Act
            fileSystem.File.Encrypt(path + "  ");

            // Assert
            fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void When_encrypting_local_readonly_file_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "Contents", attributes: FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt(path);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The specified file is read only.");
        }

        [Fact]
        private void When_encrypting_local_file_that_is_in_use_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Encrypt(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage(
                    @"The process cannot access the file 'C:\some\file.txt' because it is being used by another process.");
            }
        }

        [Fact]
        private void When_encrypting_file_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingEmptyFile(@"C:\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.Encrypt(@"\file.txt");

            // Assert
            fileSystem.File.GetAttributes(@"C:\file.txt").Should().HaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void When_encrypting_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.Encrypt("file.txt");

            // Assert
            fileSystem.File.GetAttributes(@"C:\some\file.txt").Should().HaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void When_encrypting_local_file_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt(@"C:\some\subfolder");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\subfolder'.");
        }

        [Fact]
        private void When_encrypting_existing_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.Encrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void When_encrypting_local_readonly_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.File.Encrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.ReadOnly);
            fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void When_encrypting_local_file_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt(@"c:\some\file.txt\nested.txt");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt'.");
        }

        [Fact]
        private void When_encrypting_local_file_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt(@"c:\some\file.txt\nested.txt\more.txt");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt\more.txt'.");
        }

        [Fact]
        private void When_encrypting_remote_file_on_missing_network_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt(@"\\server\share\file.txt");

            action.Should().ThrowExactly<IOException>().WithMessage(
                "The filename, directory name, or volume label syntax is incorrect.");
        }

        [Fact]
        private void When_encrypting_remote_file_on_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.Encrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void When_encrypting_file_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Encrypt("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_encrypting_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\folder\file.txt")
                .Build();

            // Act
            fileSystem.File.Encrypt(@"\\?\C:\folder\file.txt");

            // Assert
            fileSystem.File.GetAttributes(@"C:\folder\file.txt").Should().HaveFlag(FileAttributes.Encrypted);
        }
    }
}
#endif
