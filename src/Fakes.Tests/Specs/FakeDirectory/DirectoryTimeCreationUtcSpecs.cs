using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryTimeCreationUtcSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime HighTimeUtc = DateTime.MaxValue.AddDays(-2).AsUtc();
        private static readonly DateTime ZeroFileTimeUtc = 1.January(1601).AsUtc();

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.GetCreationTimeUtc(null);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(null, DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetCreationTimeUtc(string.Empty);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(string.Empty, DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetCreationTimeUtc(" ");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(" ", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetCreationTimeUtc("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc("_:", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetCreationTimeUtc(@"c:\dir?i");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(@"c:\dir?i", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_missing_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(@"C:\some\nested");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(@"C:\some\nested", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\nested'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetCreationTimeUtc(path, DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            fileSystem.Directory.SetCreationTimeUtc(@"C:\some  ", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(@"C:\some  ").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_readonly_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some", FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.Directory.SetCreationTimeUtc(@"C:\some", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(@"C:\some").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_subdirectory_in_readonly_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder", FileAttributes.ReadOnly)
                .IncludingDirectory(@"C:\folder\some")
                .Build();

            // Act
            fileSystem.Directory.SetCreationTimeUtc(@"C:\folder\some", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(@"C:\folder\some").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_current_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(path);

            // Act
            fileSystem.Directory.SetCreationTimeUtc(path, DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_directory_to_MinValue_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(path, DateTime.MinValue);

            // Assert
            action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage("Not a valid Win32 FileTime.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_directory_to_MaxValue_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetCreationTimeUtc(path, HighTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(HighTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_drive_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(path);

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_drive_it_must_fail()
        {
            // Arrange
            const string path = @"C:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(path, DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Path must not be a drive.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"C:\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(@"\folder");

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"C:\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.Directory.SetCreationTimeUtc(@"\folder", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(@"c:\folder").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_existing_relative_local_directory_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"C:\folder\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(@"some");

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_relative_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\folder\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            fileSystem.Directory.SetCreationTimeUtc(@"some", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(@"C:\folder\some").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_existing_local_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"C:\FOLDER\some")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(@"C:\folder\SOME");

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_local_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\FOLDER\some")
                .Build();

            // Act
            fileSystem.Directory.SetCreationTimeUtc(@"C:\folder\SOME", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(@"C:\FOLDER\some").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_missing_parent_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(@"C:\some\folder");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(@"C:\some\folder", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\folder'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_existing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(path);

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.Directory.SetCreationTimeUtc(path, DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(@"c:\some\file.txt\nested.txt");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(@"c:\some\file.txt\nested", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_parent_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(@"c:\some\file.txt\nested\more");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(@"c:\some\file.txt\nested\more", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested\more'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetCreationTimeUtc(path);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The network path was not found.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(path, DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The network path was not found.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(path);

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetCreationTimeUtc(path, DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_missing_remote_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\missing";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(path);

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_missing_remote_directory_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(path, DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file '\\server\share\missing'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_remote_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\personal";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetCreationTimeUtc(path, DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetCreationTimeUtc("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc("COM1", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_creation_time_in_UTC_for_missing_extended_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetCreationTimeUtc(@"\\?\C:\some\missing");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_missing_extended_local_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCreationTimeUtc(@"\\?\C:\some\missing", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file '\\?\C:\some\missing'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_creation_time_in_UTC_for_existing_extended_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\other")
                .Build();

            // Act
            fileSystem.Directory.SetCreationTimeUtc(@"\\?\C:\some\other", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(@"\\?\C:\some\other").Should().Be(DefaultTimeUtc);
        }
    }
}
