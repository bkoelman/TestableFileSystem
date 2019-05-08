#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyFileTimeCreationUtcSpecs : WatcherSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2003).At(12, 34, 56).AsUtc();

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_file_creation_time_in_UTC_it_must_not_raise_events()
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
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.GetCreationTimeUtc(filePath);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
        [WatcherNotifyTestData(@"
            * file.txt                                      @ CreationTime
        ")]
        private void When_changing_file_creation_time_in_UTC_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileName = "file.txt";
            string filePath = Path.Combine(directoryToWatch, fileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(filePath, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetCreationTimeUtc(filePath, DefaultTimeUtc);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
        [WatcherNotifyTestData(@"
            * file.txt                                      @ CreationTime
        ")]
        private void When_changing_file_creation_time_in_UTC_to_existing_value_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileName = "file.txt";
            string filePath = Path.Combine(directoryToWatch, fileName);

            var clock = new SystemClock(() => DefaultTimeUtc);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(filePath, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetCreationTimeUtc(filePath, DefaultTimeUtc);

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
