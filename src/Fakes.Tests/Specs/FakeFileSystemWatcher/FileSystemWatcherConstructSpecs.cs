#if !NETCOREAPP1_1
using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileSystemWatcher
{
    // TODO: Add specs for various features:
    // - Assert on initial defaults after construction
    // - Start watching without proper setup
    // - Automatic restart on property change if we were already running
    // - Convert incoming empty filter (via property or ctor) into *.*
    // - Multiple watchers attached to the same file system
    // - Test for various wildcards (Filter)
    // - Test for the various events (NotifyFilter)
    // - Test for single file, directory or directory subtree
    // - Test blocking for incoming changes (with timeout or infinite)
    // - Test for buffer overflow: "If the buffer overflows, the entire contents of the buffer is discarded"
    // - Test for hidden files
    // - Throw when calling properties/methods in disposed state
    // - When restarting with different settings, make sure old events from queue do not come through
    // - Works for root of drive/UNC path?

    public sealed class FileSystemWatcherConstructSpecs
    {
        [Fact]
        private void When_constructing_watcher_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher();

            // Assert
            watcher.Should().NotBeNull();
        }

        [Fact]
        private void When_constructing_watcher_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.ConstructFileSystemWatcher(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_constructing_watcher_for_path_and_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable AssignNullToNotNullAttribute
            Action action = () => fileSystem.ConstructFileSystemWatcher(@"c:\", null);
            // ReSharper restore AssignNullToNotNullAttribute

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }
    }
}
#endif
