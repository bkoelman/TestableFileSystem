#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryMoveSpecs : WatcherSpecs
    {
        // TODO: Investigate impact on attributes
        // TODO: Investigate file rename & move

        [Theory]
        [WatcherNotifyTestData(@"
            * Container                                     @ LastAccess
            > Container\MoveSource => Container\MoveTarget  @ DirectoryName
            * Container                                     @ LastWrite LastAccess
        ")]
        private void When_renaming_empty_directory_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceDirectoryName = "MoveSource";
            const string destinationDirectoryName = "MoveTarget";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceDirectory = Path.Combine(pathToContainerDirectory, sourceDirectoryName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToSourceDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        // TODO: When_renaming_directory_tree (same events as above)

        // TODO: When_moving_directory_tree_to_parent_directory

        // TODO: When_moving_directory_tree_to_subdirectory
    }
}
#endif
