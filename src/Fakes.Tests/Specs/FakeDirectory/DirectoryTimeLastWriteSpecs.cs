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
    public sealed class DirectoryTimeLastWriteSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime DefaultTime = DefaultTimeUtc.ToLocalTime();
        private static readonly DateTime HighTime = DateTime.MaxValue.AddDays(-2).AsUtc().ToLocalTime();
        private static readonly DateTime ZeroFileTime = 1.January(1601).AsUtc().ToLocalTime();

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.Directory.GetLastWriteTime(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.Directory.SetLastWriteTime(null, DefaultTime);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetLastWriteTime(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.SetLastWriteTime(string.Empty, DefaultTime);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetLastWriteTime(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.SetLastWriteTime(" ", DefaultTime);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTime("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime("_:", DefaultTime);

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTime(@"c:\dir?i");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"c:\dir?i", DefaultTime);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"C:\some\nested", DefaultTime);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\nested'.");
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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
            action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage("Not a valid Win32 FileTime.*");
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_drive_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(path);

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
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
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Path must not be a drive.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

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

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

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

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_existing_relative_local_directory_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"C:\folder\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(@"some");

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
        private void
            When_getting_last_write_time_in_local_zone_for_existing_local_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"C:\FOLDER\some")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(@"C:\folder\SOME");

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"C:\some\folder", DefaultTime);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\folder'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_existing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(path);

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"c:\some\file.txt\nested", DefaultTime);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested'.");
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"c:\some\file.txt\nested\more", DefaultTime);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested\more'.");
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkShare();

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetLastWriteTime(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkShare();

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_directory_below_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkDirectoryAtDepth(1);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetLastWriteTime(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_directory_below_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkDirectoryAtDepth(1);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_existing_network_share_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkShare();

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(path);

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_network_share_it_must_succeed(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkShare());

                IFileSystem fileSystem = factory.Create()
                    .IncludingDirectory(path)
                    .Build();

                // Act
                fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

                // Assert
                fileSystem.Directory.GetLastWriteTime(path).Should().Be(DefaultTime);
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_missing_remote_directory_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(PathFactory.NetworkShare())
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTime(path);

            // Assert
            time.Should().Be(ZeroFileTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_missing_remote_directory_it_must_fail()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(PathFactory.NetworkShare())
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage($"Could not find file '{path}'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_remote_directory_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTime(path, DefaultTime);

            // Assert
            fileSystem.Directory.GetLastWriteTime(path).Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTime("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime("COM1", DefaultTime);

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_missing_extended_local_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTime(@"\\?\C:\some\missing", DefaultTime);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file '\\?\C:\some\missing'.");
        }

        [Fact, InvestigateRunOnFileSystem]
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
