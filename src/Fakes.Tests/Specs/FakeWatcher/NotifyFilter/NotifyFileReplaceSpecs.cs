#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyFileReplaceSpecs : WatcherSpecs
    {
        [Theory(Skip = "TODO")]
        [WatcherNotifyTestData(@"
            * Container\source.txt                          @                                                       CreationTime
            * Container                                     @                               LastWrite   LastAccess
            + Container\target.txt~RF181a3a.TMP             @ FileName
            * Container                                     @                               LastWrite   LastAccess
            - Container\target.txt                          @ FileName
            * Container\target.txt~RF181a3a.TMP             @           Attributes  Size    LastWrite   LastAccess  CreationTime    Security
            * Container                                     @                               LastWrite
            * Container\source.txt                          @                                                                       Security
            > Container\source.txt => Container\target.txt  @ FileName
            * Container                                     @                               LastWrite
            - Container\target.txt~RF181a3a.TMP             @ FileName
        ")]
        private void When_replacing_file_with_different_name_in_same_directory_without_backup_it_must_raise_events(
            NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerName = "Container";

            string pathToSourceFile = Path.Combine(directoryToWatch, containerName, "source.txt");
            string pathToDestinationFile = Path.Combine(directoryToWatch, containerName, "target.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "SourceText")
                .IncludingTextFile(pathToDestinationFile, "DestinationText")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Replace(pathToSourceFile, pathToDestinationFile, null);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory(Skip = "TODO")]
        [WatcherNotifyTestData(@"
            * Container\source.txt                          @                                                       CreationTime
            * Container                                     @                               LastWrite   LastAccess
            > Container\target.txt => Container\backup.txt  @ FileName
            * Container                                     @                               LastWrite   LastAccess
            * Container\source.txt                          @                                                                       Security
            > Container\source.txt => Container\target.txt  @ FileName
            * Container                                     @                               LastWrite
        ")]
        private void When_replacing_file_with_different_name_in_same_directory_with_backup_it_must_raise_events(
            NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerName = "Container";

            string pathToSourceFile = Path.Combine(directoryToWatch, containerName, "source.txt");
            string pathToDestinationFile = Path.Combine(directoryToWatch, containerName, "target.txt");
            string pathToBackupFile = Path.Combine(directoryToWatch, containerName, "backup.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "SourceText")
                .IncludingTextFile(pathToDestinationFile, "DestinationText")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Replace(pathToSourceFile, pathToDestinationFile, pathToBackupFile);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory(Skip = "TODO")]
        [WatcherNotifyTestData(@"
            * Container\source.txt                          @                                                       CreationTime
            * Container                                     @                               LastWrite   LastAccess
            - Container\target.txt                          @ FileName
            * Container\backup.txt                          @           Attributes  Size    LastWrite   LastAccess  CreationTime    Security
            * Container                                     @                               LastWrite   LastAccess
            * Container\source.txt                          @                                                                       Security
            > Container\source.txt => Container\target.txt  @ FileName
            * Container                                     @                               LastWrite
        ")]
        private void When_replacing_file_with_different_name_in_same_directory_with_existing_backup_it_must_raise_events(
            NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerName = "Container";

            string pathToSourceFile = Path.Combine(directoryToWatch, containerName, "source.txt");
            string pathToDestinationFile = Path.Combine(directoryToWatch, containerName, "target.txt");
            string pathToBackupFile = Path.Combine(directoryToWatch, containerName, "backup.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "SourceText")
                .IncludingTextFile(pathToDestinationFile, "DestinationText")
                .IncludingTextFile(pathToBackupFile, "Backup-Text")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Replace(pathToSourceFile, pathToDestinationFile, pathToBackupFile);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory(Skip = "TODO")]
        [WatcherNotifyTestData(@"
            * Container\sourceDir\source.txt                @                                                       CreationTime
            * Container\targetDir                           @                               LastWrite   LastAccess
            * Container\sourceDir                           @                               LastWrite   LastAccess
            + Container\targetDir\target.txt~RF3a26f6.TMP   @ FileName
            * Container\targetDir                           @                               LastWrite   LastAccess
            - Container\targetDir\target.txt                @ FileName
            * Container\targetDir\target.txt~RF3a26f6.TMP   @           Attributes  Size    LastWrite   LastAccess  CreationTime    Security
            * Container\targetDir                           @                               LastWrite
            * Container\sourceDir\source.txt                @                                                                       Security
            - Container\sourceDir\source.txt                @ FileName
            + Container\targetDir\target.txt                @ FileName
            * Container\targetDir                           @                               LastWrite
            - Container\targetDir\target.txt~RF3a26f6.TMP   @ FileName
        ")]
        private void When_replacing_file_in_different_directory_without_backup_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerName = "Container";

            string pathToSourceFile = Path.Combine(directoryToWatch, containerName, "sourceDir", "source.txt");
            string pathToDestinationFile = Path.Combine(directoryToWatch, containerName, "targetDir", "target.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "SourceText")
                .IncludingTextFile(pathToDestinationFile, "DestinationText")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Replace(pathToSourceFile, pathToDestinationFile, null);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory(Skip = "TODO")]
        [WatcherNotifyTestData(@"
            * Container\sourceDir\source.txt                @                                                       CreationTime
            * Container\targetDir                           @                               LastWrite   LastAccess
            * Container\sourceDir                           @                               LastWrite   LastAccess
            - Container\targetDir\target.txt                @ FileName
            + Container\backupDir\backup.txt                @ FileName
            * Container\backupDir                           @                               LastWrite   LastAccess
            * Container\targetDir                           @                               LastWrite   LastAccess
            * Container\sourceDir\source.txt                @                                                                       Security
            - Container\sourceDir\source.txt                @ FileName
            + Container\targetDir\target.txt                @ FileName
            * Container\targetDir                           @                               LastWrite
        ")]
        private void When_replacing_file_in_different_directory_with_backup_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerName = "Container";

            string pathToSourceFile = Path.Combine(directoryToWatch, containerName, "sourceDir", "source.txt");
            string pathToDestinationFile = Path.Combine(directoryToWatch, containerName, "targetDir", "target.txt");
            string pathToBackupDirectory = Path.Combine(directoryToWatch, containerName, "backupDir");
            string pathToBackupFile = Path.Combine(pathToBackupDirectory, "backup.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "SourceText")
                .IncludingTextFile(pathToDestinationFile, "DestinationText")
                .IncludingDirectory(pathToBackupDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Replace(pathToSourceFile, pathToDestinationFile, pathToBackupFile);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory(Skip = "TODO")]
        [WatcherNotifyTestData(@"
            * Container\sourceDir\source.txt                @                                                       CreationTime
            * Container\targetDir                           @                               LastWrite   LastAccess
            * Container\sourceDir                           @                               LastWrite   LastAccess
            * Container\backupDir                           @                               LastWrite   LastAccess
            - Container\targetDir\target.txt                @ FileName
            * Container\backupDir\backup.txt                @           Attributes  Size    LastWrite   LastAccess  CreationTime    Security
            * Container\backupDir                           @                               LastWrite   LastAccess
            * Container\targetDir                           @                               LastWrite   LastAccess
            * Container\sourceDir\source.txt                @                                                                       Security
            - Container\sourceDir\source.txt                @ FileName
            + Container\targetDir\target.txt                @ FileName
            * Container\targetDir                           @                               LastWrite
        ")]
        private void When_replacing_file_in_different_directory_with_existing_backup_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerName = "Container";

            string pathToSourceFile = Path.Combine(directoryToWatch, containerName, "sourceDir", "source.txt");
            string pathToDestinationFile = Path.Combine(directoryToWatch, containerName, "targetDir", "target.txt");
            string pathToBackupFile = Path.Combine(directoryToWatch, containerName, "backupDir", "backup.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "SourceText")
                .IncludingTextFile(pathToDestinationFile, "DestinationText")
                .IncludingTextFile(pathToBackupFile, "Backup-Text")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Replace(pathToSourceFile, pathToDestinationFile, pathToBackupFile);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        // TODO: Attributes
    }
}
#endif
