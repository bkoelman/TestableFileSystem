#if !NETCOREAPP1_1
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class IncludeSubdirectoriesSpecs : WatcherSpecs
    {
        [Fact]
        private void When_changing_attributes_in_subdirectory_with_IncludeSubdirectories_enabled_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string directoryNameToUpdate = "TargetFolder";

            string pathToDirectoryToUpdate = Path.Combine(directoryToWatch, containerDirectoryName, directoryNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToUpdate);
                    args.Name.Should().Be(containerDirectoryName + @"\" + directoryNameToUpdate);
                }
            }
        }

        [Fact]
        private void When_changing_attributes_in_subdirectory_with_IncludeSubdirectories_disabled_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string directoryNameToUpdate = "TargetFolder";

            string pathToDirectoryToUpdate = Path.Combine(directoryToWatch, containerDirectoryName, directoryNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = false;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_changing_attributes_in_watched_directory_with_IncludeSubdirectories_disabled_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToUpdate = "TargetFolder";

            string pathToDirectoryToUpdate = Path.Combine(directoryToWatch, directoryNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = false;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToUpdate);
                    args.Name.Should().Be(directoryNameToUpdate);
                }
            }
        }

        [Fact]
        private void When_changing_attributes_of_watched_directory_with_IncludeSubdirectories_enabled_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string pathToDirectoryToUpdate = directoryToWatch;

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_changing_IncludeSubdirectories_on_running_watcher_it_must_discard_old_notifications_and_restart()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch, "file1.txt");
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch, "file2.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .IncludingEmptyFile(pathToFileToUpdate2)
                .Build();

            var lockObject = new object();
            bool isFirstEventInvocation = true;
            FileSystemEventArgs argsAfterRestart = null;

            var resumeEventHandlerEvent = new ManualResetEvent(false);
            var testCompletionEvent = new ManualResetEvent(false);

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;
                watcher.Changed += (sender, args) =>
                {
                    lock (lockObject)
                    {
                        if (isFirstEventInvocation)
                        {
                            // Wait for all change notifications on file1.txt and file2.txt to queue up.
                            resumeEventHandlerEvent.WaitOne(Timeout.Infinite);
                            isFirstEventInvocation = false;
                        }
                        else
                        {
                            // After event handler for first change on file1 has completed, no additional
                            // changes on file1.txt should be raised because they have become outdated.
                            argsAfterRestart = args;
                            testCompletionEvent.Set();
                        }
                    }
                };
                watcher.EnableRaisingEvents = true;

                fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.Hidden);
                fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.ReadOnly);
                fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.System);
                Thread.Sleep(SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop);

                // Act
                watcher.IncludeSubdirectories = false;

                fileSystem.File.SetAttributes(pathToFileToUpdate2, FileAttributes.Hidden);

                resumeEventHandlerEvent.Set();
                testCompletionEvent.WaitOne(Timeout.Infinite);

                lock (lockObject)
                {
                    // Assert
                    argsAfterRestart.Name.Should().Be("file2.txt");
                }
            }
        }

        [Fact]
        private void When_setting_IncludeSubdirectories_on_disposed_watcher_it_must_succeed()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher();
            watcher.Dispose();

            // Act
            watcher.IncludeSubdirectories = true;

            // Assert
            watcher.IncludeSubdirectories.Should().BeTrue();
        }
    }
}
#endif
