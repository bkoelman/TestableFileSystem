#if !NETCOREAPP1_1
using System.IO;
using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class WatcherNotifyFilterSpecs
    {
        private const int NotifyWaitTimeoutMilliseconds = 500;

        [Fact]
        private void When_creating_file_it_must_raise_event_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToCreate = "file.txt";
            string pathToFileToCreate = Path.Combine(directoryToWatch, fileNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(pathToFileToCreate))
                    {
                    }

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(pathToFileToCreate);
                    args.Name.Should().Be(fileNameToCreate);
                }
            }
        }

        [Fact]
        private void When_creating_file_it_must_not_raise_event_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToCreate = "file.txt";
            string pathToFileToCreate = Path.Combine(directoryToWatch, fileNameToCreate);

            NotifyFilters filtersExceptFileName = TestNotifyFilters.All & ~NotifyFilters.FileName;

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filtersExceptFileName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(pathToFileToCreate))
                    {
                    }

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_deleting_file_it_must_raise_event_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToDelete = "file.txt";
            string pathToFileToDelete = Path.Combine(directoryToWatch, fileNameToDelete);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToDelete)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Delete(pathToFileToDelete);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.DeleteEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    args.FullPath.Should().Be(pathToFileToDelete);
                    args.Name.Should().Be(fileNameToDelete);
                }
            }
        }

        [Fact]
        private void When_deleting_file_it_must_not_raise_event_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToDelete = "file.txt";
            string pathToFileToDelete = Path.Combine(directoryToWatch, fileNameToDelete);

            NotifyFilters filtersExceptFileName = TestNotifyFilters.All & ~NotifyFilters.FileName;

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToDelete)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filtersExceptFileName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Delete(pathToFileToDelete);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_appending_to_file_it_must_raise_event_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToAppend = "file.txt";
            string pathToFileToAppend = Path.Combine(directoryToWatch, fileNameToAppend);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToAppend)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.AppendAllText(pathToFileToAppend, "Extra");

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToAppend);
                    args.Name.Should().Be(fileNameToAppend);
                }
            }
        }

        [Fact]
        private void When_appending_to_file_it_must_raise_event_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToAppend = "file.txt";
            string pathToFileToAppend = Path.Combine(directoryToWatch, fileNameToAppend);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToAppend)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.AppendAllText(pathToFileToAppend, "Extra");

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToAppend);
                    args.Name.Should().Be(fileNameToAppend);
                }
            }
        }

        [Fact]
        private void When_appending_to_file_it_must_raise_event_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToAppend = "file.txt";
            string pathToFileToAppend = Path.Combine(directoryToWatch, fileNameToAppend);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToAppend)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.AppendAllText(pathToFileToAppend, "Extra");

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToAppend);
                    args.Name.Should().Be(fileNameToAppend);
                }
            }
        }

        [Fact]
        private void When_appending_to_file_it_must_raise_event_for_size()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToAppend = "file.txt";
            string pathToFileToAppend = Path.Combine(directoryToWatch, fileNameToAppend);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToAppend)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Size;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.AppendAllText(pathToFileToAppend, "Extra");

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToAppend);
                    args.Name.Should().Be(fileNameToAppend);
                }
            }
        }

        [Fact]
        private void When_appending_to_file_it_must_not_raise_event_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToAppend = "file.txt";
            string pathToFileToAppend = Path.Combine(directoryToWatch, fileNameToAppend);

            const NotifyFilters lastAccessLastWriteSize = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;
            NotifyFilters otherFilters = TestNotifyFilters.All & ~lastAccessLastWriteSize;

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToAppend)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = otherFilters;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.AppendAllText(pathToFileToAppend, "Extra");

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_overwriting_file_with_same_contents_it_must_raise_event_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToOverwrite = "file.txt";
            string pathToFileToOverwrite = Path.Combine(directoryToWatch, fileNameToOverwrite);

            const string fileContents = "ExampleText";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToOverwrite, fileContents)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.WriteAllText(pathToFileToOverwrite, fileContents);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToOverwrite);
                    args.Name.Should().Be(fileNameToOverwrite);
                }
            }
        }

        [Fact]
        private void When_overwriting_file_with_same_contents_it_must_raise_event_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToOverwrite = "file.txt";
            string pathToFileToOverwrite = Path.Combine(directoryToWatch, fileNameToOverwrite);

            const string fileContents = "ExampleText";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToOverwrite, fileContents)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.WriteAllText(pathToFileToOverwrite, fileContents);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToOverwrite);
                    args.Name.Should().Be(fileNameToOverwrite);
                }
            }
        }

        [Fact]
        private void When_overwriting_file_with_same_contents_it_must_raise_event_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToOverwrite = "file.txt";
            string pathToFileToOverwrite = Path.Combine(directoryToWatch, fileNameToOverwrite);

            const string fileContents = "ExampleText";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToOverwrite, fileContents)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.WriteAllText(pathToFileToOverwrite, fileContents);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToOverwrite);
                    args.Name.Should().Be(fileNameToOverwrite);
                }
            }
        }

        [Fact]
        private void When_overwriting_file_with_same_contents_it_must_not_raise_event_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToOverwrite = "file.txt";
            string pathToFileToOverwrite = Path.Combine(directoryToWatch, fileNameToOverwrite);

            const NotifyFilters lastAccessLastWrite = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            NotifyFilters otherFilters = TestNotifyFilters.All & ~lastAccessLastWrite;

            const string fileContents = "ExampleText";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToOverwrite, fileContents)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = otherFilters;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.WriteAllText(pathToFileToOverwrite, fileContents);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_truncating_existing_file_it_must_raise_event_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToTruncate = "file.txt";
            string pathToFileToTruncate = Path.Combine(directoryToWatch, fileNameToTruncate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToTruncate, "InitialText")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.WriteAllText(pathToFileToTruncate, string.Empty);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToTruncate);
                    args.Name.Should().Be(fileNameToTruncate);
                }
            }
        }

        [Fact]
        private void When_truncating_existing_file_it_must_raise_event_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToTruncate = "file.txt";
            string pathToFileToTruncate = Path.Combine(directoryToWatch, fileNameToTruncate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToTruncate, "InitialText")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.WriteAllText(pathToFileToTruncate, string.Empty);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToTruncate);
                    args.Name.Should().Be(fileNameToTruncate);
                }
            }
        }

        [Fact]
        private void When_truncating_existing_file_it_must_raise_event_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToTruncate = "file.txt";
            string pathToFileToTruncate = Path.Combine(directoryToWatch, fileNameToTruncate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToTruncate, "InitialText")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.WriteAllText(pathToFileToTruncate, string.Empty);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToTruncate);
                    args.Name.Should().Be(fileNameToTruncate);
                }
            }
        }

        [Fact]
        private void When_truncating_existing_file_it_must_raise_event_for_size()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToTruncate = "file.txt";
            string pathToFileToTruncate = Path.Combine(directoryToWatch, fileNameToTruncate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToTruncate, "InitialText")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Size;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.WriteAllText(pathToFileToTruncate, string.Empty);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToTruncate);
                    args.Name.Should().Be(fileNameToTruncate);
                }
            }
        }

        [Fact]
        private void When_truncating_existing_file_it_must_not_raise_event_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToTruncate = "file.txt";
            string pathToFileToTruncate = Path.Combine(directoryToWatch, fileNameToTruncate);

            const NotifyFilters lastAccessLastWriteSize = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;
            NotifyFilters otherFilters = TestNotifyFilters.All & ~lastAccessLastWriteSize;

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToTruncate, "InitialText")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = otherFilters;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.WriteAllText(pathToFileToTruncate, string.Empty);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_recreating_existing_empty_file_it_must_raise_event_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToRecreate = "file.txt";
            string pathToFileToRecreate = Path.Combine(directoryToWatch, fileNameToRecreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToRecreate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(pathToFileToRecreate))
                    {
                    }

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToRecreate);
                    args.Name.Should().Be(fileNameToRecreate);
                }
            }
        }

        [Fact]
        private void When_recreating_existing_empty_file_it_must_raise_event_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToRecreate = "file.txt";
            string pathToFileToRecreate = Path.Combine(directoryToWatch, fileNameToRecreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToRecreate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(pathToFileToRecreate))
                    {
                    }

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToRecreate);
                    args.Name.Should().Be(fileNameToRecreate);
                }
            }
        }

        [Fact]
        private void When_recreating_existing_empty_file_it_must_raise_event_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToRecreate = "file.txt";
            string pathToFileToRecreate = Path.Combine(directoryToWatch, fileNameToRecreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToRecreate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(pathToFileToRecreate))
                    {
                    }

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToRecreate);
                    args.Name.Should().Be(fileNameToRecreate);
                }
            }
        }

        [Fact]
        private void When_recreating_existing_empty_file_it_must_not_raise_event_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToRecreate = "file.txt";
            string pathToFileToRecreate = Path.Combine(directoryToWatch, fileNameToRecreate);

            const NotifyFilters lastAccessLastWrite = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            NotifyFilters otherFilters = TestNotifyFilters.All & ~lastAccessLastWrite;

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToRecreate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = otherFilters;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(pathToFileToRecreate))
                    {
                    }

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        // TODO: Add tests for:
        // - Change attributes
        // - Copy
        // - Move
        // - Change create/lastwrite/lastaccess times
    }
}
#endif
