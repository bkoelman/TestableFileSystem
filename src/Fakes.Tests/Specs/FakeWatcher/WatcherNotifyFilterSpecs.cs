#if !NETCOREAPP1_1
using System.IO;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class WatcherNotifyFilterSpecs
    {
        private const int NotifyWaitTimeoutMilliseconds = 500;

        [NotNull]
        private static readonly byte[] LargeFileBuffer = BufferFactory.Create(1024 * 4);

        [Fact]
        private void When_creating_file_it_must_raise_events_for_all_notify_filters()
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
                watcher.NotifyFilter = TestNotifyFilters.All;

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
        private void When_creating_file_it_must_raise_events_for_file_name()
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
        private void When_creating_file_it_must_not_raise_events_for_other_notify_filters()
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
        private void When_deleting_file_it_must_raise_events_for_all_notify_filters()
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
                watcher.NotifyFilter = TestNotifyFilters.All;

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
        private void When_deleting_file_it_must_raise_events_for_file_name()
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
        private void When_deleting_file_it_must_not_raise_events_for_other_notify_filters()
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
        private void When_appending_to_file_it_must_raise_events_for_all_notify_filters()
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
        private void When_appending_to_file_it_must_raise_events_for_last_write()
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
        private void When_appending_to_file_it_must_raise_events_for_last_access()
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
        private void When_appending_to_file_it_must_raise_events_for_size()
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
        private void When_appending_to_file_it_must_not_raise_events_for_other_notify_filters()
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
        private void When_overwriting_file_with_same_contents_it_must_raise_events_for_all_notify_filters()
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
        private void When_overwriting_file_with_same_contents_it_must_raise_events_for_last_write()
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
        private void When_overwriting_file_with_same_contents_it_must_raise_events_for_last_access()
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
        private void When_overwriting_file_with_same_contents_it_must_not_raise_events_for_other_notify_filters()
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
        private void When_truncating_existing_file_it_must_raise_events_for_all_notify_filters()
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
        private void When_truncating_existing_file_it_must_raise_events_for_last_write()
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
        private void When_truncating_existing_file_it_must_raise_events_for_last_access()
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
        private void When_truncating_existing_file_it_must_raise_events_for_size()
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
        private void When_truncating_existing_file_it_must_not_raise_events_for_other_notify_filters()
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
        private void When_recreating_existing_empty_file_it_must_raise_events_for_all_notify_filters()
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
        private void When_recreating_existing_empty_file_it_must_raise_events_for_last_write()
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
        private void When_recreating_existing_empty_file_it_must_raise_events_for_last_access()
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
        private void When_recreating_existing_empty_file_it_must_not_raise_events_for_other_notify_filters()
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
        private void When_changing_file_attributes_it_must_raise_events_for_all_notify_filters()
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
        private void When_changing_file_attributes_it_must_raise_events_for_attributes()
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
        private void When_changing_file_attributes_it_must_not_raise_events_for_other_notify_filters()
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
        private void When_changing_file_attributes_to_existing_value_it_must_not_raise_events_for_all_notify_filters()
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

        [Fact]
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

                    foreach (FileSystemEventArgs changeArgs in listener.ChangeEventArgsCollected)
                    {
                        changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        changeArgs.FullPath.Should().Be(pathToDestinationFile);
                        changeArgs.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact]
        private void When_copying_file_it_must_raise_events_for_file_name()
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

        [Fact]
        private void When_copying_file_it_must_raise_events_for_size()
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

        [Fact]
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
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs changeArgs in listener.ChangeEventArgsCollected)
                    {
                        changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        changeArgs.FullPath.Should().Be(pathToDestinationFile);
                        changeArgs.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact]
        private void When_copying_file_it_must_raise_events_for_last_access()
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

        [Fact]
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

        [Fact]
        private void When_copying_large_file_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(pathToSourceFile, LargeFileBuffer)
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
                    listener.EventsCollected.Should().HaveCount(4);
                    listener.CreateEventArgsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveCount(3);

                    foreach (FileSystemEventArgs changeArgs in listener.ChangeEventArgsCollected)
                    {
                        changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        changeArgs.FullPath.Should().Be(pathToDestinationFile);
                        changeArgs.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact]
        private void When_copying_large_file_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(pathToSourceFile, LargeFileBuffer)
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

        [Fact]
        private void When_copying_large_file_it_must_raise_events_for_size()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(pathToSourceFile, LargeFileBuffer)
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
                    listener.EventsCollected.Should().HaveCount(2);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs changeArgs in listener.ChangeEventArgsCollected)
                    {
                        changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        changeArgs.FullPath.Should().Be(pathToDestinationFile);
                        changeArgs.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact]
        private void When_copying_large_file_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(pathToSourceFile, LargeFileBuffer)
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
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    foreach (FileSystemEventArgs changeArgs in listener.ChangeEventArgsCollected)
                    {
                        changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                        changeArgs.FullPath.Should().Be(pathToDestinationFile);
                        changeArgs.Name.Should().Be(destinationFileName);
                    }
                }
            }
        }

        [Fact]
        private void When_copying_large_file_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(pathToSourceFile, LargeFileBuffer)
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

        [Fact]
        private void When_copying_large_file_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(pathToSourceFile, LargeFileBuffer)
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

        [Fact]
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

        [Fact]
        private void When_copying_empty_file_it_must_raise_events_for_file_name()
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

        [Fact]
        private void When_copying_empty_file_it_must_raise_events_for_last_write()
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
        //    x When_renaming_file_to_itself_it_must_succeed                                        => [none]
        //    x When_renaming_file_to_itself_with_different_casing_it_must_rename                   => *FileName@sourceFile,targetFile   } same
        //    x When_renaming_file_it_must_rename [BASIC]                                           => *FileName@sourceFile,targetFile   }
        //    x When_moving_file_to_parent_directory_it_must_succeed [BASIC]                        => -FileName@sourceFile +FileName@targetFile
        //    x When_moving_file_to_subdirectory_it_must_succeed [BASIC]                            => -FileName@sourceFile +FileName@targetFile *LastWrite&LastAccess@targetDir (also when deeper than 1)   } same
        //    x When_moving_file_to_sibling_directory_it_must_succeed [BASIC]                       => -FileName@sourceFile +FileName@targetFile *LastWrite&LastAccess@targetDir (also when deeper than 1)   }
        //      When_moving_file_in_from_different_drive_it_must_succeed [BASIC]                    => +FileName ** (same as copy into)
        //      When_moving_file_out_to_different_drive_it_must_succeed [BASIC]                     => -FileName@sourceFile
        //    x When_renaming/moving_system_file_it_must_succeed                                    => *FileName@sourceFile,targetFile *Attributes@targetFile  (always, except when mask contains Archive flag)

        [Fact]
        private void When_renaming_file_to_itself_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileName = "file.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, fileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch.ToUpperInvariant(), fileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_renaming_file_to_itself_with_different_casing_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "file.TXT";
            const string destinationFileName = "FILE.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    RenamedEventArgs args = listener.RenameEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                    args.FullPath.Should().Be(pathToDestinationFile);
                    args.Name.Should().Be(destinationFileName);
                    args.OldFullPath.Should().Be(pathToSourceFile);
                    args.OldName.Should().Be(sourceFileName);
                }
            }
        }

        [Fact]
        private void When_renaming_file_to_itself_with_different_casing_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "file.TXT";
            const string destinationFileName = "FILE.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    RenamedEventArgs args = listener.RenameEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                    args.FullPath.Should().Be(pathToDestinationFile);
                    args.Name.Should().Be(destinationFileName);
                    args.OldFullPath.Should().Be(pathToSourceFile);
                    args.OldName.Should().Be(sourceFileName);
                }
            }
        }

        [Fact]
        private void When_renaming_file_to_itself_with_different_casing_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "file.TXT";
            const string destinationFileName = "FILE.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.FileName);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_renaming_file_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    RenamedEventArgs args = listener.RenameEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                    args.FullPath.Should().Be(pathToDestinationFile);
                    args.Name.Should().Be(destinationFileName);
                    args.OldFullPath.Should().Be(pathToSourceFile);
                    args.OldName.Should().Be(sourceFileName);
                }
            }
        }

        [Fact]
        private void When_renaming_file_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    RenamedEventArgs args = listener.RenameEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                    args.FullPath.Should().Be(pathToDestinationFile);
                    args.Name.Should().Be(destinationFileName);
                    args.OldFullPath.Should().Be(pathToSourceFile);
                    args.OldName.Should().Be(sourceFileName);
                }
            }
        }

        [Fact]
        private void When_renaming_file_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.FileName);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        #region Attributes in rename

        [Fact]
        private void When_renaming_archive_file_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.Archive)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    RenamedEventArgs args = listener.RenameEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                    args.FullPath.Should().Be(pathToDestinationFile);
                    args.Name.Should().Be(destinationFileName);
                    args.OldFullPath.Should().Be(pathToSourceFile);
                    args.OldName.Should().Be(sourceFileName);
                }
            }
        }

        [Fact]
        private void When_renaming_archive_file_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.Archive)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    RenamedEventArgs args = listener.RenameEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                    args.FullPath.Should().Be(pathToDestinationFile);
                    args.Name.Should().Be(destinationFileName);
                    args.OldFullPath.Should().Be(pathToSourceFile);
                    args.OldName.Should().Be(sourceFileName);
                }
            }
        }

        [Fact]
        private void When_renaming_archive_file_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.Archive)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.FileName);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_renaming_system_file_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);

                    RenamedEventArgs renameArgs = listener.RenameEventArgsCollected.Single();
                    renameArgs.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                    renameArgs.FullPath.Should().Be(pathToDestinationFile);
                    renameArgs.Name.Should().Be(destinationFileName);
                    renameArgs.OldFullPath.Should().Be(pathToSourceFile);
                    renameArgs.OldName.Should().Be(sourceFileName);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationFile);
                    changeArgs.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact]
        private void When_renaming_system_file_it_must_raise_events_for_attributes()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Attributes;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDestinationFile);
                    args.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact]
        private void When_renaming_system_file_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    RenamedEventArgs args = listener.RenameEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                    args.FullPath.Should().Be(pathToDestinationFile);
                    args.Name.Should().Be(destinationFileName);
                    args.OldFullPath.Should().Be(pathToSourceFile);
                    args.OldName.Should().Be(sourceFileName);
                }
            }
        }

        [Fact]
        private void When_renaming_system_file_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.FileName | NotifyFilters.Attributes);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_renaming_readonly_hidden_system_archive_file_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT",
                    attributes: FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    RenamedEventArgs args = listener.RenameEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                    args.FullPath.Should().Be(pathToDestinationFile);
                    args.Name.Should().Be(destinationFileName);
                    args.OldFullPath.Should().Be(pathToSourceFile);
                    args.OldName.Should().Be(sourceFileName);
                }
            }
        }

        [Fact]
        private void When_renaming_readonly_hidden_system_archive_file_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT",
                    attributes: FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    RenamedEventArgs args = listener.RenameEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
                    args.FullPath.Should().Be(pathToDestinationFile);
                    args.Name.Should().Be(destinationFileName);
                    args.OldFullPath.Should().Be(pathToSourceFile);
                    args.OldName.Should().Be(sourceFileName);
                }
            }
        }

        [Fact]
        private void When_renaming_readonly_hidden_system_archive_file_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT",
                    attributes: FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.FileName);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        #endregion

        [Fact]
        private void When_moving_file_to_parent_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, "nested", sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(@"nested\source.txt");

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact]
        private void When_moving_file_to_parent_directory_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, "nested", sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(@"nested\source.txt");

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(destinationFileName);
                }
            }
        }

        [Fact]
        private void When_moving_file_to_parent_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, "nested", sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.FileName);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_moving_file_to_subdirectory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "nested");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(sourceFileName);

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(@"nested\target.txt");

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("nested");
                }
            }
        }

        [Fact]
        private void When_moving_file_to_subdirectory_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "nested");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(sourceFileName);

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(@"nested\target.txt");
                }
            }
        }

        [Fact]
        private void When_moving_file_to_subdirectory_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "nested");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("nested");
                }
            }
        }

        [Fact]
        private void When_moving_file_to_subdirectory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "nested");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("nested");
                }
            }
        }

        [Fact]
        private void When_moving_file_to_subdirectory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "nested");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_moving_file_to_sibling_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(@"srcFolder\source.txt");

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(@"dstFolder\target.txt");

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("dstFolder");
                }
            }
        }

        [Fact]
        private void When_moving_file_to_sibling_directory_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(@"srcFolder\source.txt");

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(@"dstFolder\target.txt");
                }
            }
        }

        [Fact]
        private void When_moving_file_to_sibling_directory_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("dstFolder");
                }
            }
        }

        [Fact]
        private void When_moving_file_to_sibling_directory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("dstFolder");
                }
            }
        }

        [Fact]
        private void When_moving_file_to_sibling_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        #region Attributes in move

        [Fact]
        private void When_moving_archive_file_to_sibling_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(@"srcFolder\source.txt");

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(@"dstFolder\target.txt");

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("dstFolder");
                }
            }
        }

        [Fact]
        private void When_moving_archive_file_to_sibling_directory_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(@"srcFolder\source.txt");

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(@"dstFolder\target.txt");
                }
            }
        }

        [Fact]
        private void When_moving_archive_file_to_sibling_directory_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("dstFolder");
                }
            }
        }

        [Fact]
        private void When_moving_archive_file_to_sibling_directory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("dstFolder");
                }
            }
        }

        [Fact]
        private void When_moving_archive_file_to_sibling_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void When_moving_system_file_to_sibling_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(4);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(@"srcFolder\source.txt");

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(@"dstFolder\target.txt");

                    FileSystemEventArgs[] changeArgsArray = listener.ChangeEventArgsCollected.OrderBy(x => x.Name).ToArray();
                    changeArgsArray.Should().HaveCount(2);

                    FileSystemEventArgs changeArgs1 = changeArgsArray[0];
                    changeArgs1.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs1.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs1.Name.Should().Be("dstFolder");

                    FileSystemEventArgs changeArgs2 = changeArgsArray[1];
                    changeArgs2.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs2.FullPath.Should().Be(pathToDestinationFile);
                    changeArgs2.Name.Should().Be(@"dstFolder\target.txt");
                }
            }
        }

        [Fact]
        private void When_moving_system_file_to_sibling_directory_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(@"srcFolder\source.txt");

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(@"dstFolder\target.txt");
                }
            }
        }

        [Fact]
        private void When_moving_system_file_to_sibling_directory_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("dstFolder");
                }
            }
        }

        [Fact]
        private void When_moving_system_file_to_sibling_directory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("dstFolder");
                }
            }
        }

        [Fact]
        private void When_moving_system_file_to_sibling_directory_it_must_raise_events_for_attributes()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.Attributes;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToDestinationFile);
                    args.Name.Should().Be(@"dstFolder\target.txt");
                }
            }
        }

        [Fact]
        private void When_moving_system_file_to_sibling_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.FileName | NotifyFilters.LastWrite |
                    NotifyFilters.LastAccess | NotifyFilters.Attributes);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact]
        private void
            When_moving_readonly_hidden_system_archive_file_to_sibling_directory_it_must_raise_events_for_all_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT",
                    attributes: FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(3);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(@"srcFolder\source.txt");

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(@"dstFolder\target.txt");

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("dstFolder");
                }
            }
        }

        [Fact]
        private void When_moving_readonly_hidden_system_archive_file_to_sibling_directory_it_must_raise_events_for_file_name()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT",
                    attributes: FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(2);

                    FileSystemEventArgs deleteArgs = listener.DeleteEventArgsCollected.Single();
                    deleteArgs.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    deleteArgs.FullPath.Should().Be(pathToSourceFile);
                    deleteArgs.Name.Should().Be(@"srcFolder\source.txt");

                    FileSystemEventArgs createArgs = listener.CreateEventArgsCollected.Single();
                    createArgs.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    createArgs.FullPath.Should().Be(pathToDestinationFile);
                    createArgs.Name.Should().Be(@"dstFolder\target.txt");
                }
            }
        }

        [Fact]
        private void When_moving_readonly_hidden_system_archive_file_to_sibling_directory_it_must_raise_events_for_last_write()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT",
                    attributes: FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("dstFolder");
                }
            }
        }

        [Fact]
        private void When_moving_readonly_hidden_system_archive_file_to_sibling_directory_it_must_raise_events_for_last_access()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT",
                    attributes: FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs changeArgs = listener.ChangeEventArgsCollected.Single();
                    changeArgs.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    changeArgs.FullPath.Should().Be(pathToDestinationDirectory);
                    changeArgs.Name.Should().Be("dstFolder");
                }
            }
        }

        [Fact]
        private void
            When_moving_readonly_hidden_system_archive_file_to_sibling_directory_it_must_not_raise_events_for_other_notify_filters()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(Path.Combine(directoryToWatch, "srcFolder"), sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT",
                    attributes: FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter =
                    TestNotifyFilters.All.Except(NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        #endregion

        // TODO: Add tests for:
        // - Change file times
        //
        // - Create directory
        // - Delete directory
        // - Change directory attributes
        // - Copy directory
        // - Move directory (including file)
        // - Change directory times
    }
}
#endif
