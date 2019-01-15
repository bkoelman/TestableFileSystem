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
        #region Directory move

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

        #endregion

        #region File move

        [Theory]
        [WatcherNotifyTestData(@"
            > source.txt => target.txt                      @ FileName
        ")]
        private void When_renaming_file_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        #region Attributes in file rename

        [Theory]
        [WatcherNotifyTestData(@"
            > source.txt => target.txt                      @ FileName
        ")]
        private void When_renaming_archive_file_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.Archive)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            > source.txt => target.txt                      @ FileName
            * target.txt                                    @ Attributes
        ")]
        private void When_renaming_system_file_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            > source.txt => target.txt                      @ FileName
        ")]
        private void When_renaming_readonly_hidden_system_archive_file_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT",
                    attributes: FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        #endregion

        [Theory]
        [WatcherNotifyTestData(@"
            - nested\source.txt                             @ FileName
            + target.txt                                    @ FileName
        ")]
        private void When_moving_file_to_parent_directory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, "nested", sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            - source.txt                                    @ FileName
            + nested\target.txt                             @ FileName
            * nested                                        @ LastWrite     LastAccess
        ")]
        private void When_moving_file_to_subdirectory_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "nested");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            - srcFolder\source.txt                          @ FileName
            + dstFolder\target.txt                          @ FileName
            * dstFolder                                     @ LastWrite     LastAccess
        ")]
        private void When_moving_file_to_sibling_directory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, "srcFolder", sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        #region Attributes in file move

        [Theory]
        [WatcherNotifyTestData(@"
            - srcFolder\source.txt                          @ FileName
            + dstFolder\target.txt                          @ FileName
            * dstFolder                                     @ LastWrite     LastAccess
        ")]
        private void When_moving_archive_file_to_sibling_directory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, "srcFolder", sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            - srcFolder\source.txt                          @ FileName
            + dstFolder\target.txt                          @ FileName
            * dstFolder                                     @ LastWrite     LastAccess
            * dstFolder\target.txt                          @ Attributes
        ")]
        private void When_moving_system_file_to_sibling_directory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, "srcFolder", sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.System)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            - srcFolder\source.txt                          @ FileName
            + dstFolder\target.txt                          @ FileName
            * dstFolder                                     @ LastWrite     LastAccess
        ")]
        private void When_moving_readonly_hidden_system_archive_file_to_sibling_directory_it_must_raise_events(
            NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, "srcFolder", sourceFileName);
            string pathToDestinationDirectory = Path.Combine(directoryToWatch, "dstFolder");
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT",
                    attributes: FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive)
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        #endregion

        #endregion
    }
}
#endif
