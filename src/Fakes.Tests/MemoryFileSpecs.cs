using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests
{
    public sealed class MemoryFileSpecs
    {
        [Fact]
        private void When_getting_file_that_exists_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(@"c:\doc.txt", 1, FileOptions.RandomAccess))
            {
                // Assert
                stream.Length.Should().Be(0);
            }
        }

        [Fact]
        private void When_creating_file_with_encryption_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(@"c:\doc.txt", 1, FileOptions.Encrypted);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Option 'Encrypted' is not supported.");
        }

        [Fact]
        private void When_creating_file_with_delete_on_close_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(@"c:\doc.txt", 1, FileOptions.DeleteOnClose);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Option 'DeleteOnClose' is not supported.");
        }

        [Fact]
        private void When_creating_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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
        // TODO: Add tests for Move.

        [Fact]
        private void When_deleting_file_that_exists_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Delete(path);

                action.ShouldThrow<IOException>()
                    .WithMessage(@"The process cannot access the file 'C:\some\file.txt' because it is being used by another process.");
            }
        }

        [Fact]
        private void When_getting_file_attributes_for_mising_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"C:\some\file.txt");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'C:\some\file.txt'.");
        }

        [Fact]
        private void When_setting_file_attributes_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            const FileAttributes attributes = FileAttributes.Archive | FileAttributes.Hidden;

            // Act
            fileSystem.File.SetAttributes(path, attributes);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(attributes);
        }

        [Fact]
        private void When_setting_file_creation_time_in_local_timezone_it_must_succeed()
        {
            // Arrange
            DateTime creationTime = 21.January(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetCreationTimeUtc(path, creationTimeUtc);

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetCreationTime(path).Should().NotBe(creationTimeUtc);
        }

        [Fact]
        private void When_setting_file_last_write_time_in_local_timezone_it_must_succeed()
        {
            // Arrange
            DateTime lastWriteTime = 21.January(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
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
            DateTime lastWriteTimeUtc = 21.January(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.File.GetLastWriteTime(path).Should().NotBe(lastWriteTimeUtc);
        }
    }
}
