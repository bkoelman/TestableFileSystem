#if !NETCOREAPP1_1
using System.IO;
using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryCreateSpecs : WatcherSpecs
    {
        [Fact]
        private void When_creating_existing_directory_it_must_not_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = "Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToCreate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_creating_missing_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = "Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(pathToDirectoryToCreate);
                    args.Name.Should().Be(directoryNameToCreate);
                }
            }
        }

        [Fact]
        private void When_creating_missing_directory_it_must_raise_events_for_directory_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = "Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.DirectoryName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(pathToDirectoryToCreate);
                    args.Name.Should().Be(directoryNameToCreate);
                }
            }
        }

        [Fact]
        private void When_creating_missing_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = "Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.DirectoryName);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_creating_existing_directory_tree_it_must_not_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = @"Deeper\Nested\Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToCreate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_creating_partially_missing_directory_tree_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = @"Deeper\Nested\Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(Path.Combine(directoryToWatch, "DEEPER"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.CreateEventArgsCollected.Should().HaveCount(2);

                    FileSystemEventArgs[] argsArray = listener.CreateEventArgsCollected.OrderBy(x => x.Name).ToArray();

                    argsArray[0].ChangeType.Should().Be(WatcherChangeTypes.Created);
                    argsArray[0].FullPath.Should().Be(Path.Combine(directoryToWatch, @"DEEPER\Nested"));
                    argsArray[0].Name.Should().Be(@"DEEPER\Nested");

                    argsArray[1].ChangeType.Should().Be(WatcherChangeTypes.Created);
                    argsArray[1].FullPath.Should().Be(Path.Combine(directoryToWatch, @"DEEPER\Nested\Subfolder"));
                    argsArray[1].Name.Should().Be(@"DEEPER\Nested\Subfolder");
                }
            }
        }

        [Fact]
        private void When_creating_partially_missing_directory_tree_it_must_raise_events_for_directory_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = @"Deeper\Nested\Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(Path.Combine(directoryToWatch, "DEEPER"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.DirectoryName;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.CreateEventArgsCollected.Should().HaveCount(2);

                    FileSystemEventArgs[] argsArray = listener.CreateEventArgsCollected.OrderBy(x => x.Name).ToArray();

                    argsArray[0].ChangeType.Should().Be(WatcherChangeTypes.Created);
                    argsArray[0].FullPath.Should().Be(Path.Combine(directoryToWatch, @"DEEPER\Nested"));
                    argsArray[0].Name.Should().Be(@"DEEPER\Nested");

                    argsArray[1].ChangeType.Should().Be(WatcherChangeTypes.Created);
                    argsArray[1].FullPath.Should().Be(Path.Combine(directoryToWatch, @"DEEPER\Nested\Subfolder"));
                    argsArray[1].Name.Should().Be(@"DEEPER\Nested\Subfolder");
                }
            }
        }

        [Fact]
        private void When_creating_partially_missing_directory_tree_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = @"Deeper\Nested\Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(Path.Combine(directoryToWatch, "DEEPER"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.DirectoryName);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
