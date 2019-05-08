#if !NETCOREAPP1_1
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoReplaceSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_without_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            IFileInfo sourceInfo = fileSystem.ConstructFileInfo(sourcePath);

            // Act
            IFileInfo targetInfo = sourceInfo.Replace(targetPath, null);

            // Assert
            sourceInfo.Exists.Should().BeFalse();
            targetInfo.Exists.Should().BeTrue();
            targetInfo.FullName.Should().Be(targetPath);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";
            const string backupPath = @"C:\some\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            IFileInfo sourceInfo = fileSystem.ConstructFileInfo(sourcePath);

            // Act
            IFileInfo targetInfo = sourceInfo.Replace(targetPath, backupPath);

            // Assert
            sourceInfo.Exists.Should().BeFalse();
            targetInfo.Exists.Should().BeTrue();
            targetInfo.FullName.Should().Be(targetPath);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_existing_backup_it_must_succeed()
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

            IFileInfo sourceInfo = fileSystem.ConstructFileInfo(sourcePath);

            // Act
            IFileInfo targetInfo = sourceInfo.Replace(targetPath, backupPath);

            // Assert
            sourceInfo.Exists.Should().BeFalse();
            targetInfo.Exists.Should().BeTrue();
            targetInfo.FullName.Should().Be(targetPath);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_it_must_update_cache_on_refresh()
        {
            // Arrange
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            IFileInfo sourceInfo = fileSystem.ConstructFileInfo(sourcePath);

            bool beforeFound = sourceInfo.Exists;

            sourceInfo.Replace(targetPath, null);

            // Act
            sourceInfo.Refresh();

            // Assert
            bool afterFound = sourceInfo.Exists;

            beforeFound.Should().BeTrue();
            afterFound.Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_it_must_not_refresh_automatically()
        {
            // Arrange
            const string sourcePath = @"C:\some\source.txt";
            const string targetPath = @"C:\some\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SourceText")
                .IncludingTextFile(targetPath, "TargetText")
                .Build();

            IFileInfo sourceInfo = fileSystem.ConstructFileInfo(sourcePath);

            bool beforeFound = sourceInfo.Exists;

            // Act
            sourceInfo.Replace(targetPath, null);

            // Assert
            bool afterFound = sourceInfo.Exists;

            beforeFound.Should().BeTrue();
            afterFound.Should().BeTrue();
        }
    }
}
#endif
