#if !NETCOREAPP1_1
using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryTimeLastAccessSpecs : WatcherSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2003).At(12, 34, 56).AsUtc();

        [Fact]
        private void When_getting_directory_last_access_time_in_local_zone_it_must_not_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            string directoryPath = Path.Combine(directoryToWatch, "Subfolder");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryPath)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetLastAccessTime(directoryPath);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_changing_directory_last_access_time_in_local_zone_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryName = "Subfolder";
            string directoryPath = Path.Combine(directoryToWatch, directoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryPath)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.SetLastAccessTime(directoryPath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(directoryPath);
                    args.Name.Should().Be(directoryName);
                }
            }
        }

        [Fact]
        private void When_changing_directory_last_access_time_in_local_zone_it_must_raise_events_for_last_access_time()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryName = "Subfolder";
            string directoryPath = Path.Combine(directoryToWatch, directoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryPath)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.SetLastAccessTime(directoryPath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(directoryPath);
                    args.Name.Should().Be(directoryName);
                }
            }
        }

        [Fact]
        private void When_changing_directory_last_access_time_in_local_zone_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryName = "Subfolder";
            string directoryPath = Path.Combine(directoryToWatch, directoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryPath)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.LastAccess);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.SetLastAccessTime(directoryPath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void
            When_changing_directory_last_access_time_in_local_zone_to_existing_value_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryName = "Subfolder";
            string directoryPath = Path.Combine(directoryToWatch, directoryName);

            var clock = new SystemClock(() => DefaultTimeUtc);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(directoryPath)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.SetLastAccessTime(directoryPath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(directoryPath);
                    args.Name.Should().Be(directoryName);
                }
            }
        }

        [Fact]
        private void
            When_changing_directory_last_access_time_in_local_zone_to_existing_value_it_must_raise_events_for_last_access_time()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryName = "Subfolder";
            string directoryPath = Path.Combine(directoryToWatch, directoryName);

            var clock = new SystemClock(() => DefaultTimeUtc);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(directoryPath)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.SetLastAccessTime(directoryPath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(directoryPath);
                    args.Name.Should().Be(directoryName);
                }
            }
        }

        [Fact]
        private void
            When_changing_directory_last_access_time_in_local_zone_to_existing_value_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryName = "Subfolder";
            string directoryPath = Path.Combine(directoryToWatch, directoryName);

            var clock = new SystemClock(() => DefaultTimeUtc);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(directoryPath)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.LastAccess);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.SetLastAccessTime(directoryPath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
