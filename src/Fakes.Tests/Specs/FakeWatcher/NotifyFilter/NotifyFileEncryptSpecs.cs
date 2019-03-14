#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
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

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToFileToEncrypt = Path.Combine(pathToContainerDirectory, fileNameToEncrypt);

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

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

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
        private void When_encrypting_file_again_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string fileNameToEncrypt = "file.txt";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToFileToEncrypt = Path.Combine(pathToContainerDirectory, fileNameToEncrypt);

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

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        // TODO: Add spec for directory.
    }
}
#endif
