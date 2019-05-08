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
    public sealed class NotifyFileMoveSpecs : WatcherSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_renaming_file_to_itself_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileName = "file.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, fileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch.ToUpperInvariant(), fileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
        [WatcherNotifyTestData(@"
            > Container\file.TXT => Container\FILE.txt      @ FileName
            * Container                                     @               LastWrite   LastAccess
        ")]
        private void When_renaming_file_to_itself_with_different_casing_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceFileName = "file.TXT";
            const string destinationFileName = "FILE.txt";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceFile = Path.Combine(pathToContainerDirectory, sourceFileName);
            string pathToDestinationFile = Path.Combine(pathToContainerDirectory, destinationFileName);

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
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
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
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        #region Attributes in rename

        [Theory, InvestigateRunOnFileSystem]
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
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
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
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
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
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        #endregion

        [Theory, InvestigateRunOnFileSystem]
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
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
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
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
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
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        #region Attributes in move

        [Theory, InvestigateRunOnFileSystem]
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
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
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
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
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
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        #endregion

        [Theory, InvestigateRunOnFileSystem]
        [WatcherNotifyTestData(@"
            - source.txt                                    @ FileName
        ")]
        private void When_moving_file_out_to_different_drive_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(@"d:\", destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(@"d:\")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
        [WatcherNotifyTestData(@"
            * source.txt                                    @           Attributes      LastAccess
            - source.txt                                    @ FileName
        ")]
        private void When_moving_readonly_file_out_to_different_drive_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(@"d:\", destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT", attributes: FileAttributes.ReadOnly)
                .IncludingDirectory(@"d:\")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory, InvestigateRunOnFileSystem]
        [WatcherNotifyTestData(@"
            + target.txt                                    @ FileName
            * target.txt                                    @           LastWrite                   Size
            * target.txt                                    @           LastWrite       LastAccess
        ")]
        private void When_moving_file_in_from_different_drive_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(@"d:\", sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Move(pathToSourceFile, pathToDestinationFile);

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
