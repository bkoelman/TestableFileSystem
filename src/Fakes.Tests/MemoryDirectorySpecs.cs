using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests
{
    public sealed class MemoryDirectorySpecs
    {
        [Fact]
        private void When_getting_directory_that_exists_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"C:\some\folder");

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_directory_that_does_not_exist_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new MemoryFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"C:\other\folder");

            // Assert
            found.Should().BeFalse();
        }
    }
}
