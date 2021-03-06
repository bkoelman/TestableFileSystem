﻿#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
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
            action.Should().ThrowExactly<ArgumentNullException>();
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
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The directory name  is invalid.");
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
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The directory name   is invalid.");
        }

        [Fact]
        private void When_constructing_watcher_for_invalid_drive_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileSystemWatcher("_:");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The directory name _: is invalid.");
        }

        [Fact]
        private void When_constructing_watcher_for_wildcard_characters_in_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileSystemWatcher(@"c:\SomeFolder?");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(@"The directory name c:\SomeFolder? is invalid.");
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
            action.Should().ThrowExactly<ArgumentException>().WithMessage(@"The directory name e:\MissingFolder is invalid.");
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
            action.Should().ThrowExactly<ArgumentNullException>();
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
        private void When_constructing_watcher_for_invalid_drive_filter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            const string filter = "_:";

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
