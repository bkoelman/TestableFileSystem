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
    public sealed class NotifyFileEncryptSpecs : WatcherSpecs
    {
        [Theory]
        [WatcherNotifyTestData(@"
            * Container                                     @                       LastWrite   LastAccess
            * Container\file.txt                            @                       LastWrite
            + Container\EFS0.TMP                            @ FileName
            * Container\file.txt                            @           Attributes
            * Container\EFS0.TMP                            @           Size        LastWrite
            * Container\EFS0.TMP                            @                       LastWrite   LastAccess
            * Container\file.txt                            @           Size
            * Container\file.txt                            @           Size
            * Container\file.txt                            @                                   LastAccess
            - Container\EFS0.TMP                            @ FileName
        ")]
        private void When_encrypting_file_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string fileNameToEncrypt = "file.txt";

            string pathToFileToEncrypt = Path.Combine(directoryToWatch, containerDirectoryName, fileNameToEncrypt);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToEncrypt, "SecretContent")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Encrypt(pathToFileToEncrypt);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container                                     @ LastWrite   LastAccess
        ")]
        private void When_encrypting_encrypted_file_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string fileNameToEncrypt = "file.txt";

            string pathToFileToEncrypt = Path.Combine(directoryToWatch, containerDirectoryName, fileNameToEncrypt);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToEncrypt, "SecretContent")
                .Build();

            fileSystem.File.Encrypt(pathToFileToEncrypt);

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Encrypt(pathToFileToEncrypt);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container\Subfolder                           @               LastWrite   LastAccess
            * Container\Subfolder                           @ Attributes
            * Container\Subfolder                           @ Attributes
        ")]
        private void When_encrypting_empty_directory_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string directoryNameToEncrypt = "Subfolder";

            string pathToDirectoryToEncrypt = Path.Combine(directoryToWatch, containerDirectoryName, directoryNameToEncrypt);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToEncrypt)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Encrypt(pathToDirectoryToEncrypt);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Theory]
        [WatcherNotifyTestData(@"
            * Container\Subfolder                           @               LastWrite   LastAccess
            * Container                                     @               LastWrite   LastAccess
            * Container\Subfolder                           @ Attributes
            * Container\Subfolder                           @               LastWrite
            * Container\Subfolder                           @ Attributes
        ")]
        private void When_encrypting_nonempty_directory_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string directoryNameToEncrypt = "Subfolder";

            string pathToDirectoryToEncrypt = Path.Combine(directoryToWatch, containerDirectoryName, directoryNameToEncrypt);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(Path.Combine(pathToDirectoryToEncrypt, "file.txt"), "SecretContent")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Encrypt(pathToDirectoryToEncrypt);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Fact]
        private void When_encrypting_encrypted_empty_directory_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string directoryNameToEncrypt = "Subfolder";

            string pathToDirectoryToEncrypt = Path.Combine(directoryToWatch, containerDirectoryName, directoryNameToEncrypt);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(pathToDirectoryToEncrypt)
                .Build();

            fileSystem.File.Encrypt(pathToDirectoryToEncrypt);

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Encrypt(pathToDirectoryToEncrypt);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }
    }
}
#endif
