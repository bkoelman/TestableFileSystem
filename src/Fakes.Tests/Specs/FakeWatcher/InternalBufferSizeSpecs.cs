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
        private static readonly TimeSpan SpecTimeout = TimeSpan.FromSeconds(3);

        // Unable to reproduce these documented constraints:
        // MSDN: ReadDirectoryChangesW fails with ERROR_NOACCESS when the buffer is not aligned on a DWORD boundary.
        // MSDN: ReadDirectoryChangesW fails with ERROR_INVALID_PARAMETER when the buffer length is greater than 64 KB and the application is
        //       monitoring a directory over the network. This is due to a packet size limitation with the underlying file sharing protocols.

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
                bool signaled = testCompletionEvent.WaitOne(SpecTimeout);

                signaled.Should().BeTrue();

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
            ErrorEventArgs firstErrorArgs = null;

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.InternalBufferSize = 1;
                watcher.Changed += (sender, args) => { Thread.Sleep(100); };
                watcher.Error += (sender, args) =>
                {
                    lock (lockObject)
                    {
                        if (firstErrorArgs == null)
                        {
                            firstErrorArgs = args;
                        }
                    }
                };
                watcher.EnableRaisingEvents = true;

                // Act
                while (startTime + SpecTimeout > DateTime.UtcNow)
                {
                    fileSystem.File.SetCreationTimeUtc(pathToFileToUpdate, 1.January(2001));

                    lock (lockObject)
                    {
                        if (firstErrorArgs != null)
                        {
                            break;
                        }
                    }
                }

                lock (lockObject)
                {
                    // Assert
                    firstErrorArgs.Should().NotBeNull();
                    Exception exception = firstErrorArgs.GetException();

                    exception.Should().BeOfType<InternalBufferOverflowException>()
                        .Subject.Message.Should().Be(@"Too many changes at once in directory:c:\some.");
                }
            }
        }

        [Fact]
        private void When_buffer_overflows_it_must_discard_old_notifications_and_continue()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdateBefore = Path.Combine(directoryToWatch, "file-before.txt");
            string pathToFileToUpdateAfter = Path.Combine(directoryToWatch, "file-after.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdateBefore)
                .IncludingEmptyFile(pathToFileToUpdateAfter)
                .Build();

            var lockObject = new object();
            bool isAfterBufferOverflow = false;
            FileSystemEventArgs firstChangeArgsAfterBufferOverflow = null;
            int changeCount = 0;
            int sleepCount = 0;

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.InternalBufferSize = 1;
                watcher.Changed += (sender, args) =>
                {
                    lock (lockObject)
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        if (!isAfterBufferOverflow)
                        {
                            // Block the change handler shortly, allowing the buffer to fill and overflow. Do not block
                            // the handler indefinitely, because that would prevent the error handler from getting raised.
                            Thread.Sleep(250);
                            Interlocked.Increment(ref sleepCount);
                        }
                        else
                        {
                            // Capture the first args after buffer overflow, so we can assert on main thread.
                            if (firstChangeArgsAfterBufferOverflow == null)
                            {
                                firstChangeArgsAfterBufferOverflow = args;
                            }
                        }
                    }
                };
                watcher.Error += (sender, args) =>
                {
                    lock (lockObject)
                    {
                        if (args.GetException() is InternalBufferOverflowException)
                        {
                            isAfterBufferOverflow = true;
                        }
                    }
                };
                watcher.EnableRaisingEvents = true;

                DateTime startTime = DateTime.UtcNow;
                bool timedOut = false;

                // Act (cause buffer to overflow)
                while (!timedOut)
                {
                    lock (lockObject)
                    {
                        if (!isAfterBufferOverflow)
                        {
                            fileSystem.File.SetCreationTimeUtc(pathToFileToUpdateBefore, 1.January(2001));
                            Interlocked.Increment(ref changeCount);
                        }
                        else
                        {
                            break;
                        }
                    }

                    timedOut = startTime + SpecTimeout <= DateTime.UtcNow;
                }

                if (timedOut)
                {
                    throw new Exception($"Exception in test: changeCount={changeCount}, sleepCount={sleepCount}.");
                }

                timedOut.Should().BeFalse();

                lock (lockObject)
                {
                    isAfterBufferOverflow = true;
                }

                // Wait for watcher to flush its queue.
                Thread.Sleep(NotifyWaitTimeoutMilliseconds);

                // Extra event after buffer overflow, which should be processed normally.
                fileSystem.File.SetCreationTimeUtc(pathToFileToUpdateAfter, 1.January(2001));

                watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);
            }

            lock (lockObject)
            {
                // Assert
                firstChangeArgsAfterBufferOverflow.Should().NotBeNull();
                firstChangeArgsAfterBufferOverflow.Name.Should().Be("file-after.txt");
            }
        }
    }
}
#endif
