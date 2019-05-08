#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileReplaceSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_null_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.Replace(null, @"c:\destination.txt", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_null_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.Replace(@"c:\source.txt", null, @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_empty_string_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(string.Empty, @"c:\destination.txt", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_empty_string_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(@"c:\source.txt", string.Empty, @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_whitespace_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(" ", @"c:\destination.txt", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_whitespace_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(@"c:\source.txt", " ", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_invalid_drive_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace("_:", @"c:\destination.txt", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_invalid_drive_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(@"c:\source.txt", "_:", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_wildcard_characters_in_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace("some?.txt", @"c:\destination.txt", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_wildcard_characters_in_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(@"c:\source.txt", "some?.txt", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_missing_source_location_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, null);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage("Unable to find the specified file.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_missing_destination_location_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, null);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage("Unable to find the specified file.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_same_location_without_backup_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\SOME\source.TXT";
            const string targetPath = @"c:\some\SOURCE.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, null);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                "The process cannot access the file because it is being used by another process.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_source_location_same_as_backup_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"c:\SOME\Source.TXT";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("Unable to remove the file to be replaced.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_destination_location_same_as_backup_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"c:\SOME\Target.TXT";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                "Unable to move the replacement file to the file to be replaced. The file to be replaced has retained its original name.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_different_name_in_same_directory_without_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, null);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void
            When_replacing_file_with_different_name_in_same_directory_without_backup_and_existing_temporary_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";

            var clock = new SystemClock(() => 1.January(2000));

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingEmptyFile(@"C:\some\target.txt~RFbe6312.TMP")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, null);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("Unable to remove the file to be replaced.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void
            When_replacing_file_with_different_name_in_same_directory_without_backup_and_existing_temporary_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";

            var clock = new SystemClock(() => 1.January(2000));

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingDirectory(@"C:\some\target.txt~RFbe6312.TMP")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, null);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("Unable to remove the file to be replaced.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_different_name_in_same_directory_with_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_different_name_in_same_directory_with_existing_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_files_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath + "  ", targetPath + "  ", backupPath + "  ");

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_using_absolute_paths_without_drive_letter_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\source.txt";
            const string targetPath = @"C:\target.txt";
            const string backupPath = @"C:\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .IncludingDirectory(@"c:\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.Replace(@"\source.txt", @"\target.txt", @"\backup.txt");

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_using_relative_paths_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            fileSystem.File.Replace(@"some\source.txt", @"some\target.txt", @"some\backup.txt");

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_to_parent_directory_with_existing_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\top\parent\sub\source.txt";
            const string targetPath = @"C:\top\parent\target.txt";
            const string backupPath = @"C:\top\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_to_subdirectory_with_existing_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\top\source.txt";
            const string targetPath = @"C:\top\parent\target.txt";
            const string backupPath = @"C:\top\parent\sub\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
        }

        [Theory, InvestigateRunOnFileSystem]
        [InlineData("C", "D", "E", 1175)]
        [InlineData("C", "C", "D", 1175)]
        [InlineData("C", "D", "D", 1176)]
        [InlineData("C", "D", "C", 1175)]
        [InlineData("C", "D", null, 1176)]
        private void When_replacing_file_to_different_drive_it_must_fail([NotNull] string sourceDrive,
            [NotNull] string targetDrive, [CanBeNull] string backupDrive, int nativeErrorCode)
        {
            // Arrange
            string sourcePath = sourceDrive + @":\source.txt";
            string targetPath = targetDrive + @":\target.txt";
            string backupPath = (backupDrive ?? "X") + @":\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupDrive != null ? backupPath : null);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(nativeErrorCode == 1175
                ? "Unable to remove the file to be replaced."
                : "Unable to move the replacement file to the file to be replaced. The file to be replaced has retained its original name.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_source_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\sourceDir";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage("Access to the path is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_destination_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\targetDir";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingDirectory(targetPath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage("Access to the path is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_backup_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backupDir";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingDirectory(backupPath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("Unable to remove the file to be replaced.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_source_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\file.txt\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\file.txt")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage("Could not find a part of the path.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_destination_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\file.txt\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingEmptyFile(@"C:\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage("Could not find a part of the path.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_backup_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\file.txt\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingEmptyFile(@"c:\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("Unable to remove the file to be replaced.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_source_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\file.txt\other.txt\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\file.txt")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage("Could not find a part of the path.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_destination_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\file.txt\other.txt\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingEmptyFile(@"C:\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage("Could not find a part of the path.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_backup_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\file.txt\other.txt\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingEmptyFile(@"c:\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("Unable to remove the file to be replaced.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_source_that_is_root_of_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage("Access to the path is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_destination_that_is_root_of_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage("Access to the path is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_backup_that_is_root_of_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("Unable to remove the file to be replaced.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_readonly_source_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText", attributes: FileAttributes.ReadOnly)
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage("Access to the path is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_readonly_destination_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText", attributes: FileAttributes.ReadOnly)
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage("Access to the path is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_readonly_backup_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText", attributes: FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("Unable to remove the file to be replaced.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_hidden_source_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText", attributes: FileAttributes.Hidden)
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.GetAttributes(targetPath).Should().NotHaveFlag(FileAttributes.Hidden);
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
            fileSystem.File.GetAttributes(backupPath).Should().NotHaveFlag(FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_hidden_destination_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText", attributes: FileAttributes.Hidden)
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.GetAttributes(targetPath).Should().HaveFlag(FileAttributes.Hidden);
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
            fileSystem.File.GetAttributes(backupPath).Should().HaveFlag(FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_hidden_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText", attributes: FileAttributes.Hidden)
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.GetAttributes(targetPath).Should().NotHaveFlag(FileAttributes.Hidden);
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
            fileSystem.File.GetAttributes(backupPath).Should().NotHaveFlag(FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_open_source_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            using (fileSystem.File.OpenRead(sourcePath))
            {
                // Act
                Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage(
                    "The process cannot access the file because it is being used by another process.");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_open_destination_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            using (fileSystem.File.OpenRead(targetPath))
            {
                // Act
                Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage(
                    "The process cannot access the file because it is being used by another process.");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_open_backup_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            using (fileSystem.File.OpenRead(backupPath))
            {
                // Act
                Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage("Unable to remove the file to be replaced.");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_for_missing_network_share_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"\\server\share\source.txt";
            const string targetPath = @"\\server\share\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, null);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The network path was not found.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_for_backup_on_missing_network_share_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"\\server\share\source.txt";
            const string targetPath = @"\\server\share\target.txt";
            const string backupPath = @"\\missing-server\share\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("Unable to remove the file to be replaced.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_for_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"\\server\share\source.txt";
            const string targetPath = @"\\server\share\target.txt";
            const string backupPath = @"\\server\share\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_source_reserved_name_it_must_fail()
        {
            // Arrange
            const string sourcePath = "NUL";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_destination_reserved_name_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = "COM1";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_backup_reserved_name_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"com3";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_for_extended_path_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"\\?\c:\folder\source.txt";
            const string targetPath = @"\\?\c:\folder\target.txt";
            const string backupPath = @"\\?\c:\folder\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .IncludingTextFile(backupPath, "BackupText")
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.ReadAllText(targetPath).Should().Be("SourceText");
            fileSystem.File.Exists(backupPath).Should().BeTrue();
            fileSystem.File.ReadAllText(backupPath).Should().Be("TargetText");
        }
    }
}
#endif
