#if !NETCOREAPP1_1
using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
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

                        watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

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

                        watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                        // Assert
                        listener.EventsCollected.Should().BeEmpty();
                    }
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container\file.txt                            @ LastAccess
        ")]
        private void When_async_reading_from_stream_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string fileNameToRead = "file.txt";

            string pathToFileToRead = Path.Combine(directoryToWatch, containerDirectoryName, fileNameToRead);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(pathToFileToRead, BufferFactory.Create(1024))
                .Build();

            var buffer = new byte[512];

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    using (IFileStream stream = fileSystem.File.Open(pathToFileToRead, FileMode.Open, FileAccess.ReadWrite))
                    {
                        // Act
                        Task<int> task = stream.ReadAsync(buffer, 0, buffer.Length);
                        task.Wait();
                    }

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container\file.txt                            @ LastAccess    LastWrite
        ")]
        private void When_async_writing_to_stream_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string fileNameToWrite = "file.txt";

            string pathToFileToWrite = Path.Combine(directoryToWatch, containerDirectoryName, fileNameToWrite);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(pathToFileToWrite, BufferFactory.Create(1024))
                .Build();

            var buffer = new byte[512];

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    using (IFileStream stream = fileSystem.File.Open(pathToFileToWrite, FileMode.Open, FileAccess.ReadWrite))
                    {
                        // Act
                        Task task = stream.WriteAsync(buffer, 0, buffer.Length);
                        task.Wait();
                    }

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container\file.txt                            @ LastAccess    LastWrite   Size
        ")]
        private void When_async_appending_to_stream_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string fileNameToWrite = "file.txt";

            string pathToFileToWrite = Path.Combine(directoryToWatch, containerDirectoryName, fileNameToWrite);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(pathToFileToWrite, BufferFactory.Create(1024))
                .Build();

            var buffer = new byte[512];

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    using (IFileStream stream = fileSystem.File.Open(pathToFileToWrite, FileMode.Open, FileAccess.ReadWrite))
                    {
                        stream.Seek(0, SeekOrigin.End);

                        // Act
                        Task task = stream.WriteAsync(buffer, 0, buffer.Length);
                        task.Wait();
                    }

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }
    }
}
#endif
