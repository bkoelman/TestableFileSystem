#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryDeleteSpecs : WatcherSpecs
    {
        [Theory]
        [WatcherNotifyTestData(@"
            - Subfolder                                     @ DirectoryName
        ")]
        private void When_deleting_directory_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToDelete = "Subfolder";
            string pathToDirectoryToDelete = Path.Combine(directoryToWatch, directoryNameToDelete);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToDelete)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Subfolder                                     @ LastAccess
            - Subfolder                                     @ DirectoryName
        ")]
        private void When_deleting_directory_recursively_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToDelete = "Subfolder";
            string pathToDirectoryToDelete = Path.Combine(directoryToWatch, directoryNameToDelete);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToDelete)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            - Container\TopLevel\FileInRoot1.txt                            @ FileName
            - Container\TopLevel\FileInRoot2.txt                            @ FileName
            - Container\TopLevel\FolderA\FileInA.txt                        @ FileName
            * Container\TopLevel\FolderA\SubFolderA                         @                               LastAccess
            - Container\TopLevel\FolderA\SubFolderA                         @ DirectoryName
            * Container\TopLevel\FolderA                                    @                   LastWrite   LastAccess
            - Container\TopLevel\FolderA                                    @ DirectoryName
            - Container\TopLevel\FolderB\FileInB.txt                        @ FileName
            * Container\TopLevel\FolderB                                    @                   LastWrite   LastAccess
            - Container\TopLevel\FolderB                                    @ DirectoryName
            - Container\TopLevel\FolderC\SubFolderC\FileInSubFolderC.txt    @ FileName
            * Container\TopLevel\FolderC\SubFolderC                         @                   LastWrite   LastAccess
            - Container\TopLevel\FolderC\SubFolderC                         @ DirectoryName
            * Container\TopLevel\FolderC                                    @                   LastWrite   LastAccess
            - Container\TopLevel\FolderC                                    @ DirectoryName
            * Container\TopLevel                                            @                   LastWrite   LastAccess
            - Container\TopLevel                                            @ DirectoryName
            * Container                                                     @                   LastWrite   LastAccess
            - Container                                                     @ DirectoryName
        ")]
        private void When_deleting_directory_tree_recursively_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            string pathToDirectoryToDelete = Path.Combine(directoryToWatch, "Container");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(Path.Combine(pathToDirectoryToDelete, @"TopLevel\FolderA\SubFolderA"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToDelete, @"TopLevel\FolderA\FileInA.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToDelete, @"TopLevel\FolderB\FileInB.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToDelete, @"TopLevel\FolderC\SubFolderC\FileInSubFolderC.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToDelete, @"TopLevel\FileInRoot2.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToDelete, @"TopLevel\FileInRoot1.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }
    }
}
#endif
