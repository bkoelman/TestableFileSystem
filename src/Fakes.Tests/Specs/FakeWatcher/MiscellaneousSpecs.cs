#if !NETCOREAPP1_1
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class MiscellaneousSpecs : WatcherSpecs
    {
        [Fact]
        private void When_watcher_directory_is_deleted_it_must_raise_error_event_and_terminate()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.Error += (sender, args) =>
                {
                    Exception exception = args.GetException();

                    // Assert
                    exception.Should().NotBeNull();
                    exception.Should().BeOfType<Win32Exception>().Subject.NativeErrorCode.Should().Be(5);
                    exception.Message.Should().Be("Access is denied");
                };

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(directoryToWatch);

                    Thread.Sleep(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    watcher.EnableRaisingEvents.Should().BeFalse();

                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(@"
                        ! Access is denied
                        ".TrimLines());
                }
            }
        }

        [Fact]
        private void When_parent_of_watcher_directory_is_deleted_it_must_raise_error_event_and_terminate()
        {
            // Arrange
            const string directoryToWatch = @"c:\parent\watchFolder";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.Error += (sender, args) =>
                {
                    Exception exception = args.GetException();

                    // Assert
                    exception.Should().NotBeNull();
                    exception.Should().BeOfType<Win32Exception>().Subject.NativeErrorCode.Should().Be(5);
                    exception.Message.Should().Be("Access is denied");
                };

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(@"c:\parent", true);

                    Thread.Sleep(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    watcher.EnableRaisingEvents.Should().BeFalse();

                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(@"
                        ! Access is denied
                        ".TrimLines());
                }
            }
        }

        [Fact]
        private void When_multiple_watchers_are_attached_to_the_same_file_system_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher1 = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher1.NotifyFilter = TestNotifyFilters.All;

                string text1;
                string text2;

                using (var listener1 = new FileSystemWatcherEventListener(watcher1))
                {
                    fileSystem.File.SetLastWriteTimeUtc(pathToFileToUpdate, 1.January(2002));

                    using (FakeFileSystemWatcher watcher2 = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
                    {
                        watcher2.NotifyFilter = TestNotifyFilters.All;

                        using (var listener2 = new FileSystemWatcherEventListener(watcher2))
                        {
                            fileSystem.File.SetLastWriteTimeUtc(pathToFileToUpdate, 2.January(2002));

                            watcher2.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                            text2 = string.Join(Environment.NewLine, listener2.GetEventsCollectedAsText());
                        }
                    }

                    fileSystem.File.SetLastWriteTimeUtc(pathToFileToUpdate, 3.January(2002));

                    watcher1.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    text1 = string.Join(Environment.NewLine, listener1.GetEventsCollectedAsText());
                }

                // Assert
                text1.Should().Be(@"
                        * file.txt
                        * file.txt
                        * file.txt
                        ".TrimLines());

                text2.Should().Be(@"
                        * file.txt
                        ".TrimLines());
            }
        }

        [Fact]
        private void When_watchers_are_attached_to_different_file_systems_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate1 = "file1.txt";
            const string fileNameToUpdate2 = "file2.txt";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch, fileNameToUpdate1);
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch, fileNameToUpdate2);

            FakeFileSystem fileSystem1 = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .Build();

            FakeFileSystem fileSystem2 = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate2)
                .Build();

            using (FakeFileSystemWatcher watcher1 = fileSystem1.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher1.NotifyFilter = TestNotifyFilters.All;

                string text1;
                string text2;

                using (var listener1 = new FileSystemWatcherEventListener(watcher1))
                {
                    fileSystem1.File.SetLastWriteTimeUtc(pathToFileToUpdate1, 1.January(2002));

                    using (FakeFileSystemWatcher watcher2 = fileSystem2.ConstructFileSystemWatcher(directoryToWatch))
                    {
                        watcher2.NotifyFilter = TestNotifyFilters.All;

                        using (var listener2 = new FileSystemWatcherEventListener(watcher2))
                        {
                            fileSystem2.File.SetLastWriteTimeUtc(pathToFileToUpdate2, 2.January(2002));

                            watcher2.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                            text2 = string.Join(Environment.NewLine, listener2.GetEventsCollectedAsText());
                        }
                    }

                    fileSystem1.File.SetLastWriteTimeUtc(pathToFileToUpdate1, 3.January(2002));

                    watcher1.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    text1 = string.Join(Environment.NewLine, listener1.GetEventsCollectedAsText());
                }

                // Assert
                text1.Should().Be(@"
                        * file1.txt
                        * file1.txt
                        ".TrimLines());

                text2.Should().Be(@"
                        * file2.txt
                        ".TrimLines());
            }
        }
    }
}
#endif
