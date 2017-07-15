using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FakeFileOpenSpecs
    {
        [Fact]
        private void When_opening_file_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.Open(null, FileMode.Create);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_opening_file_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(string.Empty, FileMode.Create);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Empty path name is not legal.*");
        }

        [Fact]
        private void When_opening_file_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(" ", FileMode.Create);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_opening_file_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Open("::", FileMode.Create);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_opening_file_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Open("some?.txt", FileMode.Create);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.");
        }

        // TODO: Add missing specs.

        [Fact]
        private void When_opening_existing_file_in_createNew_mode_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\sheet.xls";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
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
                .IncludingEmptyFile(path)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Write(new byte[] { 0x22 }, 0, 1);

                // Assert
                action.ShouldThrow<NotSupportedException>();
            }
        }

        [Fact]
        private void When_writer_is_active_it_must_fail_to_open()
        {
            // Arrange
            const string path = @"C:\some\sheet.xls";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Write))
            {
                // Act
                Action action = () => fileSystem.File.Open(path, FileMode.Open, FileAccess.Read);

                // Assert
                action.ShouldThrow<IOException>().WithMessage(
                    @"The process cannot access the file 'C:\some\sheet.xls' because it is being used by another process.");
            }
        }

        [Fact]
        private void When_readers_are_active_it_must_fail_to_open_for_writing()
        {
            // Arrange
            const string path = @"C:\some\sheet.xls";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    // Act
                    Action action = () => fileSystem.File.Open(path, FileMode.Open, FileAccess.Write);

                    // Assert
                    action.ShouldThrow<IOException>().WithMessage(
                        @"The process cannot access the file 'C:\some\sheet.xls' because it is being used by another process.");
                }
            }
        }
    }
}
