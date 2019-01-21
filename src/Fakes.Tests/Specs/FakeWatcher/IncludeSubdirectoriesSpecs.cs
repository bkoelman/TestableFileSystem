#if !NETCOREAPP1_1
using System.IO;
using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class IncludeSubdirectoriesSpecs : WatcherSpecs
    {
        [Fact]
        private void When_changing_attributes_in_subdirectory_with_IncludeSubdirectories_enabled_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string directoryNameToUpdate = "TargetFolder";

            string pathToDirectoryToUpdate = Path.Combine(directoryToWatch, containerDirectoryName, directoryNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate, FileAttributes.ReadOnly);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToUpdate);
                    args.Name.Should().Be(containerDirectoryName + @"\" + directoryNameToUpdate);
                }
            }
        }

        [Fact]
        private void When_changing_attributes_in_subdirectory_with_IncludeSubdirectories_disabled_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string directoryNameToUpdate = "TargetFolder";

            string pathToDirectoryToUpdate = Path.Combine(directoryToWatch, containerDirectoryName, directoryNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = false;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate, FileAttributes.ReadOnly);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_changing_attributes_in_watched_directory_with_IncludeSubdirectories_disabled_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToUpdate = "TargetFolder";

            string pathToDirectoryToUpdate = Path.Combine(directoryToWatch, directoryNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = false;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate, FileAttributes.ReadOnly);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToUpdate);
                    args.Name.Should().Be(directoryNameToUpdate);
                }
            }
        }

        [Fact]
        private void When_changing_attributes_of_watched_directory_with_IncludeSubdirectories_enabled_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string pathToDirectoryToUpdate = directoryToWatch;

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate, FileAttributes.ReadOnly);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
