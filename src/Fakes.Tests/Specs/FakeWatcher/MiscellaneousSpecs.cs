#if !NETCOREAPP1_1
using System;
using System.ComponentModel;
using System.Threading;
using FluentAssertions;
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

        // TODO: Multiple watchers attached to the same file system
    }
}
#endif
