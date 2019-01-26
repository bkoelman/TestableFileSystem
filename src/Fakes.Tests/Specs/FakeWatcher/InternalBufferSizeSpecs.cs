#if !NETCOREAPP1_1
using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class InternalBufferSizeSpecs : WatcherSpecs
    {
        // TODO: Test for "If the buffer overflows, the entire contents of the buffer is discarded"

        [Fact]
        private void When_setting_InternalBufferSize_to_low_value_it_must_correct_value()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                watcher.InternalBufferSize = 1;

                // Assert
                watcher.InternalBufferSize.Should().Be(4096);
            }
        }

        [Fact]
        private void When_setting_InternalBufferSize_to_high_value_it_must_store_value()
        {
            // Arrange
            const int oneMegabyte = 1024 * 1024;

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                watcher.InternalBufferSize = oneMegabyte;

                // Assert
                watcher.InternalBufferSize.Should().Be(oneMegabyte);
            }
        }

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

        [Fact]
        private void When_buffer_overflows_it_must_raise_error_event()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, "file.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            DateTime startTime = DateTime.UtcNow;

            var lockObject = new object();
            ErrorEventArgs errorArgs = null;

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.InternalBufferSize = 1;

                watcher.Changed += (sender, args) => { Thread.Sleep(100); };
                watcher.Error += (sender, args) =>
                {
                    lock (lockObject)
                    {
                        errorArgs = args;
                    }
                };
                watcher.EnableRaisingEvents = true;

                // Act
                while (startTime + TimeSpan.FromSeconds(3) > DateTime.UtcNow)
                {
                    fileSystem.File.SetCreationTimeUtc(pathToFileToUpdate, 1.January(2001));

                    lock (lockObject)
                    {
                        if (errorArgs != null)
                        {
                            break;
                        }
                    }
                }

                lock (lockObject)
                {
                    // Assert
                    errorArgs.Should().NotBeNull();
                    Exception exception = errorArgs.GetException();

                    exception.Should().BeOfType<InternalBufferOverflowException>()
                        .Subject.Message.Should().Be(@"Too many changes at once in directory:c:\some.");
                }
            }
        }
    }
}
#endif
