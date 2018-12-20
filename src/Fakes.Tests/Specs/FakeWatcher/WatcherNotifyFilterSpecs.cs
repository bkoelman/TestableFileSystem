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

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.FileName);

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

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToDelete)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.FileName);

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

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToAppend)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size);

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

            const string fileContents = "ExampleText";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToOverwrite, fileContents)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.LastAccess | NotifyFilters.LastWrite);

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

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToTruncate, "InitialText")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size);

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

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToRecreate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.LastAccess | NotifyFilters.LastWrite);

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

        [Fact]
        private void When_changing_file_attributes_it_must_raise_event_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";
            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToUpdate);
                    args.Name.Should().Be(fileNameToUpdate);
                }
            }
        }

        [Fact]
        private void When_changing_file_attributes_it_must_raise_event_for_attributes()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";
            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Attributes;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToUpdate);
                    args.Name.Should().Be(fileNameToUpdate);
                }
            }
        }

        [Fact]
        private void When_changing_file_attributes_it_must_not_raise_event_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";
            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.Attributes);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_changing_file_attributes_to_existing_value_it_must_not_raise_event_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";
            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate, FileAttributes.ReadOnly)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(0);
                }
            }
        }

        // TODO: When_copying_file...

        [Fact(Skip = "TODO")]
        private void When_copying_file_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "Example")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);
                    listener.CreateEventArgsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveCount(2);

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(destinationFileName);

                    FileSystemEventArgs changeArgs1 = listener.ChangeEventArgsCollected.First();
                    changeArgs1.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs1.FullPath.Should().Be(pathToDestinationFile);
                    changeArgs1.Name.Should().Be(destinationFileName);

                    FileSystemEventArgs changeArgs2 = listener.ChangeEventArgsCollected.Skip(1).Single();
                    changeArgs2.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs2.FullPath.Should().Be(pathToDestinationFile);
                    changeArgs2.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_file_it_must_raise_event_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "Example")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_file_it_must_raise_event_for_size()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "Example")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Size;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationFile);
                    changeArgs.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_file_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "Example")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);

                    FileSystemEventArgs changeArgs1 = listener.ChangeEventArgsCollected.First();
                    changeArgs1.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs1.FullPath.Should().Be(pathToDestinationFile);
                    changeArgs1.Name.Should().Be(destinationFileName);

                    FileSystemEventArgs changeArgs2 = listener.ChangeEventArgsCollected.Skip(1).Single();
                    changeArgs2.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs2.FullPath.Should().Be(pathToDestinationFile);
                    changeArgs2.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_file_it_must_raise_event_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "Example")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationFile);
                    changeArgs.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_file_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "Example")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.FileName | NotifyFilters.Size |
                    NotifyFilters.LastWrite | NotifyFilters.LastAccess);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        // TODO: When_copying_empty_file...

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(destinationFileName);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationFile);
                    changeArgs.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_it_must_raise_event_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_it_must_raise_event_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationFile);
                    changeArgs.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.FileName | NotifyFilters.LastWrite);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        // TODO: When_copying_over_existing_file...

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "NEW-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_it_must_raise_events_for_size()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "NEW-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Size;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "NEW-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "NEW-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "NEW-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        // TODO: When_copying_over_existing_file_with_same_size...

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_with_same_size_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "--NEW-CONTENT!--")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_with_same_size_it_must_raise_events_for_size()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "--NEW-CONTENT!--")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Size;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_with_same_size_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "--NEW-CONTENT!--")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_with_same_size_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "--NEW-CONTENT!--")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_with_same_size_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "--NEW-CONTENT!--")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        // TODO: When_copying_over_existing_file_with_same_content...

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_with_same_content_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "FILE-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "FILE-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_with_same_content_it_must_raise_events_for_size()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "FILE-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "FILE-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Size;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_with_same_content_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "FILE-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "FILE-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_with_same_content_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "FILE-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "FILE-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_file_with_same_content_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "FILE-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "FILE-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        // TODO: When_copying_empty_file_over_existing_empty_file...

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_over_existing_empty_file_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .IncludingEmptyFile(pathToDestinationFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_over_existing_empty_file_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .IncludingEmptyFile(pathToDestinationFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_over_existing_empty_file_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .IncludingEmptyFile(pathToDestinationFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_over_existing_empty_file_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .IncludingEmptyFile(pathToDestinationFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.LastWrite | NotifyFilters.LastAccess);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        // TODO: When_copying_empty_file_over_existing_file...

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_over_existing_file_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .IncludingTextFile(pathToDestinationFile, "ORIGINAL")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_over_existing_file_it_must_raise_events_for_size()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .IncludingTextFile(pathToDestinationFile, "ORIGINAL")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Size;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_over_existing_file_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .IncludingTextFile(pathToDestinationFile, "ORIGINAL")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_over_existing_file_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .IncludingTextFile(pathToDestinationFile, "ORIGINAL")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_empty_file_over_existing_file_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .IncludingTextFile(pathToDestinationFile, "ORIGINAL")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        // TODO: When_copying_over_existing_empty_file...

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_empty_file_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingEmptyFile(pathToDestinationFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_empty_file_it_must_raise_events_for_size()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingEmptyFile(pathToDestinationFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Size;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_empty_file_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingEmptyFile(pathToDestinationFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_empty_file_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingEmptyFile(pathToDestinationFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs args in listener.ChangeEventArgsCollected)
                    {
                        args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        args.FullPath.Should().Be(pathToDestinationFile);
                        args.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_copying_over_existing_empty_file_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingEmptyFile(pathToDestinationFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        // TODO: Add tests for:
        // - Move file
        // - Change file times
        //
        // - Create directory
        // - Delete directory
        // - Change directory attributes
        // - Copy directory
        // - Move directory
        // - Change directory times
    }
}
#endif
