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
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\folder\sub")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.SetAttributes(@"c:\folder\sub", FileAttributes.Directory | FileAttributes.ReadOnly);
            fileSystem.Directory.SetCreationTimeUtc(@"c:\folder\sub", updateTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_updating_file_attributes_and_time_it_must_not_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.SetCreationTimeUtc(@"c:\folder\file.txt", updateTimeUtc);
            fileSystem.File.SetAttributes(@"c:\folder\file.txt", FileAttributes.ReadOnly);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
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
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.WriteAllText(@"c:\folder\file.txt", "X");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_updating_file_contents_it_must_not_update_directory_timings()
        {
            // TODO: Merge with above test.

            // Arrange
            const string path = @"C:\folder";

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
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_deleting_file_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Delete(@"c:\folder\file.txt");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_renaming_file_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Move(@"c:\folder\file.txt", @"c:\folder\newname.doc");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_copying_file_into_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string directoryPath = @"C:\folder";
            const string sourceFilePath = @"c:\other\file.txt";
            const string destinationFilePath = @"c:\folder\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(directoryPath)
                .IncludingEmptyFile(sourceFilePath)
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Copy(sourceFilePath, destinationFilePath);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(directoryPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(directoryPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(directoryPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_copying_file_it_must_update_directory_timings()
        {
            // Arrange
            const string directoryPath = @"C:\folder";
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
            fileSystem.Directory.GetCreationTimeUtc(directoryPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(directoryPath).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(directoryPath).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_copying_file_overwriting_existing_file_it_must_not_update_directory_timings()
        {
            // Arrange
            const string directoryPath = @"C:\folder";
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
            fileSystem.Directory.GetCreationTimeUtc(directoryPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(directoryPath).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(directoryPath).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_moving_file_into_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .IncludingEmptyFile(@"c:\other\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Move(@"c:\other\file.txt", @"c:\folder\file.txt");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_moving_file_out_of_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.File.Move(@"c:\folder\file.txt", @"c:\file.txt");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);
        }

        // TODO: Add test for container directory timings on File.Encrypt (and encrypt encrypted)
        // TODO: Add test for container directory timings on File.Decrypt (and decrypt unencrypted)

        // TODO: Add test for container directory timings on File.Replace (multiple variations)

        [Fact]
        private void When_creating_subdirectory_it_must_update_directory_timings()
        {
            // TODO: Assert on timings of both container and subdirectory.

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
            fileSystem.Directory.CreateDirectory(@"c:\folder\sub");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_deleting_subdirectory_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\folder\sub")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.Delete(@"c:\folder\sub");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_renaming_subdirectory_it_must_update_directory_timings()
        {
            // TODO: Assert on timings of both container and subdirectory.

            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\folder\sub")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.Move(@"c:\folder\sub", @"c:\folder\newname");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_moving_subdirectory_into_directory_it_must_update_directory_timings()
        {
            // TODO: Assert on timings of both container and subdirectory.

            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .IncludingEmptyFile(@"c:\other\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.Move(@"c:\other", @"c:\folder\other");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_moving_subdirectory_out_of_directory_it_must_update_directory_timings()
        {
            // TODO: Assert on timings of both container and subdirectory.

            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\sub\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.Move(@"c:\folder\sub", @"c:\newname");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(updateTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);
        }

        [Fact]
        private void When_updating_contents_of_subdirectory_it_must_not_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

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
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_enumerating_entries_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\sub\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.GetFileSystemEntries(path);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);

            fileSystem.Directory.GetCreationTimeUtc(@"c:\folder\sub").Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(@"c:\folder\sub").Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(@"c:\folder\sub").Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_enumerating_entries_it_must_recursively_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\sub\file.txt")
                .Build();

            DateTime updateTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => updateTimeUtc;

            // Act
            fileSystem.Directory.GetFileSystemEntries(path, searchOption: SearchOption.AllDirectories);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(updateTimeUtc);

            fileSystem.Directory.GetCreationTimeUtc(@"c:\folder\sub").Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(@"c:\folder\sub").Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(@"c:\folder\sub").Should().Be(updateTimeUtc);
        }
    }
}
