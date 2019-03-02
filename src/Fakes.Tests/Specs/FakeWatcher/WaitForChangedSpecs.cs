#if !NETCOREAPP1_1
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class WaitForChangedSpecs : WatcherSpecs
    {
        private const int OperationDelayInMilliseconds = 250;

        [Fact]
        private void When_waiting_for_changes_with_negative_timeout_it_must_fail()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.WaitForChanged(WatcherChangeTypes.All, -5);

                // Assert
                action.Should().ThrowExactly<ArgumentOutOfRangeException>()
                    .WithMessage("Specified argument was out of the range of valid values.*");
            }
        }

        [Fact]
        private void When_waiting_for_changes_while_timeout_expires_it_must_succeed()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                // Act
                WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.All, 1000);

                // Assert
                result.TimedOut.Should().BeTrue();
                result.ChangeType.Should().Be(0);
                result.Name.Should().BeNull();
                result.OldName.Should().BeNull();
            }
        }

        [Fact]
        private void When_waiting_for_file_creation_while_files_are_created_it_must_succeed()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToCreate1 = "file1.txt";
            const string fileNameToCreate2 = "file2.txt";

            string pathToFileToCreate1 = Path.Combine(directoryToWatch, fileNameToCreate1);
            string pathToFileToCreate2 = Path.Combine(directoryToWatch, fileNameToCreate2);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;

                Task.Run(() =>
                {
                    Thread.Sleep(OperationDelayInMilliseconds);
                    fileSystem.File.WriteAllText(pathToFileToCreate1, "X");
                    fileSystem.File.WriteAllText(pathToFileToCreate2, "*");
                });

                // Act
                WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.Created);

                // Assert
                result.TimedOut.Should().BeFalse();
                result.ChangeType.Should().Be(WatcherChangeTypes.Created);
                result.Name.Should().Be(fileNameToCreate1);
                result.OldName.Should().BeNull();
            }
        }

        [Fact]
        private void When_waiting_for_directory_deletion_while_directories_are_deleted_it_must_succeed()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToDelete1 = "subfolder1";
            const string directoryNameToDelete2 = "subfolder2";

            string pathToDirectoryToDelete1 = Path.Combine(directoryToWatch, directoryNameToDelete1);
            string pathToDirectoryToDelete2 = Path.Combine(directoryToWatch, directoryNameToDelete2);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToDelete1)
                .IncludingDirectory(pathToDirectoryToDelete2)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.DirectoryName;

                Task.Run(() =>
                {
                    Thread.Sleep(OperationDelayInMilliseconds);
                    fileSystem.Directory.Delete(pathToDirectoryToDelete1);
                    fileSystem.Directory.Delete(pathToDirectoryToDelete2);
                });

                // Act
                WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.Deleted);

                // Assert
                result.TimedOut.Should().BeFalse();
                result.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                result.Name.Should().Be(directoryNameToDelete1);
                result.OldName.Should().BeNull();
            }
        }

        [Fact]
        private void When_waiting_for_file_rename_while_files_are_renamed_it_must_succeed()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName1 = "source1.txt";
            const string targetFileName1 = "target1.txt";
            const string sourceFileName2 = "source2.txt";
            const string targetFileName2 = "target2.txt";

            string pathToSourceFile1 = Path.Combine(directoryToWatch, sourceFileName1);
            string pathToTargetFile1 = Path.Combine(directoryToWatch, targetFileName1);
            string pathToSourceFile2 = Path.Combine(directoryToWatch, sourceFileName2);
            string pathToTargetFile2 = Path.Combine(directoryToWatch, targetFileName2);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile1)
                .IncludingEmptyFile(pathToSourceFile2)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;

                Task.Run(() =>
                {
                    Thread.Sleep(OperationDelayInMilliseconds);
                    fileSystem.File.Move(pathToSourceFile1, pathToTargetFile1);
                    fileSystem.File.Move(pathToSourceFile2, pathToTargetFile2);
                });

                // Act
                WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.Renamed);

                // Assert
                result.TimedOut.Should().BeFalse();
                result.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                result.Name.Should().Be(targetFileName1);
                result.OldName.Should().Be(sourceFileName1);
            }
        }

        [Fact]
        private void When_waiting_for_file_change_while_file_attributes_are_changed_it_must_succeed()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate1 = "file1.txt";
            const string fileNameToUpdate2 = "file2.txt";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch, fileNameToUpdate1);
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch, fileNameToUpdate2);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .IncludingEmptyFile(pathToFileToUpdate2)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Attributes;

                Task.Run(() =>
                {
                    Thread.Sleep(OperationDelayInMilliseconds);
                    fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.System);
                    fileSystem.File.SetAttributes(pathToFileToUpdate2, FileAttributes.System);
                });

                // Act
                WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.Changed);

                // Assert
                result.TimedOut.Should().BeFalse();
                result.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                result.Name.Should().Be(fileNameToUpdate1);
                result.OldName.Should().BeNull();
            }
        }

        [Fact]
        private void When_waiting_for_file_change_while_watcher_was_running_it_must_keep_running_after_wait()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetLastWriteTimeUtc(pathToFileToUpdate, 1.January(2000));
                    Thread.Sleep(SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop);

                    Task.Run(() =>
                    {
                        Thread.Sleep(OperationDelayInMilliseconds);
                        fileSystem.File.SetLastWriteTimeUtc(pathToFileToUpdate, 2.January(2000));
                    });

                    // Act
                    WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.Changed);

                    fileSystem.File.SetLastWriteTimeUtc(pathToFileToUpdate, 3.January(2000));

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    result.TimedOut.Should().BeFalse();
                    result.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    result.Name.Should().Be(fileNameToUpdate);
                    result.OldName.Should().BeNull();

                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(@"
                        * file.txt
                        * file.txt
                        * file.txt
                        ".TrimLines());
                }
            }
        }

        [Fact]
        private void When_waiting_for_file_change_while_watcher_was_not_running_it_must_be_stopped_after_wait()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    watcher.EnableRaisingEvents = false;

                    fileSystem.File.SetLastWriteTimeUtc(pathToFileToUpdate, 1.January(2000));
                    Thread.Sleep(SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop);

                    Task.Run(() =>
                    {
                        Thread.Sleep(OperationDelayInMilliseconds);
                        fileSystem.File.SetLastWriteTimeUtc(pathToFileToUpdate, 2.January(2000));
                    });

                    // Act
                    WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.Changed);

                    fileSystem.File.SetLastWriteTimeUtc(pathToFileToUpdate, 3.January(2000));

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    result.TimedOut.Should().BeFalse();
                    result.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    result.Name.Should().Be(fileNameToUpdate);
                    result.OldName.Should().BeNull();

                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(@"
                        * file.txt
                        ".TrimLines());
                }
            }
        }

        [Fact]
        private void When_waiting_for_file_change_multiple_times_it_must_succeed()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            // ReSharper disable once ConvertToLocalFunction
            Action<DateTime> updateCreationTimeAction = creationTimeUtc =>
            {
                Thread.Sleep(OperationDelayInMilliseconds);
                fileSystem.File.SetCreationTimeUtc(pathToFileToUpdate, creationTimeUtc);
            };

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.CreationTime;

                Task.Run(() => updateCreationTimeAction(1.January(2000)));

                // Act
                WaitForChangedResult result1 = watcher.WaitForChanged(WatcherChangeTypes.Changed);

                Task.Run(() => updateCreationTimeAction(2.January(2000)));

                WaitForChangedResult result2 = watcher.WaitForChanged(WatcherChangeTypes.Changed);

                // Assert
                result1.TimedOut.Should().BeFalse();
                result1.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                result1.Name.Should().Be(fileNameToUpdate);
                result1.OldName.Should().BeNull();

                result2.TimedOut.Should().BeFalse();
                result2.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                result2.Name.Should().Be(fileNameToUpdate);
                result2.OldName.Should().BeNull();
            }
        }

        [Fact]
        private void When_waiting_for_change_it_must_apply_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToMismatchingFile = Path.Combine(directoryToWatch, "OtherName.txt");
            string pathToMismatchingDirectory = Path.Combine(directoryToWatch, "SomeFolderIgnored");
            string pathToMatchingFile = Path.Combine(directoryToWatch, "Subfolder", "SomeFile.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToMismatchingFile)
                .IncludingDirectory(Path.Combine(directoryToWatch, "Subfolder"))
                .IncludingEmptyFile(pathToMatchingFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
                watcher.IncludeSubdirectories = true;
                watcher.Filter = "Some*";

                Task.Run(() =>
                {
                    Thread.Sleep(OperationDelayInMilliseconds);
                    fileSystem.Directory.CreateDirectory(pathToMismatchingDirectory);
                    fileSystem.File.AppendAllText(pathToMismatchingFile, "IgnoreMe");
                    fileSystem.File.AppendAllText(pathToMatchingFile, "NotifyForMe");
                });

                // Act
                WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.All);

                // Assert
                result.TimedOut.Should().BeFalse();
                result.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                result.Name.Should().Be(@"Subfolder\SomeFile.txt");
                result.OldName.Should().BeNull();
            }
        }

        [Fact]
        private void When_waiting_for_disposed_watcher_it_must_fail()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher();
            watcher.Dispose();

            // Act
            Action action = () => watcher.WaitForChanged(WatcherChangeTypes.All);

            // Assert
            action.Should().ThrowExactly<ObjectDisposedException>().WithMessage("Cannot access a disposed object.*")
                .And.ObjectName.Should().Be("FileSystemWatcher");
        }
    }
}
#endif
