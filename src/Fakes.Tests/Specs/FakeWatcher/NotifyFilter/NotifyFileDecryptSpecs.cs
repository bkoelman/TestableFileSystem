#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher.NotifyFilter
{
    public sealed class NotifyFileDecryptSpecs : WatcherSpecs
    {
        [Theory]
        [WatcherNotifyTestData(@"
            * Container                                     @                       LastWrite   LastAccess
            * Container\file.txt                            @                       LastWrite
            + Container\EFS0.TMP                            @ FileName
            * Container\EFS0.TMP                            @           Size        LastWrite
            * Container\EFS0.TMP                            @                       LastWrite   LastAccess
            * Container\file.txt                            @           Size
            * Container\file.txt                            @           Size
            * Container\file.txt                            @                                   LastAccess
            * Container\file.txt                            @           Attributes
            - Container\EFS0.TMP                            @ FileName
        ")]
        private void When_decrypting_file_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string fileNameToDecrypt = "file.txt";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToFileToDecrypt = Path.Combine(pathToContainerDirectory, fileNameToDecrypt);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToDecrypt, "SecretContent")
                .Build();

            fileSystem.File.Encrypt(pathToFileToDecrypt);

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Decrypt(pathToFileToDecrypt);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    string text = string.Join(Environment.NewLine, listener.GetEventsCollectedAsText());
                    text.Should().Be(expectedText);
                }
            }
        }

        [Fact]
        private void When_decrypting_unencrypted_file_it_must_not_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string containerDirectoryName = "Container";
            const string fileNameToDecrypt = "file.txt";

            string pathToContainerDirectory = Path.Combine(directoryToWatch, containerDirectoryName);
            string pathToFileToDecrypt = Path.Combine(pathToContainerDirectory, fileNameToDecrypt);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(pathToFileToDecrypt, "SecretContent")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.File.Decrypt(pathToFileToDecrypt);

                    watcher.FinishAndWaitForFlushed(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().BeEmpty();
                }
            }
        }

        // TODO: Add spec for directory.
    }
}
#endif
