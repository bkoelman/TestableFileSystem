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
    public sealed class NotifyFileTimeCreationSpecs : WatcherSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2003).At(12, 34, 56).AsUtc();

        [Fact]
        private void When_getting_file_creation_time_in_local_zone_it_must_not_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            string filePath = Path.Combine(directoryToWatch, "file.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(filePath, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.GetCreationTime(filePath);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_changing_file_creation_time_in_local_zone_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileName = "file.txt";
            string filePath = Path.Combine(directoryToWatch, fileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(filePath, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetCreationTime(filePath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(filePath);
                    args.Name.Should().Be(fileName);
                }
            }
        }

        [Fact]
        private void When_changing_file_creation_time_in_local_zone_it_must_raise_events_for_creation_time()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileName = "file.txt";
            string filePath = Path.Combine(directoryToWatch, fileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(filePath, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.CreationTime;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetCreationTime(filePath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(filePath);
                    args.Name.Should().Be(fileName);
                }
            }
        }

        [Fact]
        private void When_changing_file_creation_time_in_local_zone_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileName = "file.txt";
            string filePath = Path.Combine(directoryToWatch, fileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(filePath, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.CreationTime);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetCreationTime(filePath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void
            When_changing_file_creation_time_in_local_zone_to_existing_value_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileName = "file.txt";
            string filePath = Path.Combine(directoryToWatch, fileName);

            var clock = new SystemClock(() => DefaultTimeUtc);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(filePath, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetCreationTime(filePath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(filePath);
                    args.Name.Should().Be(fileName);
                }
            }
        }

        [Fact]
        private void When_changing_file_creation_time_in_local_zone_to_existing_value_it_must_raise_events_for_creation_time()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileName = "file.txt";
            string filePath = Path.Combine(directoryToWatch, fileName);

            var clock = new SystemClock(() => DefaultTimeUtc);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(filePath, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.CreationTime;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetCreationTime(filePath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(filePath);
                    args.Name.Should().Be(fileName);
                }
            }
        }

        [Fact]
        private void
            When_changing_file_creation_time_in_local_zone_to_existing_value_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileName = "file.txt";
            string filePath = Path.Combine(directoryToWatch, fileName);

            var clock = new SystemClock(() => DefaultTimeUtc);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(filePath, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.CreationTime);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetCreationTime(filePath, DefaultTimeUtc.ToLocalTime());

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
