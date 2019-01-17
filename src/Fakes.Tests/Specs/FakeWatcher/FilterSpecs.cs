﻿#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class FilterSpecs : WatcherSpecs
    {
        [Fact(Skip = "TODO")]
        private void When_filtering_directory_with_extension_pattern_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch, "MatchingFile.txt");
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch, "NonMatchingFile");
            string pathToDirectoryToUpdate1 = Path.Combine(directoryToWatch, "MatchingFolder.txt");
            string pathToDirectoryToUpdate2 = Path.Combine(directoryToWatch, "NonMatchingFolder");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .IncludingEmptyFile(pathToFileToUpdate2)
                .IncludingDirectory(pathToDirectoryToUpdate1)
                .IncludingDirectory(pathToDirectoryToUpdate2)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;
                watcher.Filter = "*.TXT";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate2, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate2, FileAttributes.Hidden);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(@"
                        * MatchingFile.txt
                        * MatchingFolder.txt
                        ".TrimLines());
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_filtering_directory_with_name_pattern_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch, "MatchingFile");
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch, "NonMatchingFile");
            string pathToDirectoryToUpdate1 = Path.Combine(directoryToWatch, "MatchingFolder");
            string pathToDirectoryToUpdate2 = Path.Combine(directoryToWatch, "NonMatchingFolder");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .IncludingEmptyFile(pathToFileToUpdate2)
                .IncludingDirectory(pathToDirectoryToUpdate1)
                .IncludingDirectory(pathToDirectoryToUpdate2)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;
                watcher.Filter = "m?tchingF*";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate2, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate2, FileAttributes.Hidden);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(@"
                        * MatchingFile
                        * MatchingFolder
                        ".TrimLines());
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_filtering_directory_tree_with_extension_pattern_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch, "RootMatchingFile.txt");
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch, "RootNonMatchingFile");
            string pathToFileToUpdate3 = Path.Combine(directoryToWatch, "SubFolder", "SubMatchingFile.txt");
            string pathToFileToUpdate4 = Path.Combine(directoryToWatch, "SubFolder", "SubNonMatchingFile");
            string pathToDirectoryToUpdate1 = Path.Combine(directoryToWatch, "RootMatchingFolder.txt");
            string pathToDirectoryToUpdate2 = Path.Combine(directoryToWatch, "RootNonMatchingFolder");
            string pathToDirectoryToUpdate3 = Path.Combine(directoryToWatch, "SubFolder", "SubMatchingFolder.txt");
            string pathToDirectoryToUpdate4 = Path.Combine(directoryToWatch, "SubFolder", "SubNonMatchingFolder");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .IncludingEmptyFile(pathToFileToUpdate2)
                .IncludingEmptyFile(pathToFileToUpdate3)
                .IncludingEmptyFile(pathToFileToUpdate4)
                .IncludingDirectory(pathToDirectoryToUpdate1)
                .IncludingDirectory(pathToDirectoryToUpdate2)
                .IncludingDirectory(pathToDirectoryToUpdate3)
                .IncludingDirectory(pathToDirectoryToUpdate4)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;
                watcher.Filter = "*.TXT";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate2, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate3, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate4, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate2, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate3, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate4, FileAttributes.Hidden);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(@"
                        * RootMatchingFile.txt
                        * RootMatchingFolder.txt
                        * SubFolder\SubMatchingFile.txt
                        * SubFolder\SubMatchingFolder.txt
                        ".TrimLines());
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_filtering_directory_tree_with_name_pattern_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch, "RMatchingFile");
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch, "RNonMatchingFile");
            string pathToFileToUpdate3 = Path.Combine(directoryToWatch, "SubFolder", "SMatchingFile");
            string pathToFileToUpdate4 = Path.Combine(directoryToWatch, "SubFolder", "SNonMatchingFile");
            string pathToDirectoryToUpdate1 = Path.Combine(directoryToWatch, "RMatchingFolder");
            string pathToDirectoryToUpdate2 = Path.Combine(directoryToWatch, "RNonMatchingFolder");
            string pathToDirectoryToUpdate3 = Path.Combine(directoryToWatch, "SubFolder", "SMatchingFolder");
            string pathToDirectoryToUpdate4 = Path.Combine(directoryToWatch, "SubFolder", "SNonMatchingFolder");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .IncludingEmptyFile(pathToFileToUpdate2)
                .IncludingEmptyFile(pathToFileToUpdate3)
                .IncludingEmptyFile(pathToFileToUpdate4)
                .IncludingDirectory(pathToDirectoryToUpdate1)
                .IncludingDirectory(pathToDirectoryToUpdate2)
                .IncludingDirectory(pathToDirectoryToUpdate3)
                .IncludingDirectory(pathToDirectoryToUpdate4)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;
                watcher.Filter = "?matchingF*";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate2, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate3, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate4, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate2, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate3, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate4, FileAttributes.Hidden);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(@"
                        * RMatchingFile
                        * RMatchingFolder
                        * SubFolder\SMatchingFile
                        * SubFolder\SMatchingFolder
                        ".TrimLines());
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_filtering_directory_tree_with_asterisk_dot_pattern_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch, "RootFile");
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch, "SubFolder", "SubFile");
            string pathToDirectoryToUpdate1 = Path.Combine(directoryToWatch, "RootFolder");
            string pathToDirectoryToUpdate2 = Path.Combine(directoryToWatch, "SubFolder", "Nested");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .IncludingDirectory(pathToDirectoryToUpdate1)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;
                watcher.Filter = "*.";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate2, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate2, FileAttributes.Hidden);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().BeEmpty();
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_filtering_directory_tree_with_asterisk_pattern_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch, "RootFile1.txt");
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch, "RootFile2");
            string pathToFileToUpdate3 = Path.Combine(directoryToWatch, "SubFolder", "SubFile1.txt");
            string pathToFileToUpdate4 = Path.Combine(directoryToWatch, "SubFolder", "SubFile2");
            string pathToDirectoryToUpdate1 = Path.Combine(directoryToWatch, "RootFolder1.txt");
            string pathToDirectoryToUpdate2 = Path.Combine(directoryToWatch, "RootFolder2");
            string pathToDirectoryToUpdate3 = Path.Combine(directoryToWatch, "SubFolder", "DeepFolder1.txt");
            string pathToDirectoryToUpdate4 = Path.Combine(directoryToWatch, "SubFolder", "DeepFolder2");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .IncludingEmptyFile(pathToFileToUpdate2)
                .IncludingEmptyFile(pathToFileToUpdate3)
                .IncludingEmptyFile(pathToFileToUpdate4)
                .IncludingDirectory(pathToDirectoryToUpdate1)
                .IncludingDirectory(pathToDirectoryToUpdate2)
                .IncludingDirectory(pathToDirectoryToUpdate3)
                .IncludingDirectory(pathToDirectoryToUpdate4)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;
                watcher.Filter = "*";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate2, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate3, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate4, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate2, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate3, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToDirectoryToUpdate4, FileAttributes.Hidden);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(@"
                        * RootFile1.txt
                        * RootFile2
                        * SubFolder\SubFile1.txt
                        * SubFolder\SubFile2
                        * RootFolder1.txt
                        * RootFolder2
                        * SubFolder\DeepFolder1.txt
                        * SubFolder\DeepFolder2
                        ".TrimLines());
                }
            }
        }

        [Fact(Skip = "TODO")]
        private void When_filtering_directory_tree_with_subdirectory_pattern_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch, "RootFile1.txt");
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch, "SubFolder", "SubFile1.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .IncludingEmptyFile(pathToFileToUpdate2)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;
                watcher.Filter = @"SubFolder\*.txt";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.Hidden);
                    fileSystem.File.SetAttributes(pathToFileToUpdate2, FileAttributes.Hidden);

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
