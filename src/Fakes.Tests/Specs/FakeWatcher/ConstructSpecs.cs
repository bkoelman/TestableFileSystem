﻿#if !NETCOREAPP1_1
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
    // File/Directory operation specs:
    // - Test for the various events (.NotifyFilter)
    //      - Test for single file, directory or directory subtree
    //      - Test for various wildcards (.Filter)
    //      - Test for hidden files?
    //      - Works for root of drive/UNC path?
    //      - Test blocking for incoming changes (with timeout or infinite)
    //
    // Construction specs:
    // - Construct with different casing, trailing whitespace etc.
    // - Construct with extended path => use extended path in event.FullPath
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
        [Fact(Skip = "TODO")]
        private void When_constructing_watcher_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher();

            // Assert
            watcher.Path.Should().Be(string.Empty);
            watcher.Filter.Should().Be("*.*");
            watcher.NotifyFilter.Should().Be(NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite);
            watcher.IncludeSubdirectories.Should().BeFalse();
            watcher.EnableRaisingEvents.Should().BeFalse();
            watcher.InternalBufferSize.Should().Be(8192);
        }

        [Fact(Skip = "TODO")]
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

        [Fact(Skip = "TODO")]
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

        [Fact(Skip = "TODO")]
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

        [Fact(Skip = "TODO")]
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

        [Fact(Skip = "TODO")]
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

        [Fact(Skip = "TODO")]
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

        [Fact(Skip = "TODO")]
        private void When_constructing_watcher_for_existing_directory_it_must_succeed()
        {
            // Arrange
            const string directoryToWatch = @"e:\ExistingFolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            // Act
            var watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch);

            // Assert
            watcher.Path.Should().Be(directoryToWatch);
            watcher.Filter.Should().Be("*.*");
        }

        [Fact(Skip = "TODO")]
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

        [Fact(Skip = "TODO")]
        private void When_constructing_watcher_for_empty_filter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            string filter = string.Empty;

            // Act
            var watcher = fileSystem.ConstructFileSystemWatcher(@"c:\", filter);

            // Assert
            watcher.Filter.Should().Be(filter);
        }

        [Fact(Skip = "TODO")]
        private void When_constructing_watcher_for_invalid_filter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            const string filter = "::";

            // Act
            var watcher  = fileSystem.ConstructFileSystemWatcher(@"c:\", filter);

            // Assert
            watcher.Filter.Should().Be(filter);
        }

        [Fact(Skip = "TODO")]
        private void When_constructing_watcher_for_valid_filter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            const string filter = "fil*.txt";

            // Act
            var watcher = fileSystem.ConstructFileSystemWatcher(@"c:\", filter);

            // Assert
            watcher.Filter.Should().Be(filter);
        }
    }
}
#endif
