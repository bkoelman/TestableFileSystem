#if !NETCOREAPP1_1
using System;
using System.Threading;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class FinishAndWaitForFlushedSpecs : WatcherSpecs
    {
        [Fact]
        private void When_flushing_with_negative_timeout_it_must_fail()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.FinishAndWaitForFlushed(-5);

                // Assert
                action.Should().ThrowExactly<ArgumentOutOfRangeException>()
                    .WithMessage("Specified argument was out of the range of valid values.*");
            }
        }

        [Fact]
        private void When_flushing_and_timeout_expires_it_must_fail()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.Created += (sender, args) => { Thread.Sleep(500); };

                watcher.EnableRaisingEvents = true;

                fileSystem.File.WriteAllText(@"c:\some\file.txt", "Content");

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.FinishAndWaitForFlushed(1);

                // Assert
                action.Should().ThrowExactly<TimeoutException>()
                    .WithMessage("Timed out waiting for notification event handlers to finish.");
            }
        }
    }
}
#endif
