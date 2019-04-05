using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryTimeUpdatesSpecs
    {
        [Fact]
        private void When_updating_directory_attributes_it_must_not_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Directory | FileAttributes.Hidden);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_updating_subdirectory_attributes_and_time_it_must_not_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string subdirectoryPath = @"c:\folder\sub";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(subdirectoryPath)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.SetAttributes(subdirectoryPath, FileAttributes.Directory | FileAttributes.ReadOnly);
            fileSystem.Directory.SetCreationTimeUtc(subdirectoryPath, updateTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_updating_file_attributes_and_time_it_must_not_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string subdirectoryPath = @"c:\folder\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(subdirectoryPath)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.SetCreationTimeUtc(subdirectoryPath, updateTimeUtc);
            fileSystem.File.SetAttributes(subdirectoryPath, FileAttributes.ReadOnly);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_creating_file_it_must_update_directory_timings()
        {
            // TODO: Split file I/O into various operations:
            // - Open existing file
            // - Read from file
            // - Write to file
            // - Append to file
            // - Truncate file
            // - Create new file
            // - Overwrite existing file

            // Arrange
            const string containerPath = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(containerPath)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.WriteAllText(@"c:\folder\file.txt", "X");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_updating_file_contents_it_must_not_update_directory_timings()
        {
            // TODO: Merge with above test.

            // Arrange
            const string containerPath = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.AppendAllText(@"c:\folder\file.txt", "X");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_deleting_file_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string filePath = @"c:\folder\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(containerPath)
                .IncludingEmptyFile(filePath)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Delete(filePath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_renaming_file_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string sourceFilePath = @"c:\folder\file.txt";
            const string destinationFilePath = @"c:\folder\newname.doc";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(containerPath)
                .IncludingEmptyFile(sourceFilePath)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Move(sourceFilePath, destinationFilePath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_copying_file_into_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string sourceFilePath = @"c:\other\file.txt";
            const string destinationFilePath = @"c:\folder\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(containerPath)
                .IncludingEmptyFile(sourceFilePath)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Copy(sourceFilePath, destinationFilePath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_copying_file_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string sourceFilePath = @"C:\folder\source.txt";
            const string destinationFilePath = @"C:\folder\target.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(sourceFilePath, "SourceContent")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Copy(sourceFilePath, destinationFilePath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_copying_file_overwriting_existing_file_it_must_not_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string sourceFilePath = @"C:\folder\source.txt";
            const string destinationFilePath = @"C:\folder\target.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(sourceFilePath, "SourceContent")
                .IncludingTextFile(destinationFilePath, "DestinationContent")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Copy(sourceFilePath, destinationFilePath, true);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_moving_file_into_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string sourceFilePath = @"c:\other\file.txt";
            const string destinationFilePath = @"c:\folder\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(containerPath)
                .IncludingEmptyFile(sourceFilePath)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Move(sourceFilePath, destinationFilePath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_moving_file_out_of_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string sourceFilePath = @"c:\folder\file.txt";
            const string destinationFilePath = @"c:\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(containerPath)
                .IncludingEmptyFile(sourceFilePath)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Move(sourceFilePath, destinationFilePath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        // TODO: Add test for container directory timings on File.Encrypt (and encrypt encrypted)
        // TODO: Add test for container directory timings on File.Decrypt (and decrypt unencrypted)

        // TODO: Add test for container directory timings on File.Replace (multiple variations)

        [Fact]
        private void When_creating_subdirectory_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string subdirectoryPath = @"c:\folder\sub";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(containerPath)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.CreateDirectory(subdirectoryPath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(subdirectoryPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(subdirectoryPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(subdirectoryPath).Should().Be(updateTimeUtc);

            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_deleting_subdirectory_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string subdirectoryPath = @"c:\folder\sub";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(subdirectoryPath)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.Delete(subdirectoryPath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_renaming_subdirectory_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string sourceDirectoryPath = @"c:\folder\sub";
            const string destinationDirectoryPath = @"c:\folder\newname";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(sourceDirectoryPath)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            fileSystem.Directory.SetLastWriteTimeUtc(sourceDirectoryPath, lastWriteTimeUtc);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            fileSystem.Directory.SetLastAccessTimeUtc(sourceDirectoryPath, lastAccessTimeUtc);

            DateTime updateTimeUtc = 20.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.Move(sourceDirectoryPath, destinationDirectoryPath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(destinationDirectoryPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(destinationDirectoryPath).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(destinationDirectoryPath).Should().Be(lastAccessTimeUtc);

            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_moving_subdirectory_into_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string sourceDirectoryPath = @"c:\other";
            const string destinationDirectoryPath = @"c:\folder\other";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(containerPath)
                .IncludingEmptyFile(@"c:\other\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            fileSystem.Directory.SetLastWriteTimeUtc(sourceDirectoryPath, lastWriteTimeUtc);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            fileSystem.Directory.SetLastAccessTimeUtc(sourceDirectoryPath, lastAccessTimeUtc);

            DateTime updateTimeUtc = 20.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.Move(sourceDirectoryPath, destinationDirectoryPath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(destinationDirectoryPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(destinationDirectoryPath).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(destinationDirectoryPath).Should().Be(lastAccessTimeUtc);

            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_moving_subdirectory_out_of_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string sourceDirectoryPath = @"c:\folder\sub";
            const string destinationDirectoryPath = @"c:\newname";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\sub\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            fileSystem.Directory.SetLastWriteTimeUtc(sourceDirectoryPath, lastWriteTimeUtc);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            fileSystem.Directory.SetLastAccessTimeUtc(sourceDirectoryPath, lastAccessTimeUtc);

            DateTime updateTimeUtc = 20.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.Move(sourceDirectoryPath, destinationDirectoryPath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(destinationDirectoryPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(destinationDirectoryPath).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(destinationDirectoryPath).Should().Be(lastAccessTimeUtc);

            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_updating_contents_of_subdirectory_it_must_not_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\sub\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Delete(@"c:\folder\sub\file.txt");
            fileSystem.File.WriteAllText(@"c:\folder\sub\new.txt", "X");
            fileSystem.File.SetAttributes(@"c:\folder\sub\new.txt", FileAttributes.Hidden);
            fileSystem.Directory.CreateDirectory(@"c:\folder\sub\more");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_enumerating_entries_it_must_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string subdirectoryPath = @"c:\folder\sub";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\sub\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.GetFileSystemEntries(containerPath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(subdirectoryPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(subdirectoryPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(subdirectoryPath).Should().Be(creationTimeUtc);

            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_enumerating_entries_it_must_recursively_update_directory_timings()
        {
            // Arrange
            const string containerPath = @"C:\folder";
            const string subdirectoryPath = @"c:\folder\sub";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\sub\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.GetFileSystemEntries(containerPath, searchOption: SearchOption.AllDirectories);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(subdirectoryPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(subdirectoryPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(subdirectoryPath).Should().Be(updateTimeUtc);

            fileSystem.Directory.GetCreationTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(containerPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(containerPath).Should().Be(updateTimeUtc);
        }
    }
}
