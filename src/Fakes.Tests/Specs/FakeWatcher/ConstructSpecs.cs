#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class ConstructSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
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

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_watcher_for_null_path_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.ConstructFileSystemWatcher(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_watcher_for_empty_path_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.ConstructFileSystemWatcher(string.Empty, "*.txt");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The directory name  is invalid.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_watcher_for_whitespace_path_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.ConstructFileSystemWatcher(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The directory name ' ' does not exist.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_watcher_for_invalid_drive_path_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.ConstructFileSystemWatcher("_:");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The directory name '_:' does not exist.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_watcher_for_wildcard_characters_in_path_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.ConstructFileSystemWatcher(@"c:\SomeFolder?");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage(@"The directory name 'c:\SomeFolder?' does not exist.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_watcher_for_missing_directory_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.ConstructFileSystemWatcher(@"c:\MissingFolder\25D6015F-1843-4610-AAF9-06EBB076A81F");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage(@"The directory name 'c:\MissingFolder\25D6015F-1843-4610-AAF9-06EBB076A81F' does not exist.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_watcher_for_null_filter_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.ConstructFileSystemWatcher(@"c:\", null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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
