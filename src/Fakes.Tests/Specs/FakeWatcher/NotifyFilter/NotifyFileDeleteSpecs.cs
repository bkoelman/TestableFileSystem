#if !NETCOREAPP1_1
using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyFileDeleteSpecs : WatcherSpecs
    {
        [Theory]
        [WatcherNotifyTestData(@"
            - Container\file.txt                            @ FileName
        ")]
        private void When_deleting_file_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string fileNameToDelete = "file.txt";

            string pathToFileToDelete = Path.Combine(directoryToWatch, containerDirectoryName, fileNameToDelete);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToDelete)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Delete(pathToFileToDelete);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Fact]
        private void When_changing_notify_filter_on_running_watcher_it_must_discard_old_notifications_and_restart()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToDelete1 = Path.Combine(directoryToWatch, "file1.txt");
            string pathToFileToDelete2 = Path.Combine(directoryToWatch, "file2.txt");
            string pathToFileToDelete3 = Path.Combine(directoryToWatch, "file3.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToDelete1)
                .IncludingEmptyFile(pathToFileToDelete2)
                .IncludingEmptyFile(pathToFileToDelete3)
                .Build();

            var lockObject = new object();
            bool isFirstEventInvocation = true;
            FileSystemEventArgs argsAfterRestart = null;

            var resumeEventHandlerWaitHandle = new ManualResetEventSlim(false);
            var testCompletionWaitHandle = new ManualResetEventSlim(false);

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Attributes;
                watcher.Deleted += (sender, args) =>
                {
                    lock (lockObject)
                    {
                        if (isFirstEventInvocation)
                        {
                            // Wait for delete notifications on all files to queue up.
                            resumeEventHandlerWaitHandle.Wait(MaxTestDurationInMilliseconds);
                            isFirstEventInvocation = false;
                        }
                        else
                        {
                            // After event handler for file1 has completed, no event for
                            // file2 should be raised because it has become outdated.
                            argsAfterRestart = args;
                            testCompletionWaitHandle.Set();
                        }
                    }
                };
                watcher.EnableRaisingEvents = true;

                fileSystem.File.Delete(pathToFileToDelete1);
                fileSystem.File.Delete(pathToFileToDelete2);
                Thread.Sleep(SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop);

                // Act
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;

                fileSystem.File.Delete(pathToFileToDelete3);

                resumeEventHandlerWaitHandle.Set();
                testCompletionWaitHandle.Wait(MaxTestDurationInMilliseconds);

                lock (lockObject)
                {
                    // Assert
                    FileSystemEventArgs argsAfterRestartNotNull = argsAfterRestart.ShouldNotBeNull();
                    argsAfterRestartNotNull.Name.Should().Be("file3.txt");
                }
            }
        }
    }
}
#endif
