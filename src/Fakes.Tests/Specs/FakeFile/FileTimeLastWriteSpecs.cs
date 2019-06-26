using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileTimeLastWriteSpecs
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
                Action action = () => fileSystem.File.GetLastWriteTime(null);

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
                Action action = () => fileSystem.File.SetLastWriteTime(null, DefaultTime);

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
                Action action = () => fileSystem.File.GetLastWriteTime(string.Empty);

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
                Action action = () => fileSystem.File.SetLastWriteTime(string.Empty, DefaultTime);

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
                Action action = () => fileSystem.File.GetLastWriteTime(" ");

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
                Action action = () => fileSystem.File.SetLastWriteTime(" ", DefaultTime);

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
            Action action = () => fileSystem.File.GetLastWriteTime("_:");

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
            Action action = () => fileSystem.File.SetLastWriteTime("_:", DefaultTime);

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
            Action action = () => fileSystem.File.GetLastWriteTime(@"c:\dir?i");

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
            Action action = () => fileSystem.File.SetLastWriteTime(@"c:\dir?i", DefaultTime);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_missing_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTime(@"C:\some\file.txt");

            // Assert
            time.Should().Be(ZeroFileTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_missing_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTime(@"C:\some\file.txt", DefaultTime);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTime(path, DefaultTime);

            // Assert
            fileSystem.File.GetLastWriteTime(path).Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some.txt")
                .Build();

            // Act
            fileSystem.File.SetLastWriteTime(@"C:\some.txt  ", DefaultTime);

            // Assert
            fileSystem.File.GetLastWriteTime(@"C:\some.txt  ").Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_readonly_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some.txt", FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTime(@"C:\some.txt", DefaultTime);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>()
                .WithMessage(@"Access to the path 'C:\some.txt' is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_file_in_readonly_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder", FileAttributes.ReadOnly)
                .IncludingEmptyFile(@"C:\folder\some.txt")
                .Build();

            // Act
            fileSystem.File.SetLastWriteTime(@"C:\folder\some.txt", DefaultTime);

            // Assert
            fileSystem.File.GetLastWriteTime(@"C:\folder\some.txt").Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_opened_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\folder\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (fileSystem.File.OpenRead(path))
            {
                // Act
                Action action = () => fileSystem.File.SetLastWriteTime(path, DefaultTime);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage(
                    @"The process cannot access the file 'C:\folder\some.txt' because it is being used by another process.");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_file_to_MinValue_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTime(path, DateTime.MinValue);

            // Assert
            action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage(
                "The UTC time represented when the offset is applied must be between year 0 and 10,000.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_file_to_MaxValue_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTime(path, HighTime);

            // Assert
            fileSystem.File.GetLastWriteTime(path).Should().Be(HighTime);
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
            DateTime time = fileSystem.File.GetLastWriteTime(path);

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
            Action action = () => fileSystem.File.SetLastWriteTime(path, DefaultTime);

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
                .IncludingEmptyFile(@"C:\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            DateTime time = fileSystem.File.GetLastWriteTime(@"\file.txt");

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
                .IncludingEmptyFile(@"C:\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.SetLastWriteTime(@"\file.txt", DefaultTime);

            // Assert
            fileSystem.File.GetLastWriteTime(@"c:\file.txt").Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_existing_relative_local_file_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"C:\folder\some.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            DateTime time = fileSystem.File.GetLastWriteTime(@"some.txt");

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\folder\some.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            fileSystem.File.SetLastWriteTime(@"some.txt", DefaultTime);

            // Assert
            fileSystem.File.GetLastWriteTime(@"C:\folder\some.txt").Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"C:\FOLDER\some.TXT")
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTime(@"C:\folder\SOME.txt");

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\FOLDER\some.TXT")
                .Build();

            // Act
            fileSystem.File.SetLastWriteTime(@"C:\folder\SOME.txt", DefaultTime);

            // Assert
            fileSystem.File.GetLastWriteTime(@"C:\FOLDER\some.TXT").Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_missing_parent_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTime(@"C:\some\file.txt");

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
            Action action = () => fileSystem.File.SetLastWriteTime(@"C:\some\file.txt", DefaultTime);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\file.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_existing_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTime(path);

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTime(path, DefaultTime);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\subfolder'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTime(@"c:\some\file.txt\nested.txt");

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
            Action action = () => fileSystem.File.SetLastWriteTime(@"c:\some\file.txt\nested.txt", DefaultTime);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_parent_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTime(@"c:\some\file.txt\nested.txt\more.txt");

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
            Action action = () => fileSystem.File.SetLastWriteTime(@"c:\some\file.txt\nested.txt\more.txt", DefaultTime);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt\more.txt'.");
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
                Action action = () => fileSystem.File.GetLastWriteTime(path);

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
                Action action = () => fileSystem.File.SetLastWriteTime(path, DefaultTime);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_file_below_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkFileAtDepth(1);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.GetLastWriteTime(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_file_below_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkFileAtDepth(1);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.SetLastWriteTime(path, DefaultTime);

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
            DateTime time = fileSystem.File.GetLastWriteTime(path);

            // Assert
            time.Should().Be(DefaultTime);
        }

        [Theory]
        [CanRunOnFileSystem(FileSystemRunConditions.RequiresAdministrativeRights)]
        private void When_setting_last_write_time_in_local_zone_for_existing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkShare());

                IFileSystem fileSystem = factory.Create()
                    .IncludingDirectory(path)
                    .Build();

                // Act
                Action action = () => fileSystem.File.SetLastWriteTime(path, DefaultTime);

                // Assert
                action.Should().ThrowExactly<UnauthorizedAccessException>()
                    .WithMessage($"Access to the path '{path}' is denied.");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_missing_remote_file_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkFileAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(PathFactory.NetworkShare())
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTime(path);

            // Assert
            time.Should().Be(ZeroFileTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_missing_remote_file_it_must_fail()
        {
            // Arrange
            string path = PathFactory.NetworkFileAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(PathFactory.NetworkShare())
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTime(path, DefaultTime);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>()
                .WithMessage($"Could not find file '{path}'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_remote_file_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkFileAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTime(path, DefaultTime);

            // Assert
            fileSystem.File.GetLastWriteTime(path).Should().Be(DefaultTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetLastWriteTime("COM1");

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
            Action action = () => fileSystem.File.SetLastWriteTime("COM1", DefaultTime);

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_last_write_time_in_local_zone_for_missing_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            DateTime time = fileSystem.File.GetLastWriteTime(@"\\?\C:\some\missing.txt");

            // Assert
            time.Should().Be(ZeroFileTime);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_missing_extended_local_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTime(@"\\?\C:\some\missing.txt", DefaultTime);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file '\\?\C:\some\missing.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_last_write_time_in_local_zone_for_existing_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\other.txt")
                .Build();

            // Act
            fileSystem.File.SetLastWriteTime(@"\\?\C:\some\other.txt", DefaultTime);

            // Assert
            fileSystem.File.GetLastWriteTime(@"\\?\C:\some\other.txt").Should().Be(DefaultTime);
        }
    }
}
