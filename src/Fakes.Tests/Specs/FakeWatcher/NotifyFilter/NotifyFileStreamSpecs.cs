#if !NETCOREAPP1_1
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyFileStreamSpecs : WatcherSpecs
    {
        [Fact]
        private void When_locking_bytes_in_stream_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            string filePath = Path.Combine(directoryToWatch, "file.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(filePath, BufferFactory.Create(4096))
                .Build();

            using (IFileStream stream = fileSystem.File.Open(filePath, FileMode.Open))
            {
                using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
                {
                    watcher.NotifyFilter = TestNotifyFilters.All;
                    watcher.IncludeSubdirectories = true;

                    using (var listener = new FileSystemWatcherEventListener(watcher))
                    {
                        // Act
                        stream.Lock(0, 256);

                        watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                        // Assert
                        listener.EventsCollected.Should().BeEmpty();
                    }
                }
            }
        }

        [Fact]
        private void When_unlocking_bytes_in_stream_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            string filePath = Path.Combine(directoryToWatch, "file.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(filePath, BufferFactory.Create(4096))
                .Build();

            using (IFileStream stream = fileSystem.File.Open(filePath, FileMode.Open))
            {
                stream.Lock(0, 256);

                using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
                {
                    watcher.NotifyFilter = TestNotifyFilters.All;
                    watcher.IncludeSubdirectories = true;

                    using (var listener = new FileSystemWatcherEventListener(watcher))
                    {
                        // Act
                        stream.Unlock(0, 256);

                        watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                        // Assert
                        listener.EventsCollected.Should().BeEmpty();
                    }
                }
            }
        }
    }
}
#endif
