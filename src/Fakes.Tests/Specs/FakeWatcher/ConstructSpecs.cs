#if !NETCOREAPP1_1
using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    // TODO: Add specs for various features...
    //
    // File/Directory operation specs:
    // - Test for the various events (.NotifyFilter)
    //      - Test for single file, directory or directory subtree
    //      - Test for various wildcards (.Filter)
    //      - Test for hidden files?
    //      - Works for root of drive/UNC path?
    //      - Test blocking for incoming changes (with timeout or infinite)
    //
    // Construction specs:
    // - Construct with invalid path characters, different casing, trailing whitespace etc.
    // - Construct with extended path => use extended path in event.FullPath
    // - Assert on initial defaults after construction
    // - Start watching without proper setup
    // - Convert incoming empty filter (via property or ctor) into *.*
    //
    // Miscellaneous specs:
    // - Automatic restart on property change if we were already running
    // - Multiple watchers attached to the same file system
    // - Test for buffer overflow: Raises Error event; "If the buffer overflows, the entire contents of the buffer is discarded"
    // - Throw when calling properties/methods in disposed state
    // - When restarting with different settings, make sure old events from queue are discarded

    public sealed class ConstructSpecs
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
            action.Should().Throw<ArgumentNullException>();
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
            action.Should().Throw<ArgumentNullException>();
        }
    }
}
#endif
