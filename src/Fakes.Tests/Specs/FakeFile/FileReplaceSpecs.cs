#if !NETCOREAPP1_1
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileReplaceSpecs
    {
        // TODO: Basic null/empty/whitespace/... checks

        // TODO: When_replacing_file_with_same_location...
        // TODO: When_replacing_file_with_same_location_in_different_casing...

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

        // TODO: Paths with trailing whitespace
        // TODO: Absolute/relative paths
        // TODO: Paths on same/different volumes and parent/child directories
        // TODO: Files that exist as directory and directories that exist as file

        // TODO: Existing/non-existing files where unexpected
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
