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
    public sealed class NotifyPathGetTempFileNameSpecs : WatcherSpecs
    {
        [Theory]
        [WatcherNotifyTestData(@"
            + Temp\tmp1111.tmp                              @ FileName
        ")]
        private void When_getting_temp_file_name_it_must_raise_events(NotifyFilters filters, [NotNull] string expectedText)
        {
            // Arrange
            const string directoryToWatch = @"c:\User";
            const string tempDirectory = @"c:\User\Temp";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .WithEmptyFilesInTempDirectory(tempDirectory, 0x1111)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = filters;
                watcher.IncludeSubdirectories = true;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    fileSystem.Path.GetTempFileName();

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
