#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileDecryptSpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_decrypting_file_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.File.Decrypt(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory(Skip = "TODO: Activate when running on .NET CORE 3")]
        [CanRunOnFileSystem]
        private void When_decrypting_file_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Decrypt(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory(Skip = "TODO: Activate when running on .NET CORE 3")]
        [CanRunOnFileSystem]
        private void When_decrypting_file_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Decrypt(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_file_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Decrypt("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_file_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Decrypt("some?.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_missing_local_file_it_must_fail()
        {
            // Arrange
            const string path = @"c:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Decrypt(path);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'c:\file.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_file_on_missing_drive_it_must_fail()
        {
            // Arrange
            const string path = @"z:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Decrypt(path);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'z:\folder\file.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_unencrypted_local_file_on_fat16_volume_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("c:", new FakeVolumeInfoBuilder()
                    .InFormat("FAT16"))
                .IncludingTextFile(path, "Contents")
                .Build();

            // Act
            fileSystem.File.Decrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_existing_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "Contents")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Decrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\FILE.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "Contents")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Decrypt(@"c:\FOLDER\file.TXT");

            // Assert
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_existing_local_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "Contents")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Decrypt(path + "  ");

            // Assert
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_local_readonly_file_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "Contents")
                .Build();

            fileSystem.File.Decrypt(path);
            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Act
            Action action = () => fileSystem.File.Decrypt(path);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The specified file is read only.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_local_file_that_is_in_use_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            fileSystem.File.Encrypt(path);

            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Decrypt(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage(
                    @"The process cannot access the file 'C:\some\file.txt' because it is being used by another process.");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_file_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingEmptyFile(@"C:\file.txt")
                .Build();

            fileSystem.File.Encrypt(@"C:\file.txt");

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.Decrypt(@"\file.txt");

            // Assert
            fileSystem.File.GetAttributes(@"C:\file.txt").Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            fileSystem.File.Encrypt(@"C:\some\file.txt");

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.Decrypt("file.txt");

            // Assert
            fileSystem.File.GetAttributes(@"C:\some\file.txt").Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_local_file_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Decrypt(@"C:\some\subfolder");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\subfolder'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_existing_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Decrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_local_readonly_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path, FileAttributes.ReadOnly)
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Decrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.ReadOnly);
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_local_file_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Decrypt(@"c:\some\file.txt\nested.txt");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_local_file_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Decrypt(@"c:\some\file.txt\nested.txt\more.txt");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt\more.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_remote_file_on_missing_network_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Decrypt(@"\\server\share\file.txt");

            action.Should().ThrowExactly<IOException>().WithMessage("The network path was not found.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_remote_file_on_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Decrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_file_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Decrypt("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_extended_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Decrypt(@"\\?\C:\folder\file.txt");

            // Assert
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }
    }
}

#endif
