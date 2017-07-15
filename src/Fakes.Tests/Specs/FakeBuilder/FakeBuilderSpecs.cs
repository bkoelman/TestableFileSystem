using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeBuilder
{
    public sealed class FakeBuilderSpecs
    {
        [Fact]
        private void When_creating_builder_it_must_include_drive_C()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            FakeFileSystem fileSystem = builder.Build();

            // Assert
            fileSystem.Directory.Exists(@"c:\").Should().BeTrue();
        }
    }
}
