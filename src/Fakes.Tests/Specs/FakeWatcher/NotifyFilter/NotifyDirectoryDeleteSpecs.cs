using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

#if !NETCOREAPP1_1
namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryDeleteSpecs : WatcherSpecs
    {
        [Fact]
        private void When_deleting_directory_it_must_raise_events_for_all_notify_filters()
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
                watcher.NotifyFilter = TestNotifyFilters.All;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.DeleteEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    args.FullPath.Should().Be(pathToDirectoryToDelete);
                    args.Name.Should().Be(directoryNameToDelete);
                }
            }
        }

        [Fact]
        private void When_deleting_directory_it_must_raise_events_for_directory_name()
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
                watcher.NotifyFilter = NotifyFilters.DirectoryName;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.DeleteEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
                    args.FullPath.Should().Be(pathToDirectoryToDelete);
                    args.Name.Should().Be(directoryNameToDelete);
                }
            }
        }

        [Fact]
        private void When_deleting_directory_it_must_not_raise_events_for_other_notify_filters()
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
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.DirectoryName);

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_deleting_directory_tree_recursively_it_must_raise_events_for_all_notify_filters()
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
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());

                    /*

                    Start monitoring changes...
                    * Container\TopLevel                                                            LastAccess  LastWrite
                    - Container\TopLevel\FileInRoot1.txt                            FileName
                    - Container\TopLevel\FileInRoot2.txt                            FileName
                    * Container\TopLevel\FolderA                                                    LastAccess  LastWrite
                    - Container\TopLevel\FolderA\FileInA.txt                        FileName
                    - Container\TopLevel\FolderA\SubFolderA                         DirectoryName
                    * Container\TopLevel\FolderA                                                                LastWrite (2)
                    - Container\TopLevel\FolderA                                    DirectoryName
                    * Container\TopLevel\FolderB                                                    LastAccess  LastWrite
                    - Container\TopLevel\FolderB\FileInB.txt                        FileName
                    * Container\TopLevel\FolderB                                                                LastWrite (2)
                    - Container\TopLevel\FolderB                                    DirectoryName
                    * Container\TopLevel\FolderC\SubFolderC                                         LastAccess  LastWrite
                    - Container\TopLevel\FolderC\SubFolderC\FileInSubFolderC.txt    FileName
                    * Container\TopLevel\FolderC\SubFolderC                                                     LastWrite (2)
                    - Container\TopLevel\FolderC\SubFolderC                         DirectoryName
                    * Container\TopLevel\FolderC                                                                LastWrite
                    - Container\TopLevel\FolderC                                    DirectoryName
                    * Container\TopLevel                                                                        LastWrite
                    - Container\TopLevel                                            DirectoryName
                    * Container                                                                                 LastWrite
                    - Container                                                     DirectoryName

                    Observations:
                    - applies to FileName for files, DirectoryName for directories
                    * applies to directories only; LastWrite (always) and LastAccess (only first occurence)

                    */

                    throw new NotImplementedException();
                }
            }
        }

        [Fact]
        private void When_deleting_directory_tree_recursively_it_must_raise_events_for_directory_name()
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
                watcher.NotifyFilter = NotifyFilters.DirectoryName;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());

                    text.Should().Be(@"
                        - Container\TopLevel\FolderA\SubFolderA
                        - Container\TopLevel\FolderA
                        - Container\TopLevel\FolderB
                        - Container\TopLevel\FolderC\SubFolderC
                        - Container\TopLevel\FolderC
                        - Container\TopLevel
                        - Container
                        ".TrimLines());
                }
            }
        }

        [Fact]
        private void When_deleting_directory_tree_recursively_it_must_raise_events_for_file_name()
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
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());

                    text.Should().Be(@"
                        - Container\TopLevel\FileInRoot1.txt
                        - Container\TopLevel\FileInRoot2.txt
                        - Container\TopLevel\FolderA\FileInA.txt
                        - Container\TopLevel\FolderB\FileInB.txt
                        - Container\TopLevel\FolderC\SubFolderC\FileInSubFolderC.txt
                        ".TrimLines());
                }
            }
        }

        // TODO: When_deleting_directory_tree_recursively_it_must_raise_events_for_last_write

        // TODO: When_deleting_directory_tree_recursively_it_must_raise_events_for_last_access

        [Fact]
        private void When_deleting_directory_tree_recursively_it_must_not_raise_events_for_other_notify_filters()
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
                watcher.NotifyFilter = TestNotifyFilters.All.Except(NotifyFilters.DirectoryName | NotifyFilters.FileName |
                    NotifyFilters.LastWrite | NotifyFilters.LastAccess);
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Delete(pathToDirectoryToDelete, true);

                    watcher.WaitForEventDispatcherIdle(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());

                    text.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
