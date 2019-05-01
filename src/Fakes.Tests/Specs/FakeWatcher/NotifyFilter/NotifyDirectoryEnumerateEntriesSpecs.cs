#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyDirectoryEnumerateEntriesSpecs : WatcherSpecs
    {
        #region Non-recursive

        [Theory]
        [WatcherNotifyTestData(@"
            * EnumerateMe                                   @ LastAccess
        ")]
        private void When_enumerating_entries_for_empty_directory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToEnumerate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * EnumerateMe                                   @ LastAccess
        ")]
        private void When_enumerating_entries_for_non_empty_directory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * EnumerateMe                                   @ LastAccess
        ")]
        private void When_enumerating_entries_for_directory_tree_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "Subfolder", "SubFile.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        #endregion

        #region Pattern

        [Theory]
        [WatcherNotifyTestData(@"
            * EnumerateMe                                   @ LastAccess
        ")]
        private void When_enumerating_entries_with_matching_pattern_for_non_empty_directory_it_must_raise_events(
            NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate, "*.txt");

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * EnumerateMe                                   @ LastAccess
        ")]
        private void When_enumerating_entries_with_non_matching_pattern_for_non_empty_directory_it_must_raise_events(
            NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.doc"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate, "*.txt");

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        #endregion

        #region Recursive

        [Theory]
        [WatcherNotifyTestData(@"
            * EnumerateMe                                   @ LastAccess
        ")]
        private void When_enumerating_entries_recursively_for_empty_directory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToEnumerate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate,
                        searchOption: SearchOption.AllDirectories);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * EnumerateMe                                   @ LastAccess
        ")]
        private void When_enumerating_entries_recursively_for_non_empty_directory_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string directoryNameToEnumerate = "EnumerateMe";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, directoryNameToEnumerate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, "file.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate,
                        searchOption: SearchOption.AllDirectories);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * EnumerateMe                                   @ LastAccess
            * EnumerateMe\TopLevel                          @ LastAccess
            * EnumerateMe\TopLevel\FolderA                  @ LastAccess
            * EnumerateMe\TopLevel\FolderA\SubFolderA       @ LastAccess
            * EnumerateMe\TopLevel\FolderB                  @ LastAccess
            * EnumerateMe\TopLevel\FolderC                  @ LastAccess
            * EnumerateMe\TopLevel\FolderC\SubFolderC       @ LastAccess
        ")]
        private void When_enumerating_entries_recursively_for_directory_tree_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            string pathToDirectoryToEnumerate = Path.Combine(directoryToWatch, "EnumerateMe");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FolderA\SubFolderA"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FolderA\FileInA.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FolderB\FileInB.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FolderC\SubFolderC\FileInSubFolderC.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FileInRoot2.txt"))
                .IncludingEmptyFile(Path.Combine(pathToDirectoryToEnumerate, @"TopLevel\FileInRoot1.txt"))
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Directory.GetFileSystemEntries(pathToDirectoryToEnumerate,
                        searchOption: SearchOption.AllDirectories);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        #endregion
    }
}
#endif
