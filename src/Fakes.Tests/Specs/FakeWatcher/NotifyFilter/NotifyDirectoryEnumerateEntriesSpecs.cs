#if !NETCOREAPP1_1
using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryEnumerateEntriesSpecs : WatcherSpecs
    {
        #region Non-recursive

        [Fact]
        private void When_enumerating_entries_for_empty_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToEnumerate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_for_empty_directory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToEnumerate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_for_empty_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToEnumerate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_for_non_empty_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_for_non_empty_directory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_for_non_empty_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_for_directory_tree_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "Subfolder", "SubFile.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_for_directory_tree_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "Subfolder", "SubFile.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_for_directory_tree_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "Subfolder", "SubFile.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        #endregion

        #region Pattern

        [Fact]
        private void
            When_enumerating_entries_with_matching_pattern_for_non_empty_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate, "*.txt");

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_with_matching_pattern_for_non_empty_directory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate, "*.txt");

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void
            When_enumerating_entries_with_matching_pattern_for_non_empty_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate, "*.txt");

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void
            When_enumerating_entries_with_non_matching_pattern_for_non_empty_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.doc"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate, "*.txt");

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void
            When_enumerating_entries_with_non_matching_pattern_for_non_empty_directory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.doc"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate, "*.txt");

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void
            When_enumerating_entries_with_non_matching_pattern_for_non_empty_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.doc"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate, "*.txt");

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        #endregion

        #region Recursive

        [Fact]
        private void When_enumerating_entries_recursively_for_empty_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToEnumerate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate,
                        searchOption: SearchOption.AllDirectories);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_recursively_for_empty_directory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToEnumerate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate,
                        searchOption: SearchOption.AllDirectories);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_recursively_for_empty_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToEnumerate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate,
                        searchOption: SearchOption.AllDirectories);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_recursively_for_non_empty_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate,
                        searchOption: SearchOption.AllDirectories);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_recursively_for_non_empty_directory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate,
                        searchOption: SearchOption.AllDirectories);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDirectoryToEnumerate);
                    args.Name.Should().Be(directoryNameToEnumerate);
                }
            }
        }

        [Fact]
        private void
            When_enumerating_entries_recursively_for_non_empty_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate,
                        searchOption: SearchOption.AllDirectories);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_enumerating_entries_recursively_for_directory_tree_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, "EnumerateMe");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FolderA\SubFolderA"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FolderA\FileInA.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FolderB\FileInB.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FolderC\SubFolderC\FileInSubFolderC.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FileInRoot2.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FileInRoot1.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate, searchOption: SearchOption.AllDirectories);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());

                    text.Should().Be(@"
                        * EnumerateMe
                        * EnumerateMe\TopLevel
                        * EnumerateMe\TopLevel\FolderA
                        * EnumerateMe\TopLevel\FolderA\SubFolderA
                        * EnumerateMe\TopLevel\FolderB
                        * EnumerateMe\TopLevel\FolderC
                        * EnumerateMe\TopLevel\FolderC\SubFolderC
                        ".TrimLines());
                }
            }
        }

        #endregion
    }
}
#endif
