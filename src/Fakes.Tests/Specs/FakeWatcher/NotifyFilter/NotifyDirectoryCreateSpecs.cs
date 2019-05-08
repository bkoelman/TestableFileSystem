#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryCreateSpecs : WatcherSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_existing_directory_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = "Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToCreate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
        [WatcherNotifyTestData(@"
            + Subfolder                                     @ DirectoryName
        ")]
        private void When_creating_missing_directory_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = "Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_existing_directory_tree_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = @"Deeper\Nested\Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToCreate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
        [WatcherNotifyTestData(@"
            + DEEPER\Nested                                 @ DirectoryName
            + DEEPER\Nested\Subfolder                       @ DirectoryName
        ")]
        private void When_creating_partially_missing_directory_tree_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToCreate = @"Deeper\Nested\Subfolder";
            string pathToDirectoryToCreate = Path.Combine(directoryToWatch, directoryNameToCreate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(Path.Combine(directoryToWatch, "DEEPER"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.CreateDirectory(pathToDirectoryToCreate);

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
