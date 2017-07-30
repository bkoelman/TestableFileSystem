using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileOpenSpecs
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

        [Fact]
        private void When_opening_existing_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "X")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(1);
            }
        }

        [Fact]
        private void When_opening_missing_local_file_it_must_fail()
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
        private void When_opening_missing_local_file_in_Truncate_mode_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\")
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(@"C:\some\sheet.xls", FileMode.Truncate);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\sheet.xls'.");
        }

        [Fact]
        private void When_opening_existing_local_file_in_Truncate_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ABC")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Truncate))
            {
                stream.WriteByte(0x20);
            }

            // Assert
            fileSystem.ConstructFileInfo(path).Length.Should().Be(1);
            fileSystem.File.ReadAllText(path).Should().Be(" ");
        }

        [Fact]
        private void When_opening_existing_local_file_in_Append_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ABC")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Append))
            {
                byte[] buffer = { (byte)'X', (byte)'Y', (byte)'Z' };
                stream.Write(buffer, 0, buffer.Length);
            }

            // Assert
            fileSystem.ConstructFileInfo(path).Length.Should().Be(6);
            fileSystem.File.ReadAllText(path).Should().Be("ABCXYZ");
        }

        [Fact]
        private void When_opening_existing_local_file_in_CreateNew_mode_it_must_fail()
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
        private void When_opening_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(@"C:\some\FILE.txt", "ABC")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(@"c:\SOME\file.TXT", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(3);
            }
        }

        [Fact]
        private void When_opening_existing_local_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(@"C:\some\file.txt", "ABC")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(@"C:\some\file.txt  ", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(3);
            }
        }

        [Fact]
        private void When_opening_existing_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(@"C:\some\FILE.txt", "ABC")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            using (IFileStream stream = fileSystem.File.Open("file.txt", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(3);
            }
        }

        [Fact]
        private void When_opening_existing_relative_local_file_on_different_drive_in_subfolder_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .IncludingEmptyFile(@"D:\other\child.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");
            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            Action action = () => fileSystem.File.Open("D:child.txt", FileMode.Open);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'D:\child.txt'.");
        }

        [Fact]
        private void When_opening_existing_relative_local_file_on_different_drive_in_root_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .IncludingDirectory(@"D:\other")
                .IncludingTextFile(@"D:\child.txt", "ABC")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");
            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            using (IFileStream stream = fileSystem.File.Open("D:child.txt", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(3);
            }
        }

        [Fact]
        private void When_opening_existing_relative_local_file_on_same_drive_in_subfolder_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(@"D:\other\child.txt", "ABC")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            using (IFileStream stream = fileSystem.File.Open("D:child.txt", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(3);
            }
        }

        [Fact]
        private void When_opening_existing_relative_local_file_on_same_drive_in_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"D:\other")
                .IncludingEmptyFile(@"D:\child.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            Action action = () => fileSystem.File.Open("D:child.txt", FileMode.Open);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'D:\other\child.txt'.");
        }

        [Fact]
        private void When_opening_local_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.Open);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\subfolder' is denied.");
        }

        [Fact]
        private void When_opening_local_file_that_exists_as_parent_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(@"C:\some\file.txt\nested.txt", FileMode.Open);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\file.txt\nested.txt'.");
        }

        [Fact]
        private void When_opening_local_file_in_CreateNew_mode_below_existing_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(@"c:\some\file.txt\nested.txt", FileMode.CreateNew);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\some\file.txt\nested.txt'.");
        }

        [Fact]
        private void When_opening_local_file_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(@"C:\some\doc.txt", FileMode.Open);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\doc.txt'.");
        }

        [Fact]
        private void When_opening_file_on_missing_network_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(@"\\server\share\file.txt", FileMode.Open);

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The network path was not found");
        }

        [Fact]
        private void When_opening_missing_remote_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(@"\\server\share\file.txt", FileMode.Open);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file '\\server\share\file.txt'.");
        }

        [Fact]
        private void When_opening_existing_remote_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ABC")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(3);
            }
        }

        [Fact]
        private void When_opening_file_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Open("COM1", FileMode.Open);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_opening_existing_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(@"C:\folder\file.txt", "ABC")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(@"\\?\C:\folder\file.txt", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(3);
            }
        }
    }
}
