using System.IO;
using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

#if !NETCOREAPP1_1
namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryDeleteSpecs : WatcherSpecs
    {
        [Fact]
        private void When_deleting_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToDelete = "Subfolder";
            string pathToDirectoryToDelete = Path.Combine(directoryToWatch, directoryNameToDelete);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToDelete)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.DeleteEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    args.FullPath.Should().Be(pathToDirectoryToDelete);
                    args.Name.Should().Be(directoryNameToDelete);
                }
            }
        }

        [Fact]
        private void When_deleting_directory_it_must_raise_events_for_directory_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToDelete = "Subfolder";
            string pathToDirectoryToDelete = Path.Combine(directoryToWatch, directoryNameToDelete);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToDelete)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.DirectoryName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.DeleteEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    args.FullPath.Should().Be(pathToDirectoryToDelete);
                    args.Name.Should().Be(directoryNameToDelete);
                }
            }
        }

        [Fact]
        private void When_deleting_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToDelete = "Subfolder";
            string pathToDirectoryToDelete = Path.Combine(directoryToWatch, directoryNameToDelete);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToDelete)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.DirectoryName);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
