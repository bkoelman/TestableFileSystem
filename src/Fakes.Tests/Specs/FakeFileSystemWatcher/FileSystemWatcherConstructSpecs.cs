#if !NETCOREAPP1_1
using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileSystemWatcher
{
    public sealed class FileSystemWatcherConstructSpecs
    {
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
    }
}
#endif
