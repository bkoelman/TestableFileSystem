#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileReplaceSpecs
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        private void When_replacing_file_with_invalid_source_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace("_:", @"c:\destination.txt", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_replacing_file_with_invalid_destination_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(@"c:\source.txt", "_:", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_replacing_file_with_invalid_characters_in_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace("some?.txt", @"c:\destination.txt", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_replacing_file_with_invalid_characters_in_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Replace(@"c:\source.txt", "some?.txt", @"c:\backup.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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
            action.Should().ThrowExactly<IOException>()
                .WithMessage("The process cannot access the file because it is being used by another process.");
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        // TODO: Absolute/relative paths

        [Fact]
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

        [Fact]
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

        [Theory]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        // TODO: Missing parent and parent-parent directories
        // TODO: Root of drive cannot be used
        // TODO: Readonly/hidden files
        // TODO: Files that are in use
        // TODO: UNC paths
        // TODO: Reserved names
        // TODO: Extended paths

        // TODO: Investigate the effects of ignoreMetadataErrors
        // TODO: Investigate transfer of attributes and file times

        // TODO: Review notes at:
        //          https://docs.microsoft.com/en-us/dotnet/api/system.io.file.replace?view=netframework-4.7.2#System_IO_File_Replace_System_String_System_String_System_String_System_Boolean_
        //       and:
        //          https://docs.microsoft.com/en-us/windows/desktop/api/winbase/nf-winbase-replacefilew
    }
}
#endif
