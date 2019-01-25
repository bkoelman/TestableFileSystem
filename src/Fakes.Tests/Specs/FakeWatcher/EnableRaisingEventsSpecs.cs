#if !NETCOREAPP1_1
using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class EnableRaisingEventsSpecs : WatcherSpecs
    {
        [Fact]
        private void When_starting_watcher_without_setting_path_it_must_fail()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.EnableRaisingEvents = true;

                // Assert
                action.Should().Throw<FileNotFoundException>().WithMessage(@"Error reading the  directory.");
            }
        }

        [Fact]
        private void When_starting_watcher_for_removed_directory_it_must_fail()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                fileSystem.Directory.Delete(directoryToWatch);

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.EnableRaisingEvents = true;

                // Assert
                action.Should().Throw<FileNotFoundException>().WithMessage(@"Error reading the c:\some directory.");
            }
        }

        [Fact]
        private void When_starting_watcher_multiple_times_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, "file.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    // Act
                    watcher.EnableRaisingEvents = true;

                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.Hidden);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);
                }
            }
        }

        [Fact]
        private void When_stopping_watcher_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, "file.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);
                    Thread.Sleep(SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop);

                    // Act
                    watcher.EnableRaisingEvents = false;

                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.Hidden);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);
                }
            }
        }

        [Fact]
        private void When_stopping_and_resuming_watcher_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch, "file1.txt");
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch, "file2.txt");
            string pathToFileToUpdate3 = Path.Combine(directoryToWatch, "file3.txt");
            string pathToFileToUpdate4 = Path.Combine(directoryToWatch, "file4.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .IncludingEmptyFile(pathToFileToUpdate2)
                .IncludingEmptyFile(pathToFileToUpdate3)
                .IncludingEmptyFile(pathToFileToUpdate4)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.Hidden);
                    Thread.Sleep(SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop);

                    // Act
                    watcher.EnableRaisingEvents = false;

                    fileSystem.File.SetAttributes(pathToFileToUpdate2, FileAttributes.Hidden);
                    Thread.Sleep(SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop);

                    watcher.EnableRaisingEvents = true;

                    fileSystem.File.SetAttributes(pathToFileToUpdate3, FileAttributes.Hidden);
                    Thread.Sleep(SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop);

                    watcher.EnableRaisingEvents = false;

                    fileSystem.File.SetAttributes(pathToFileToUpdate4, FileAttributes.Hidden);
                    Thread.Sleep(SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(@"
                        * file1.txt
                        * file3.txt
                        ".TrimLines());
                }
            }
        }

        [Fact]
        private void When_restarting_watcher_it_must_discard_old_notifications()
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
                watcher.EnableRaisingEvents = false;
                watcher.EnableRaisingEvents = true;

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
        private void When_disposing_unstarted_watcher_multiple_times_it_must_succeed()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            // Act
            FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch);

            // Act
            Action action = () =>
            {
                watcher.Dispose();
                watcher.Dispose();
            };

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        private void When_disposing_started_watcher_multiple_times_it_must_succeed()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, "file.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch);
            watcher.NotifyFilter = TestNotifyFilters.All;
            watcher.EnableRaisingEvents = true;

            fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);
            Thread.Sleep(SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop);

            // Act
            Action action = () =>
            {
                watcher.Dispose();
                watcher.Dispose();
            };

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        private void When_starting_disposed_watcher_it_must_fail()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher();
            watcher.Dispose();

            // Act
            Action action = () => watcher.EnableRaisingEvents = true;

            // Assert
            action.Should().Throw<ObjectDisposedException>().WithMessage("Cannot access a disposed object.*")
                .And.ObjectName.Should().Be("FileSystemWatcher");
        }
    }
}
#endif
