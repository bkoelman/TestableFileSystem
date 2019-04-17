#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryTimeCreationUtcSpecs : WatcherSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2003).At(12, 34, 56).AsUtc();

        [Fact]
        private void When_getting_directory_creation_time_in_UTC_it_must_not_raise_events()
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
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetCreationTimeUtc(directoryPath);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Subfolder                                     @ CreationTime
        ")]
        private void When_changing_directory_creation_time_in_UTC_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryName = "Subfolder";
            string directoryPath = Path.Combine(directoryToWatch, directoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryPath)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.SetCreationTimeUtc(directoryPath, DefaultTimeUtc);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Subfolder                                     @ CreationTime
        ")]
        private void When_changing_directory_creation_time_in_UTC_to_existing_value_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryName = "Subfolder";
            string directoryPath = Path.Combine(directoryToWatch, directoryName);

            var clock = new SystemClock(() => DefaultTimeUtc);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(directoryPath)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.SetCreationTimeUtc(directoryPath, DefaultTimeUtc);

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
