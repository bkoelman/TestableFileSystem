#if !NETCOREAPP1_1
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyFileExistsSpecs : WatcherSpecs
    {
        [Fact]
        private void When_getting_file_existence_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            string filePath = Path.Combine(directoryToWatch, "file.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(filePath, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Exists(filePath);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
