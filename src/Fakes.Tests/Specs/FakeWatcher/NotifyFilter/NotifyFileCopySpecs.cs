#if !NETCOREAPP1_1
using System.IO;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyFileCopySpecs : WatcherSpecs
    {
        [NotNull]
        private static readonly byte[] LargeFileBuffer = BufferFactory.Create(1024 * 4);

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
    }
}
#endif
