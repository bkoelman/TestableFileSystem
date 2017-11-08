using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileOpenSpecs
    {
        private const string DefaultContents = "ABC";

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
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_opening_file_in_CreateNew_mode_with_Read_access_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.CreateNew, FileAccess.Read);

            // Assert
            action.ShouldThrow<ArgumentException>()
                .WithMessage(@"Combining FileMode: CreateNew with FileAccess: Read is invalid.*");
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
        private void When_opening_missing_local_file_in_CreateNew_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.CreateNew))
            {
                // Assert
                stream.Length.Should().Be(0);
            }

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Exists.Should().BeTrue();
            info.Length.Should().Be(0);
            info.Attributes.Should().Be(FileAttributes.Archive);
        }

        [Fact]
        private void When_opening_file_in_Create_mode_with_Read_access_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.Create, FileAccess.Read);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage(@"Combining FileMode: Create with FileAccess: Read is invalid.*");
        }

        [Fact]
        private void When_opening_existing_local_file_in_Create_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Create))
            {
                // Assert
                stream.Length.Should().Be(0);
            }
        }

        [Fact]
        private void When_opening_existing_local_hidden_file_in_Create_mode_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents, attributes: FileAttributes.Hidden)
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.Create);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\file.txt' is denied.");
        }

        [Fact]
        private void When_opening_existing_local_readonly_file_in_Create_mode_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents, attributes: FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.Create);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\file.txt' is denied.");
        }

        [Fact]
        private void When_opening_missing_local_file_in_Create_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Create))
            {
                // Assert
                stream.Length.Should().Be(0);
            }

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Exists.Should().BeTrue();
            info.Length.Should().Be(0);
            info.Attributes.Should().Be(FileAttributes.Archive);
        }

        [Fact]
        private void When_opening_existing_local_file_in_Open_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open))
            {
                stream.ReadByte();

                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
            }

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Exists.Should().BeTrue();
        }

        [Fact]
        private void When_opening_existing_local_readonly_file_in_Open_mode_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents, attributes: FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.Open);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\file.txt' is denied.");
        }

        [Fact]
        private void When_opening_existing_local_readonly_file_in_Open_mode_with_Read_access_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents, attributes: FileAttributes.ReadOnly)
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
            }
        }

        [Fact]
        private void When_opening_missing_local_file_in_Open_mode_it_must_fail()
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
        private void When_opening_existing_local_file_in_OpenOrCreate_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.OpenOrCreate))
            {
                stream.ReadByte();

                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
            }

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Exists.Should().BeTrue();
        }

        [Fact]
        private void When_opening_existing_local_readonly_file_in_OpenOrCreate_mode_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents, attributes: FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.OpenOrCreate);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\file.txt' is denied.");
        }

        [Fact]
        private void When_opening_existing_local_readonly_file_in_OpenOrCreate_mode_with_Read_access_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents, attributes: FileAttributes.ReadOnly)
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.OpenOrCreate, FileAccess.Read))
            {
                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
            }
        }

        [Fact]
        private void When_opening_missing_local_file_in_OpenOrCreate_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.OpenOrCreate))
            {
                // Assert
                stream.Length.Should().Be(0);
            }

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Exists.Should().BeTrue();
            info.Length.Should().Be(0);
        }

        [Fact]
        private void When_opening_file_in_Truncate_mode_with_Read_access_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.Truncate, FileAccess.Read);

            // Assert
            action.ShouldThrow<ArgumentException>()
                .WithMessage(@"Combining FileMode: Truncate with FileAccess: Read is invalid.*");
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
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Truncate))
            {
                stream.WriteByte(0x20);
            }

            // Assert
            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Exists.Should().BeTrue();
            info.Length.Should().Be(1);

            fileSystem.File.ReadAllText(path).Should().Be(" ");
        }

        [Fact]
        private void When_opening_existing_local_readonly_file_in_Truncate_mode_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents, attributes: FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.Truncate);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\file.txt' is denied.");
        }

        [Fact]
        private void When_opening_file_in_Append_mode_with_Read_access_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.Append, FileAccess.Read);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage(@"Combining FileMode: Append with FileAccess: Read is invalid.*");
        }

        [Fact]
        private void When_opening_missing_local_file_in_Append_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Append))
            {
                // Assert
                stream.Length.Should().Be(0);
            }

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Exists.Should().BeTrue();
            info.Length.Should().Be(0);
        }

        [Fact]
        private void When_opening_existing_local_file_in_Append_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Append))
            {
                byte[] buffer = { (byte)'X', (byte)'Y', (byte)'Z' };
                stream.Write(buffer, 0, buffer.Length);
            }

            // Assert
            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Exists.Should().BeTrue();
            info.Length.Should().Be(DefaultContents.Length + 3);

            fileSystem.File.ReadAllText(path).Should().Be(DefaultContents + "XYZ");
        }

        [Fact]
        private void When_opening_existing_local_readonly_file_in_Append_mode_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents, attributes: FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(path, FileMode.Append);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\file.txt' is denied.");
        }

        [Fact]
        private void When_opening_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(@"C:\some\FILE.txt", DefaultContents)
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(@"c:\SOME\file.TXT", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
            }
        }

        [Fact]
        private void When_opening_existing_local_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(@"C:\some\file.txt", DefaultContents)
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(@"C:\some\file.txt  ", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
            }
        }

        [Fact]
        private void When_opening_file_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingTextFile(@"C:\file.txt", DefaultContents)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            using (IFileStream stream = fileSystem.File.Open(@"\file.txt", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
            }
        }

        [Fact]
        private void When_opening_existing_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(@"C:\some\FILE.txt", DefaultContents)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            using (IFileStream stream = fileSystem.File.Open("file.txt", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
            }
        }

        [Fact]
        private void When_opening_existing_relative_local_file_on_different_drive_in_subdirectory_it_must_fail()
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
                .IncludingTextFile(@"D:\child.txt", DefaultContents)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");
            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            using (IFileStream stream = fileSystem.File.Open("D:child.txt", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
            }
        }

        [Fact]
        private void When_opening_existing_relative_local_file_on_same_drive_in_subdirectory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(@"D:\other\child.txt", DefaultContents)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            using (IFileStream stream = fileSystem.File.Open("D:child.txt", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
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
        private void When_opening_local_file_for_file_that_exists_as_directory_it_must_fail()
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
        private void When_opening_local_file_for_parent_directory_that_exists_as_file_it_must_fail()
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
        private void When_opening_local_file_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Open(@"C:\some\file.txt\nested.txt\more.txt", FileMode.Open);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\file.txt\nested.txt\more.txt'.");
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
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
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
                .IncludingTextFile(@"C:\folder\file.txt", DefaultContents)
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(@"\\?\C:\folder\file.txt", FileMode.Open))
            {
                // Assert
                stream.Length.Should().Be(DefaultContents.Length);
            }
        }

        [Fact]
        private void When_writing_to_existing_file_it_must_update_properties_only_after_closing()
        {
            // Arrange
            const string path = @"C:\file.txt";

            DateTime createTimeUtc = 2.January(2017).At(22, 14);
            var clock = new SystemClock { UtcNow = () => createTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime writeTimeUtc = 3.January(2017).At(23, 11);

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open))
            {
                clock.UtcNow = () => writeTimeUtc;

                stream.WriteByte(0x20);

                // Assert
                fileSystem.File.GetCreationTimeUtc(path).Should().Be(createTimeUtc);
                fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(createTimeUtc);
                fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(createTimeUtc);
                fileInfo.Length.Should().Be(0);
            }

            fileSystem.File.GetCreationTimeUtc(path).Should().Be(createTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(writeTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(writeTimeUtc);

            fileInfo.Refresh();
            fileInfo.Length.Should().Be(1);
        }

        [Fact]
        private void When_reading_from_existing_file_it_must_update_properties_only_after_closing()
        {
            // Arrange
            const string path = @"C:\file.txt";

            DateTime createTimeUtc = 4.January(2017).At(22, 14);
            var clock = new SystemClock { UtcNow = () => createTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(path, "X")
                .Build();

            DateTime accessTimeUtc = 5.January(2017).At(23, 11);
            clock.UtcNow = () => accessTimeUtc;

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                stream.ReadByte();

                // Assert
                fileSystem.File.GetCreationTimeUtc(path).Should().Be(createTimeUtc);
                fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(createTimeUtc);
                fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(createTimeUtc);
            }

            fileSystem.File.GetCreationTimeUtc(path).Should().Be(createTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(createTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(accessTimeUtc);
        }

        [Fact]
        private void When_updating_existing_file_on_background_thread_its_property_changes_must_be_observable_from_main_thread()
        {
            // Arrange
            const string path = @"C:\file.txt";

            DateTime createTimeUtc = 31.January(2017).At(22, 14);
            var clock = new SystemClock { UtcNow = () => createTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime accessTimeUtcBefore = fileSystem.File.GetLastAccessTimeUtc(path);
            DateTime writeTimeUtcBefore = fileSystem.File.GetLastWriteTimeUtc(path);
            long sizeBefore = fileInfo.Length;

            // Act
            Task.Factory.StartNew(() =>
            {
                clock.UtcNow = () => createTimeUtc.AddSeconds(1);
                fileSystem.File.WriteAllText(path, "Y");
            }).Wait();

            clock.UtcNow = () => createTimeUtc.AddSeconds(2);

            DateTime accessTimeUtcAfter = fileSystem.File.GetLastAccessTimeUtc(path);
            DateTime writeTimeUtcAfter = fileSystem.File.GetLastWriteTimeUtc(path);

            fileInfo.Refresh();
            long sizeAfter = fileInfo.Length;

            // Assert
            accessTimeUtcAfter.Should().BeAfter(accessTimeUtcBefore);
            writeTimeUtcAfter.Should().BeAfter(writeTimeUtcBefore);
            sizeAfter.Should().BeGreaterThan(sizeBefore);
        }
    }
}
