#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    // TODO: Add specs for various features...
    //
    // Property specs:
    // - NotifyFilter
    //   - Review operations for usage of hidden files
    // - InternalBufferSize
    //   - Allow when changed in disposed state
    //   - Test for buffer overflow: Raises Error event; "If the buffer overflows, the entire contents of the buffer is discarded"
    //
    // Method specs:
    // - WaitForChanged (blocking for incoming changes, with timeout or infinite)
    //   - Throw when called in disposed state

    // Miscellaneous specs:
    // - Automatic restart on property change if we were already running
    // - When restarting with different settings, make sure old events from queue are discarded
    // - Multiple watchers attached to the same file system

    public sealed class ConstructSpecs
    {
        [Fact]
        private void When_constructing_watcher_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Assert
                watcher.Path.Should().Be(string.Empty);
                watcher.Filter.Should().Be("*.*");
                watcher.NotifyFilter.Should().Be(NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite);
                watcher.IncludeSubdirectories.Should().BeFalse();
                watcher.EnableRaisingEvents.Should().BeFalse();
                watcher.InternalBufferSize.Should().Be(8192);
            }
        }

        [Fact]
        private void When_constructing_watcher_for_null_path_it_must_fail()
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
        private void When_constructing_watcher_for_empty_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileSystemWatcher(string.Empty, "*.txt");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("The directory name  is invalid.");
        }

        [Fact]
        private void When_constructing_watcher_for_whitespace_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileSystemWatcher(" ");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("The directory name   is invalid.");
        }

        [Fact]
        private void When_constructing_watcher_for_invalid_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileSystemWatcher("::");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("The directory name :: is invalid.");
        }

        [Fact]
        private void When_constructing_watcher_for_invalid_characters_in_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileSystemWatcher(@"c:\SomeFolder?");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage(@"The directory name c:\SomeFolder? is invalid.");
        }

        [Fact]
        private void When_constructing_watcher_for_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileSystemWatcher(@"e:\MissingFolder");

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage(@"The directory name e:\MissingFolder is invalid.");
        }

        [Fact]
        private void When_constructing_watcher_for_existing_directory_it_must_succeed()
        {
            // Arrange
            const string directoryToWatch = @"e:\ExistingFolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            // Act
            using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch))
            {
                // Assert
                watcher.Path.Should().Be(directoryToWatch);
                watcher.Filter.Should().Be("*.*");
            }
        }

        [Fact]
        private void When_constructing_watcher_for_null_filter_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.ConstructFileSystemWatcher(@"c:\", null);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        private void When_constructing_watcher_for_empty_filter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            string filter = string.Empty;

            // Act
            using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(@"c:\", filter))
            {
                // Assert
                watcher.Filter.Should().Be(filter);
            }
        }

        [Fact]
        private void When_constructing_watcher_for_invalid_filter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            const string filter = "::";

            // Act
            using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(@"c:\", filter))
            {
                // Assert
                watcher.Filter.Should().Be(filter);
            }
        }

        [Fact]
        private void When_constructing_watcher_for_valid_filter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            const string filter = "fil*.txt";

            // Act
            using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(@"c:\", filter))
            {
                // Assert
                watcher.Filter.Should().Be(filter);
            }
        }
    }
}
#endif
