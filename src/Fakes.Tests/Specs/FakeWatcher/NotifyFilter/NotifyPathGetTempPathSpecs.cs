#if !NETCOREAPP1_1
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyPathGetTempPathSpecs : WatcherSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_temp_path_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\User";
            const string tempDirectory = @"c:\User\Temp";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithTempDirectory(tempDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Path.GetTempPath();

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
