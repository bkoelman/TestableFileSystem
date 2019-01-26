#if !NETCOREAPP1_1
using System.IO;
using System.Threading;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class InternalBufferSizeSpecs : WatcherSpecs
    {
        // TODO: Add basic specs for buffer size.

        //   - Test for buffer overflow: Raises Error event; "If the buffer overflows, the entire contents of the buffer is discarded"
        //   - "If there are many changes in a short time, the buffer can overflow. This causes the component to lose track of changes
        //       in the directory, and it will only provide blanket notification."

        // MSDN: You can set the buffer to 4 KB or larger, but it must not exceed 64 KB. If you try to set the InternalBufferSize property to less than 4096 bytes, 
        // your value is discarded and the InternalBufferSize property is set to 4096 bytes. For best performance, use a multiple of 4 KB on Intel-based computers.

        [Fact]
        private void When_changing_InternalBufferSize_on_running_watcher_it_must_discard_old_notifications_and_restart()
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
                watcher.InternalBufferSize = watcher.InternalBufferSize * 2;

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
        private void When_setting_InternalBufferSize_on_disposed_watcher_it_must_succeed()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher();
            watcher.Dispose();

            // Act
            watcher.InternalBufferSize = 4099;

            // Assert
            watcher.InternalBufferSize.Should().Be(4099);
        }
    }
}
#endif
