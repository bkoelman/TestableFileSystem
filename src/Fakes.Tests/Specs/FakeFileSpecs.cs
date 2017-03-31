using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs
{
    public sealed class FakeFileSpecs
    {
        private static readonly DateTime ZeroFileTime = 1.January(1601).AsUtc().ToLocalTime();
        private static readonly DateTime ZeroFileTimeUtc = 1.January(1601).AsUtc();

        [Fact]
        private void When_getting_file_that_exists_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            bool found = fileSystem.File.Exists(path);

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_file_that_does_not_exist_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(@"C:\some\file.txt")
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"C:\some\other.txt");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_testing_if_null_file_exists_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.File.Exists(null);

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_creating_file_for_random_access_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            using (fileSystem.File.Create(@"c:\doc.txt", 1, FileOptions.RandomAccess))
            {
                // Assert
                fileSystem.File.GetAttributes(@"c:\doc.txt").Should().Be(FileAttributes.Normal);
            }
        }

        [Fact]
        private void When_creating_file_with_encryption_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(@"c:\doc.txt", 1, FileOptions.Encrypted);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'c:\doc.txt' is denied.");
        }

        [Fact]
        private void When_creating_file_with_delete_on_close_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(@"c:\doc.txt", 1, FileOptions.DeleteOnClose);

            // Assert
            action.ShouldThrow<NotImplementedException>().WithMessage("Option 'DeleteOnClose' is not supported.");
        }

        [Fact]
        private void When_creating_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            const string path = @"C:\some\file.txt";

            // Act
            using (fileSystem.File.Create(path))
            {
            }

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_creating_file_it_must_overwrite_existing()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path, "existing data")
                .Build();

            // Act
            using (var stream = fileSystem.File.Create(path))
            {
                // Assert
                stream.Length.Should().Be(0);
            }
        }

        [Fact]
        private void When_opening_existing_file_in_createNew_mode_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\sheet.xls";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.CreateNew);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The file 'C:\some\sheet.xls' already exists.");
        }

        [Fact]
        private void When_trying_to_open_existing_file_that_does_not_exist_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\")
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(@"C:\some\sheet.xls", FileMode.Open);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\sheet.xls'.");
        }

        [Fact]
        private void When_trying_to_open_existing_file_for_reading_it_must_fail_on_write()
        {
            // Arrange
            const string path = @"C:\some\sheet.xls";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            using (var stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Write(new byte[] { 0x22 }, 0, 1);

                // Assert
                action.ShouldThrow<NotSupportedException>();
            }
        }

        // TODO: Add tests for Copy.

        [Fact]
        private void When_moving_file_to_same_location_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
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
                .IncludingFile(sourcePath)
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
                .IncludingFile(sourcePath)
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
                .IncludingFile(sourcePath)
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
                .IncludingFile(sourcePath)
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
                .IncludingFile(sourcePath)
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
                .IncludingFile(sourcePath)
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
                .IncludingFile(sourcePath)
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
                .IncludingFile(sourcePath)
                .IncludingFile(destinationPath)
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
                .IncludingFile(sourcePath)
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
                .IncludingFile(sourcePath)
                .Build();

            using (fileSystem.File.Open(sourcePath, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

                // Assert
                action.ShouldThrow<IOException>().WithMessage("The process cannot access the file because it is being used by another process.");
                fileSystem.File.Exists(sourcePath).Should().BeTrue();
                fileSystem.File.Exists(destinationPath).Should().BeTrue();
            }
        }

        [Fact]
        private void When_deleting_file_that_exists_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.Delete(path);

            // Assert
            fileSystem.File.Exists(path).Should().BeFalse();
        }

        [Fact]
        private void When_deleting_file_that_does_not_exist_it_must_succeed()
        {
            // Arrange
            const string existingPath = @"C:\some\file.txt";
            const string missingPath = @"C:\some\other.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(existingPath)
                .Build();

            // Act
            fileSystem.File.Delete(missingPath);

            // Assert
            fileSystem.File.Exists(existingPath).Should().BeTrue();
        }

        [Fact]
        private void When_deleting_file_from_missing_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\other\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(path);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'C:\other\file.txt'.");
        }

        [Fact]
        private void When_deleting_file_that_is_in_use_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Delete(path);

                // Assert
                action.ShouldThrow<IOException>()
                    .WithMessage(@"The process cannot access the file 'C:\some\file.txt' because it is being used by another process.");
            }
        }

        [Fact]
        private void When_deleting_readonly_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path, null, FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(path);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\file.txt' is denied.");
        }

        [Fact]
        private void When_getting_file_attributes_for_mising_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"C:\some\file.txt");

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
        }

        [Fact]
        private void When_getting_file_attributes_for_mising_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"C:\some\file.txt");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'C:\some\file.txt'.");
        }

        [Fact]
        private void When_getting_file_creation_time_for_missing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            var creationTime = fileSystem.File.GetCreationTime(path);
            var creationTimeUtc = fileSystem.File.GetCreationTimeUtc(path);

            // Assert
            creationTime.Should().Be(ZeroFileTime);
            creationTimeUtc.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_file_creation_time_in_local_timezone_it_must_succeed()
        {
            // Arrange
            DateTime creationTime = 21.January(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetCreationTime(path, creationTime);

            // Assert
            fileSystem.File.GetCreationTime(path).Should().Be(creationTime);
            fileSystem.File.GetCreationTimeUtc(path).Should().NotBe(creationTime);
        }

        [Fact]
        private void When_setting_file_creation_time_in_utc_it_must_succeed()
        {
            // Arrange
            DateTime creationTimeUtc = 21.January(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetCreationTimeUtc(path, creationTimeUtc);

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetCreationTime(path).Should().NotBe(creationTimeUtc);
        }

        [Fact]
        private void When_getting_file_last_write_time_for_missing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            var lastWriteTime = fileSystem.File.GetLastWriteTime(path);
            var lastWriteTimeUtc = fileSystem.File.GetLastWriteTimeUtc(path);

            // Assert
            lastWriteTime.Should().Be(ZeroFileTime);
            lastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_file_last_write_time_in_local_timezone_it_must_succeed()
        {
            // Arrange
            DateTime lastWriteTime = 22.January(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTime(path, lastWriteTime);

            // Assert
            fileSystem.File.GetLastWriteTime(path).Should().Be(lastWriteTime);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().NotBe(lastWriteTime);
        }

        [Fact]
        private void When_setting_file_last_write_time_in_utc_it_must_succeed()
        {
            // Arrange
            DateTime lastWriteTimeUtc = 22.January(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.File.GetLastWriteTime(path).Should().NotBe(lastWriteTimeUtc);
        }

        [Fact]
        private void When_getting_file_last_access_time_for_missing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            var lastAccessTime = fileSystem.File.GetLastAccessTime(path);
            var lastAccessTimeUtc = fileSystem.File.GetLastAccessTimeUtc(path);

            // Assert
            lastAccessTime.Should().Be(ZeroFileTime);
            lastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_file_last_access_time_in_local_timezone_it_must_succeed()
        {
            // Arrange
            DateTime lastAccessTime = 23.January(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastAccessTime(path, lastAccessTime);

            // Assert
            fileSystem.File.GetLastAccessTime(path).Should().Be(lastAccessTime);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().NotBe(lastAccessTime);
        }

        [Fact]
        private void When_setting_file_last_access_time_in_utc_it_must_succeed()
        {
            // Arrange
            DateTime lastAccessTimeUtc = 23.January(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

            // Assert
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(lastAccessTimeUtc);
            fileSystem.File.GetLastAccessTime(path).Should().NotBe(lastAccessTimeUtc);
        }
    }
}
