using System;
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

        [Fact]
        private void When_creating_builder_without_drive_C_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.WithoutDefaultDriveC().Build();

            // Assert
            action.ShouldThrow<InvalidOperationException>().WithMessage("System contains no drives.");
        }

        [Fact]
        private void When_creating_builder_with_drive_D_only_it_must_succeed()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder().WithoutDefaultDriveC();

            // Act
            FakeFileSystem fileSystem = builder.IncludingDirectory("D:").Build();

            // Assert
            fileSystem.Directory.Exists(@"c:\").Should().BeFalse();
            fileSystem.Directory.Exists(@"d:\").Should().BeTrue();
        }
    }
}
