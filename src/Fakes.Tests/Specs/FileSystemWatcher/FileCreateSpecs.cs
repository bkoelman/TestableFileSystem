#if !NETCOREAPP1_1
using System.IO;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FileSystemWatcher
{
    public sealed class FileCreateSpecs
    {
        private const NotifyFilters NotifyFilterAll = NotifyFilters.FileName | NotifyFilters.DirectoryName |
            NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastAccess | NotifyFilters.LastWrite |
            NotifyFilters.CreationTime;

        [CanBeNull]
        private const int NotifyWaitTimeoutMilliseconds = 3000;

        [Fact]
        private void When_creating_file_it_must_raise_event()
        {
            // Arrange
            const string directory = @"c:\some";
            const string path = @"c:\some\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directory))
            {
                watcher.NotifyFilter = NotifyFilterAll;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    watcher.EnableRaisingEvents = true;

                    // Act
                    using (fileSystem.File.Create(path))
                    {
                    }

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

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
        private void When_creating_file_that_already_exists_it_must_not_raise_event()
        {
            // Arrange
            const string directory = @"c:\some";
            const string path = @"c:\some\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directory))
            {
                watcher.NotifyFilter = NotifyFilterAll;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    watcher.EnableRaisingEvents = true;

                    // Act
                    using (fileSystem.File.Create(path))
                    {
                    }

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
