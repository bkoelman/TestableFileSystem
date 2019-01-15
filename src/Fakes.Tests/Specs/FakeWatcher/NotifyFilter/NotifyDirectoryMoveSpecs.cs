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
        // TODO: Investigate file rename & move

        [Theory]
        [WatcherNotifyTestData(@"
            * Container                                     @                               LastAccess
            > Container\MoveSource => Container\MoveTarget  @ DirectoryName
            * Container                                     @                   LastWrite   LastAccess
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container                                     @                               LastAccess
            > Container\MoveSource => Container\MoveTarget  @ DirectoryName
            * Container                                     @                   LastWrite   LastAccess
        ")]
        private void When_renaming_directory_tree_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
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
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderA\SubFolderA"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderA\FileInA.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderB\FileInB.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderC\SubFolderC\FileInSubFolderC.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot2.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot1.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container                                     @                               LastAccess
            > Container\MoveSource => Container\MoveTarget  @ DirectoryName
            * Container                                     @                   LastWrite   LastAccess
        ")]
        private void When_renaming_readonly_directory_tree_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
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
                .IncludingDirectory(pathToContainerDirectory, FileAttributes.ReadOnly)
                .IncludingDirectory(pathToSourceDirectory, FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderA"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderA\SubFolderA"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderA\FileInA.txt"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderB"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderB\FileInB.txt"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderC"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderC\SubFolderC"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderC\SubFolderC\FileInSubFolderC.txt"),
                    FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot2.txt"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot1.txt"), FileAttributes.ReadOnly)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container                                     @                               LastAccess
            - Container\MoveSource\SubFolderA               @ DirectoryName
            + Container\MovedSubFolderA                     @ DirectoryName
            * Container                                     @                   LastWrite   LastAccess
        ")]
        private void When_moving_empty_directory_to_parent_directory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceDirectoryName = @"MoveSource\SubFolderA";
            const string destinationDirectoryName = "MovedSubFolderA";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceDirectory = Path.Combine(pathToContainerDirectory, sourceDirectoryName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderA\SubFolderA"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container                                     @                               LastAccess
            - Container\MoveSource\SubFolderA               @ DirectoryName
            + Container\MovedSubFolderA                     @ DirectoryName
            * Container                                     @                   LastWrite   LastAccess
        ")]
        private void When_moving_directory_tree_to_parent_directory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceDirectoryName = @"MoveSource\SubFolderA";
            const string destinationDirectoryName = "MovedSubFolderA";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceDirectory = Path.Combine(pathToContainerDirectory, sourceDirectoryName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderA\SubFolderA"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderA\FileInA.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderB\FileInB.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderC\SubFolderC\FileInSubFolderC.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot2.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot1.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container                                     @                               LastAccess
            - Container\MoveSource\SubFolderA               @ DirectoryName
            + Container\MovedSubFolderA                     @ DirectoryName
            * Container                                     @                   LastWrite   LastAccess
        ")]
        private void When_moving_readonly_directory_tree_to_parent_directory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceDirectoryName = @"MoveSource\SubFolderA";
            const string destinationDirectoryName = "MovedSubFolderA";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceDirectory = Path.Combine(pathToContainerDirectory, sourceDirectoryName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToContainerDirectory, FileAttributes.ReadOnly)
                .IncludingDirectory(pathToSourceDirectory, FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderA"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderA\SubFolderA"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderA\FileInA.txt"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderB"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderB\FileInB.txt"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderC"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderC\SubFolderC"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderC\SubFolderC\FileInSubFolderC.txt"),
                    FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot2.txt"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot1.txt"), FileAttributes.ReadOnly)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container\MoveTarget                          @                               LastAccess
            - Container\MoveSource                          @ DirectoryName
            + Container\MoveTarget\NewMoveSource            @ DirectoryName
            * Container\MoveTarget                          @                   LastWrite   LastAccess
        ")]
        private void When_moving_empty_directory_to_subdirectory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceDirectoryName = "MoveSource";
            const string destinationDirectoryName = @"MoveTarget\NewMoveSource";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceDirectory = Path.Combine(pathToContainerDirectory, sourceDirectoryName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToSourceDirectory)
                .IncludingDirectory(Path.Combine(pathToContainerDirectory, "MoveTarget"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container\MoveTarget                          @                               LastAccess
            - Container\MoveSource                          @ DirectoryName
            + Container\MoveTarget\NewMoveSource            @ DirectoryName
            * Container\MoveTarget                          @                   LastWrite   LastAccess
        ")]
        private void When_moving_directory_tree_to_subdirectory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceDirectoryName = "MoveSource";
            const string destinationDirectoryName = @"MoveTarget\NewMoveSource";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceDirectory = Path.Combine(pathToContainerDirectory, sourceDirectoryName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderA\SubFolderA"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderA\FileInA.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderB\FileInB.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderC\SubFolderC\FileInSubFolderC.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot2.txt"))
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot1.txt"))
                .IncludingDirectory(Path.Combine(pathToContainerDirectory, "MoveTarget"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container\MoveTarget                          @                               LastAccess
            - Container\MoveSource                          @ DirectoryName
            + Container\MoveTarget\NewMoveSource            @ DirectoryName
            * Container\MoveTarget                          @                   LastWrite   LastAccess
        ")]
        private void When_moving_readonly_directory_tree_to_subdirectory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceDirectoryName = "MoveSource";
            const string destinationDirectoryName = @"MoveTarget\NewMoveSource";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceDirectory = Path.Combine(pathToContainerDirectory, sourceDirectoryName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToContainerDirectory, FileAttributes.ReadOnly)
                .IncludingDirectory(pathToSourceDirectory, FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderA"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderA\SubFolderA"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderA\FileInA.txt"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderB"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderB\FileInB.txt"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderC"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToSourceDirectory, @"FolderC\SubFolderC"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FolderC\SubFolderC\FileInSubFolderC.txt"),
                    FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot2.txt"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(pathToSourceDirectory, @"FileInRoot1.txt"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(pathToContainerDirectory, "MoveTarget"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceDirectory, pathToDestinationDirectory);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }
    }
}
#endif
