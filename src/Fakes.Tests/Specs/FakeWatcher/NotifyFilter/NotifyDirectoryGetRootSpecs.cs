#if !NETCOREAPP1_1
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryGetRootSpecs : WatcherSpecs
    {
        [Fact]
        private void When_getting_directory_root_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            string directoryPath = Path.Combine(directoryToWatch, "Subfolder");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryPath)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetDirectoryRoot(directoryPath);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
