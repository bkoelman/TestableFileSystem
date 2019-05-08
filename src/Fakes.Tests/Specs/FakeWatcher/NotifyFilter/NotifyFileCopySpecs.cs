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
    public sealed class NotifyFileCopySpecs : WatcherSpecs
    {
        [NotNull]
        private static readonly byte[] LargeFileBuffer = BufferFactory.Create(1024 * 4);

        [Theory]
        [WatcherNotifyTestData(@"
            + Container\target.txt                          @ FileName
            * Container\target.txt                          @           LastWrite               Size
            * Container\target.txt                          @           LastWrite   LastAccess
        ")]
        private void When_copying_file_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceFile = Path.Combine(pathToContainerDirectory, sourceFileName);
            string pathToDestinationFile = Path.Combine(pathToContainerDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "Example")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            + Container\Subfolder\target.txt                @ FileName
            * Container\Subfolder\target.txt                @           LastWrite               Size
            * Container\Subfolder\target.txt                @           LastWrite   LastAccess
        ")]
        private void When_copying_file_to_subdirectory_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string sourceFileName = "source.txt";
            const string destinationDirectoryName = "Subfolder";
            const string destinationFileName = "target.txt";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToSourceFile = Path.Combine(pathToContainerDirectory, sourceFileName);
            string pathToDestinationDirectory = Path.Combine(pathToContainerDirectory, destinationDirectoryName);
            string pathToDestinationFile = Path.Combine(pathToDestinationDirectory, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "Example")
                .IncludingDirectory(pathToDestinationDirectory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            + target.txt                                    @ FileName
            * target.txt                                    @           LastWrite                   Size
            * target.txt                                    @           LastWrite       LastAccess
            * target.txt                                    @                                       Size
        ")]
        private void When_copying_large_file_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(pathToSourceFile, LargeFileBuffer)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            + target.txt                                    @ FileName
            * target.txt                                    @           LastWrite
        ")]
        private void When_copying_empty_file_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * target.txt                                    @ LastWrite
            * target.txt                                    @ LastWrite     LastAccess  Size
            * target.txt                                    @ LastWrite     LastAccess  Size
        ")]
        private void When_copying_over_existing_file_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "NEW-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * target.txt                                    @ LastWrite
            * target.txt                                    @ LastWrite     LastAccess  Size
            * target.txt                                    @ LastWrite     LastAccess  Size
        ")]
        private void When_copying_over_existing_file_with_same_size_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "--NEW-CONTENT!--")
                .IncludingTextFile(pathToDestinationFile, "PREVIOUS-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * target.txt                                    @ LastWrite
            * target.txt                                    @ LastWrite     LastAccess  Size
            * target.txt                                    @ LastWrite     LastAccess  Size
        ")]
        private void When_copying_over_existing_file_with_same_content_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "FILE-CONTENT")
                .IncludingTextFile(pathToDestinationFile, "FILE-CONTENT")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * target.txt                                    @ LastWrite
            * target.txt                                    @ LastWrite     LastAccess
        ")]
        private void When_copying_empty_file_over_existing_empty_file_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .IncludingEmptyFile(pathToDestinationFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * target.txt                                    @ LastWrite
            * target.txt                                    @ LastWrite     LastAccess  Size
        ")]
        private void When_copying_empty_file_over_existing_file_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToSourceFile)
                .IncludingTextFile(pathToDestinationFile, "ORIGINAL")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * target.txt                                    @ LastWrite
            * target.txt                                    @ LastWrite     LastAccess  Size
            * target.txt                                    @ LastWrite     LastAccess
        ")]
        private void When_copying_over_existing_empty_file_it_must_raise_events(NotifyFilters filters,
            [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string sourceFileName = "source.txt";
            const string destinationFileName = "target.txt";

            string pathToSourceFile = Path.Combine(directoryToWatch, sourceFileName);
            string pathToDestinationFile = Path.Combine(directoryToWatch, destinationFileName);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToSourceFile, "CONTENT")
                .IncludingEmptyFile(pathToDestinationFile)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Copy(pathToSourceFile, pathToDestinationFile, true);

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
