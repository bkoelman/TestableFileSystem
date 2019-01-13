#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyFileAttributesSpecs : WatcherSpecs
    {
        [Fact]
        private void When_getting_file_attributes_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            string filePath = Path.Combine(directoryToWatch, "file.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(filePath, FileAttributes.Hidden)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.GetAttributes(filePath);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container\file.txt                            @ Attributes
        ")]
        private void When_changing_file_attributes_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string fileNameToUpdate = "file.txt";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToFileToUpdate = Path.Combine(pathToContainerDirectory, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Fact]
        private void When_changing_file_attributes_to_existing_value_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";
            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate, FileAttributes.ReadOnly)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(0);
                }
            }
        }
    }
}
#endif
