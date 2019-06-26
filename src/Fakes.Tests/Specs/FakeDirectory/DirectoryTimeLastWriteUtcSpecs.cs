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
    public sealed class DirectoryTimeLastWriteUtcSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime HighTimeUtc = DateTime.MaxValue.AddDays(-2).AsUtc();
        private static readonly DateTime ZeroFileTimeUtc = 1.January(1601).AsUtc();

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.Directory.GetLastWriteTimeUtc(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(null, DefaultTimeUtc);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetLastWriteTimeUtc(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(string.Empty, DefaultTimeUtc);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetLastWriteTimeUtc(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(" ", DefaultTimeUtc);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTimeUtc("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTimeUtc("_:", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTimeUtc(@"c:\dir?i");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(@"c:\dir?i", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_missing_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(@"C:\some\nested");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(@"C:\some\nested", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\nested'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_existing_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_existing_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(@"C:\some  ", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(@"C:\some  ").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_existing_readonly_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some", FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(@"C:\some", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(@"C:\some").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_existing_subdirectory_in_readonly_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder", FileAttributes.ReadOnly)
                .IncludingDirectory(@"C:\folder\some")
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(@"C:\folder\some", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(@"C:\folder\some").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_current_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(path);

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_existing_directory_to_MinValue_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(path, DateTime.MinValue);

            // Assert
            action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage("Not a valid Win32 FileTime.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_existing_directory_to_MaxValue_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(path, HighTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(HighTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_drive_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(path);

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_drive_it_must_fail()
        {
            // Arrange
            const string path = @"C:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Path must not be a drive.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"C:\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(@"\folder");

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"C:\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(@"\folder", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(@"c:\folder").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_existing_relative_local_directory_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"C:\folder\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(@"some");

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_existing_relative_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\folder\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(@"some", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(@"C:\folder\some").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_existing_local_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"C:\FOLDER\some")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(@"C:\folder\SOME");

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_existing_local_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\FOLDER\some")
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(@"C:\folder\SOME", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(@"C:\FOLDER\some").Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_missing_parent_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(@"C:\some\folder");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(@"C:\some\folder", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\folder'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_existing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(path);

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_existing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(@"c:\some\file.txt\nested.txt");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(@"c:\some\file.txt\nested", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_parent_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(@"c:\some\file.txt\nested\more");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(@"c:\some\file.txt\nested\more", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested\more'.");
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkShare();

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetLastWriteTimeUtc(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkShare();

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(path, DefaultTimeUtc);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_directory_below_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkDirectoryAtDepth(1);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetLastWriteTimeUtc(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_directory_below_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkDirectoryAtDepth(1);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(path, DefaultTimeUtc);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_existing_network_share_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkShare();

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(path);

            // Assert
            time.Should().Be(DefaultTimeUtc);
        }

        [Theory]
        [CanRunOnFileSystem(FileSystemRunConditions.RequiresAdministrativeRights)]
        private void When_setting_last_write_time_in_UTC_for_existing_network_share_it_must_succeed(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkShare());

                IFileSystem fileSystem = factory.Create()
                    .IncludingDirectory(path)
                    .Build();

                // Act
                fileSystem.Directory.SetLastWriteTimeUtc(path, DefaultTimeUtc);

                // Assert
                fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(DefaultTimeUtc);
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_missing_remote_directory_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);
            string parentPath = PathFactory.NetworkShare();

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(parentPath)
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(path);

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_missing_remote_directory_it_must_fail()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);
            string parentPath = PathFactory.NetworkShare();

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(parentPath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage($"Could not find file '{path}'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_existing_remote_directory_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(path, DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetLastWriteTimeUtc("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTimeUtc("COM1", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_UTC_for_missing_extended_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(@"\\?\C:\some\missing");

            // Assert
            time.Should().Be(ZeroFileTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_missing_extended_local_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetLastWriteTimeUtc(@"\\?\C:\some\missing", DefaultTimeUtc);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file '\\?\C:\some\missing'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_UTC_for_existing_extended_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\other")
                .Build();

            // Act
            fileSystem.Directory.SetLastWriteTimeUtc(@"\\?\C:\some\other", DefaultTimeUtc);

            // Assert
            fileSystem.Directory.GetLastWriteTimeUtc(@"\\?\C:\some\other").Should().Be(DefaultTimeUtc);
        }
    }
}
