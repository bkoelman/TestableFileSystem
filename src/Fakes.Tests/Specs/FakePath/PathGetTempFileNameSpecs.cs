using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakePath
{
    public sealed class PathGetTempFileNameSpecs
    {
        private const string RandomFileNameRegex = "^tmp[0-9A-F]{1,4}.tmp$";

        [Fact]
        private void When_getting_temp_file_name_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder().Build();

            // Act
            string tempPath = fileSystem.Path.GetTempFileName();

            // Assert
            Path.GetDirectoryName(tempPath).Should().Be(@"C:\Temp");
            Path.GetFileName(tempPath).Should().MatchRegex(RandomFileNameRegex);

            fileSystem.File.Exists(tempPath).Should().BeTrue();
        }

        [Fact]
        private void When_getting_temp_file_name_without_drive_C_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithoutDefaultDriveC()
                .IncludingDirectory(@"x:\data")
                .Build();

            // Act
            string tempPath = fileSystem.Path.GetTempFileName();

            // Assert
            Path.GetDirectoryName(tempPath).Should().Be(@"X:\Temp");
            Path.GetFileName(tempPath).Should().MatchRegex(RandomFileNameRegex);

            fileSystem.File.Exists(tempPath).Should().BeTrue();
        }

        [Fact]
        private void When_getting_temp_file_name_with_custom_temp_directory_it_must_succeed()
        {
            // Arrange
            const string directory = @"x:\Users\joe\Temp";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithTempDirectory(directory)
                .Build();

            // Act
            string tempPath = fileSystem.Path.GetTempFileName();

            // Assert
            Path.GetDirectoryName(tempPath).Should().Be(@"X:\Users\joe\Temp");
            Path.GetFileName(tempPath).Should().MatchRegex(RandomFileNameRegex);

            fileSystem.File.Exists(tempPath).Should().BeTrue();
        }

        [Fact]
        private void When_getting_temp_file_name_with_missing_temp_directory_it_must_fail()
        {
            // Arrange
            const string directory = @"x:\missing\folder\tree";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithTempDirectory(directory)
                .Build();

            fileSystem.Directory.Delete(@"x:\missing", true);

            // Act
            Action action = () => fileSystem.Path.GetTempFileName();

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The directory name is invalid.");
        }

        [Fact]
        private void When_getting_temp_file_name_with_temp_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string fileName = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithTempDirectory(fileName)
                .Build();

            fileSystem.Directory.Delete(fileName);
            fileSystem.File.WriteAllText(fileName, string.Empty);

            // Act
            Action action = () => fileSystem.Path.GetTempFileName();

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The directory name is invalid.");
        }

        [Fact]
        private void When_getting_temp_file_name_with_temp_directory_that_exists_as_nested_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithTempDirectory(@"c:\folder\file.txt\deeper.txt")
                .Build();

            fileSystem.Directory.Delete(@"c:\folder\file.txt", true);
            fileSystem.File.WriteAllText(@"c:\folder\file.txt", string.Empty);

            // Act
            Action action = () => fileSystem.Path.GetTempFileName();

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The directory name is invalid.");
        }

        [Fact]
        private void When_getting_temp_file_name_with_existing_temp_files_except_low_number_it_must_succeed()
        {
            // Arrange
            const string directory = @"c:\Temp";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithEmptyFilesInTempDirectory(directory, 0x1234)
                .Build();

            // Act
            string tempPath = fileSystem.Path.GetTempFileName();

            // Assert
            tempPath.Should().Be(@"C:\Temp\tmp1234.tmp");
        }

        [Fact]
        private void When_getting_temp_file_name_with_existing_temp_files_except_high_number_it_must_succeed()
        {
            // Arrange
            const string directory = @"c:\Temp";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithEmptyFilesInTempDirectory(directory, 0xFEDC)
                .Build();

            // Act
            string tempPath = fileSystem.Path.GetTempFileName();

            // Assert
            tempPath.Should().Be(@"C:\Temp\tmpFEDC.tmp");
        }

        [Fact]
        private void When_getting_temp_file_name_with_all_existing_temp_files_it_must_fail()
        {
            // Arrange
            const string directory = @"c:\Temp";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithEmptyFilesInTempDirectory(directory)
                .Build();

            // Act
            Action action = () => fileSystem.Path.GetTempFileName();

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The file exists.");
        }
    }

    internal static class PathGetTempFileNameFakeFileSystemBuilderExtensions
    {
        [NotNull]
        public static FakeFileSystemBuilder WithEmptyFilesInTempDirectory([NotNull] this FakeFileSystemBuilder builder,
            [NotNull] string tempDirectory,
            int indexToExclude = -1)
        {
            builder.WithTempDirectory(tempDirectory);

            for (int index = 0; index <= 0xFFFF; index++)
            {
                if (index != indexToExclude)
                {
                    string path = Path.Combine(tempDirectory, "tmp" + index.ToString("X") + ".tmp");
                    builder.IncludingEmptyFile(path);
                }
            }

            return builder;
        }
    }
}
