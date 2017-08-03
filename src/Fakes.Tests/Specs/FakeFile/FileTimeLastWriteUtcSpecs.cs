using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileTimeLastWriteUtcSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime ZeroFileTimeUtc = 1.January(1601).AsUtc();

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.GetLastWriteTimeUtc(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(null, DefaultTimeUtc);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetLastWriteTimeUtc(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(string.Empty, DefaultTimeUtc);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetLastWriteTimeUtc(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(" ", DefaultTimeUtc);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetLastWriteTimeUtc("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc("::", DefaultTimeUtc);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC__for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetLastWriteTimeUtc(@"c:\dir?i");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(@"c:\dir?i", DefaultTimeUtc);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_missing_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTimeUtc(@"C:\some\file.txt");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_missing_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(@"C:\some\file.txt", DefaultTimeUtc);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_existing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_existing_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some.txt")
                .Build();

            // Act
            fileSystem.File.SetLastWriteTimeUtc(@"C:\some.txt  ", DefaultTimeUtc);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(@"C:\some.txt  ").Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_existing_readonly_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some.txt", FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(@"C:\some.txt", DefaultTimeUtc);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some.txt' is denied.");
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_existing_file_in_readonly_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder", FileAttributes.ReadOnly)
                .IncludingEmptyFile(@"C:\folder\some.txt")
                .Build();

            // Act
            fileSystem.File.SetLastWriteTimeUtc(@"C:\folder\some.txt", DefaultTimeUtc);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(@"C:\folder\some.txt").Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_opened_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\folder\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (fileSystem.File.OpenRead(path))
            {
                // Act
                Action action = () => fileSystem.File.SetLastWriteTimeUtc(path, DefaultTimeUtc);

                // Assert
                action.ShouldThrow<IOException>().WithMessage(
                    @"The process cannot access the file 'C:\folder\some.txt' because it is being used by another process.");
            }
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_existing_file_to_MinValue_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(path, DateTime.MinValue);

            // Assert
            action.ShouldThrow<ArgumentOutOfRangeException>()
                .WithMessage("The UTC time represented when the offset is applied must be between year 0 and 10,000.*");
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_existing_file_to_MaxValue_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTimeUtc(path, DateTime.MaxValue);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(DateTime.MaxValue);
        }

        [Fact(Skip = "TODO: Implement timings for directories.")]
        private void When_getting_last_write_time_in_UTC_for_drive_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTimeUtc(path);

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_drive_it_must_fail()
        {
            // Arrange
            const string path = @"C:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Path must not be a drive.*");
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_exising_relative_local_file_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"C:\folder\some.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            DateTime time = fileSystem.File.GetLastWriteTimeUtc(@"some.txt");

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_exising_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\folder\some.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            fileSystem.File.SetLastWriteTimeUtc(@"some.txt", DefaultTimeUtc);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(@"C:\folder\some.txt").Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"C:\FOLDER\some.TXT")
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTimeUtc(@"C:\folder\SOME.txt");

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\FOLDER\some.TXT")
                .Build();

            // Act
            fileSystem.File.SetLastWriteTimeUtc(@"C:\folder\SOME.txt", DefaultTimeUtc);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(@"C:\FOLDER\some.TXT").Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_missing_parent_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTimeUtc(@"C:\some\file.txt");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(@"C:\some\file.txt", DefaultTimeUtc);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\file.txt'.");
        }

        [Fact(Skip = "TODO: Implement timings for directories.")]
        private void When_getting_last_write_time_in_UTC_for_existing_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTimeUtc(path);

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_existing_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\subfolder'.");
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTimeUtc(@"c:\some\file.txt\nested.txt");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(@"c:\some\file.txt\nested.txt", DefaultTimeUtc);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\some\file.txt\nested.txt'.");
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_parent_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTimeUtc(@"c:\some\file.txt\nested.txt\more.txt");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(@"c:\some\file.txt\nested.txt\more.txt", DefaultTimeUtc);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\some\file.txt\nested.txt\more.txt'.");
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetLastWriteTimeUtc(path);

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The network path was not found");
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing.docx";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The network path was not found");
        }

        [Fact(Skip = "TODO: Implement timings for directories.")]
        private void When_getting_last_write_time_in_UTC_for_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTimeUtc(path);

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_existing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path '\\server\share' is denied.");
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_missing_remote_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\missing.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTimeUtc(path);

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_missing_remote_file_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing.docx";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file '\\server\share\missing.docx'.");
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_existing_remote_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\personal.docx";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetLastWriteTimeUtc("COM1");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc("COM1", DefaultTimeUtc);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_for_missing_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTimeUtc(@"\\?\C:\some\missing.txt");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_missing_extended_local_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(@"\\?\C:\some\missing.txt", DefaultTimeUtc);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file '\\?\C:\some\missing.txt'.");
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_for_existing_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\other.txt")
                .Build();

            // Act
            fileSystem.File.SetLastWriteTimeUtc(@"\\?\C:\some\other.txt", DefaultTimeUtc);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(@"\\?\C:\some\other.txt").Should().Be(DefaultTimeUtc);
        }
    }
}