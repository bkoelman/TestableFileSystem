using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryTimeLastWriteSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime DefaultTime = DefaultTimeUtc.ToLocalTime();
        private static readonly DateTime HighTime = DateTime.MaxValue.AddDays(-2).AsUtc().ToLocalTime();
        private static readonly DateTime ZeroFileTime = 1.January(1601).AsUtc().ToLocalTime();

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.GetLastWriteTime(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.SetLastWriteTime(null, DefaultTime);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTime(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(string.Empty, DefaultTime);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTime(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(" ", DefaultTime);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTime("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime("::", DefaultTime);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTime(@"c:\dir?i");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"c:\dir?i", DefaultTime);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_missing_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(@"C:\some\nested");

            // Assert
            time.Should().Be(ZeroFileTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"C:\some\nested", DefaultTime);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\nested'.");
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_existing_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(path).Should().Be(DefaultTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_existing_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTime(@"C:\some  ", DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(@"C:\some  ").Should().Be(DefaultTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_existing_readonly_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some", FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTime(@"C:\some", DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(@"C:\some").Should().Be(DefaultTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_existing_subdirectory_in_readonly_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder", FileAttributes.ReadOnly)
                .IncludingDirectory(@"C:\folder\some")
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTime(@"C:\folder\some", DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(@"C:\folder\some").Should().Be(DefaultTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_current_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(path);

            // Act
            fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(path).Should().Be(DefaultTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_existing_directory_to_MinValue_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(path, DateTime.MinValue);

            // Assert
            action.ShouldThrow<ArgumentOutOfRangeException>().WithMessage("Not a valid Win32 FileTime.*");
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_existing_directory_to_MaxValue_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTime(path, HighTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(path).Should().Be(HighTime);
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_drive_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(path);

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_drive_it_must_fail()
        {
            // Arrange
            const string path = @"C:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Path must not be a drive.*");
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"C:\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(@"\folder");

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"C:\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.Directory.SetLastWriteTime(@"\folder", DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(@"c:\folder").Should().Be(DefaultTime);
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_existing_relative_local_directory_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"C:\folder\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(@"some");

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_existing_relative_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\folder\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            fileSystem.Directory.SetLastWriteTime(@"some", DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(@"C:\folder\some").Should().Be(DefaultTime);
        }

        [Fact]
        private void
            When_getting_last_write_time_in_local_zone_for_existing_local_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"C:\FOLDER\some")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(@"C:\folder\SOME");

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact]
        private void
            When_setting_last_write_time_in_local_zone_for_existing_local_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\FOLDER\some")
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTime(@"C:\folder\SOME", DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(@"C:\FOLDER\some").Should().Be(DefaultTime);
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_missing_parent_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(@"C:\some\folder");

            // Assert
            time.Should().Be(ZeroFileTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"C:\some\folder", DefaultTime);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\folder'.");
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_existing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(path);

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_existing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(path).Should().Be(DefaultTime);
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(@"c:\some\file.txt\nested.txt");

            // Assert
            time.Should().Be(ZeroFileTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"c:\some\file.txt\nested", DefaultTime);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\some\file.txt\nested'.");
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_parent_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(@"c:\some\file.txt\nested\more");

            // Assert
            time.Should().Be(ZeroFileTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"c:\some\file.txt\nested\more", DefaultTime);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\some\file.txt\nested\more'.");
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTime(path);

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The network path was not found");
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The network path was not found");
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(path);

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(path).Should().Be(DefaultTime);
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_missing_remote_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\missing";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(path);

            // Assert
            time.Should().Be(ZeroFileTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_missing_remote_directory_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file '\\server\share\missing'.");
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_existing_remote_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\personal";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(path).Should().Be(DefaultTime);
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTime("COM1");

            // Assert
            action.ShouldThrow<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime("COM1", DefaultTime);

            // Assert
            action.ShouldThrow<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_getting_last_write_time_in_local_zone_for_missing_extended_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(@"\\?\C:\some\missing");

            // Assert
            time.Should().Be(ZeroFileTime);
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_missing_extended_local_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"\\?\C:\some\missing", DefaultTime);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file '\\?\C:\some\missing'.");
        }

        [Fact]
        private void When_setting_last_write_time_in_local_zone_for_existing_extended_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\other")
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTime(@"\\?\C:\some\other", DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(@"\\?\C:\some\other").Should().Be(DefaultTime);
        }
    }
}
