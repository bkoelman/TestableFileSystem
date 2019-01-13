#if !NETCOREAPP1_1
using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryMoveSpecs : WatcherSpecs
    {
        // TODO: Investigate impact on attributes
        // TODO: Investigate file rename & move

        [Fact]
        private void When_renaming_empty_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceDirectoryName = "MoveSource";
            const string destinationDirectoryName = "MoveTarget";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceDirectory = Path.Combine(pathToContainerDirectory, sourceDirectoryName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToSourceDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());

                    text.Should().Be(@"
                        * Container
                        > Container\MoveSource => Container\MoveTarget
                        * Container
                        ".TrimLines());
                }
            }
        }

        [Fact]
        private void When_renaming_empty_directory_it_must_raise_events_for_directory_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceDirectoryName = "MoveSource";
            const string destinationDirectoryName = "MoveTarget";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceDirectory = Path.Combine(pathToContainerDirectory, sourceDirectoryName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToSourceDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.DirectoryName;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    RenamedEventArgs args = listener.RenameEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                    args.FullPath.Should().Be(pathToDestinationDirectory);
                    args.Name.Should().Be(containerDirectoryName + @"\" + destinationDirectoryName);
                    args.OldFullPath.Should().Be(pathToSourceDirectory);
                    args.OldName.Should().Be(containerDirectoryName + @"\" + sourceDirectoryName);
                }
            }
        }

        [Fact]
        private void When_renaming_empty_directory_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceDirectoryName = "MoveSource";
            const string destinationDirectoryName = "MoveTarget";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceDirectory = Path.Combine(pathToContainerDirectory, sourceDirectoryName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToSourceDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToContainerDirectory);
                    args.Name.Should().Be(containerDirectoryName);
                }
            }
        }

        [Fact]
        private void When_renaming_empty_directory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceDirectoryName = "MoveSource";
            const string destinationDirectoryName = "MoveTarget";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceDirectory = Path.Combine(pathToContainerDirectory, sourceDirectoryName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToSourceDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());

                    text.Should().Be(@"
                        * Container
                        * Container
                        ".TrimLines());
                }
            }
        }

        // TODO: When_renaming_directory_tree (same events as above)

        // TODO: When_moving_directory_tree_to_parent_directory

        // TODO: When_moving_directory_tree_to_subdirectory
    }
}
#endif
