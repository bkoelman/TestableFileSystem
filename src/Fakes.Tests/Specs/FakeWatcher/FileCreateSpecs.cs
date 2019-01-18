#if !NETCOREAPP1_1
using System.IO;
using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class FileCreateSpecs : WatcherSpecs
    {
        // TODO: Move these specs elsewhere.

        [Fact]
        private void When_overwriting_existing_local_file_with_different_casing_it_must_raise_event()
        {
            // Arrange
            const string directory = @"c:\some";
            const string path = @"c:\some\FILE.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directory))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(@"c:\SOME\file.TXT"))
                    {
                    }

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(path);
                    args.Name.Should().Be("FILE.txt");
                }
            }
        }

        [Fact]
        private void When_creating_local_file_with_trailing_whitespace_it_must_raise_event()
        {
            // Arrange
            const string directory = @"c:\some";
            const string path = @"c:\some\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directory))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(path + "  "))
                    {
                    }

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(path);
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact]
        private void When_creating_file_using_absolute_path_without_drive_letter_it_must_raise_event()
        {
            // Arrange
            const string directory = @"c:\some";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directory)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(directory);

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(@"c:\"))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(@"\file.txt"))
                    {
                    }

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(@"c:\file.txt");
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact]
        private void When_creating_relative_local_file_it_must_raise_event()
        {
            // Arrange
            const string directory = @"c:\some";
            const string path = @"c:\some\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directory)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(directory);

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directory))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create("file.txt"))
                    {
                    }

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(path);
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact]
        private void When_creating_remote_file_on_existing_network_share_it_must_raise_event()
        {
            // Arrange
            const string directory = @"\\server\share";
            const string path = @"\\server\share\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directory))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(path))
                    {
                    }

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(path);
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact]
        private void When_overwriting_existing_remote_file_it_must_raise_event()
        {
            // Arrange
            const string directory = @"\\server\share";
            const string path = @"\\server\share\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directory))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(path))
                    {
                    }

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(path);
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact]
        private void When_creating_extended_local_file_it_must_raise_event()
        {
            // Arrange
            const string directory = @"\\?\C:\folder";
            const string path = @"\\?\C:\folder\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(@"C:\folder"))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(path))
                    {
                    }

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(@"C:\folder\file.txt");
                    args.Name.Should().Be("file.txt");
                }
            }
        }
    }
}
#endif
