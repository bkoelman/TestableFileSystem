using System;
using FluentAssertions;
using Xunit;

namespace TestableFileSystem.Fakes.Tests
{
    public sealed class AbsolutePathSpecs
    {
        [Fact]
        private void When_creating_drive_letter_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"C:\");

            // Assert
            path.Components.Should().HaveCount(1);
            path.Name.Should().Be("C:");
        }

        [Fact]
        private void When_creating_path_starting_with_drive_letter_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"C:\docs");

            // Assert
            path.Components.Should().HaveCount(2);
            path.Components[0].Should().Be("C:");
            path.Components[1].Should().Be("docs");
        }

        [Fact]
        private void When_creating_network_share_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"\\teamserver");

            // Assert
            path.Components.Should().HaveCount(1);
            path.Name.Should().Be(@"\\teamserver");
        }

        [Fact]
        private void When_creating_path_starting_with_network_share_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"\\teamserver\management\reports");

            // Assert
            path.Components.Should().HaveCount(3);
            path.Components[0].Should().Be(@"\\teamserver");
            path.Components[1].Should().Be("management");
            path.Components[2].Should().Be("reports");
        }

        [Fact]
        private void When_network_share_has_invalid_name_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => new AbsolutePath(@"\\team*server");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage(@"The path '\\team*server' is invalid.*");
        }

        [Fact]
        private void When_directory_has_invalid_name_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => new AbsolutePath(@"c:\games\try?me");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage(@"The path 'c:\games\try?me' is invalid.*");
        }

        [Fact]
        private void When_creating_relative_path_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => new AbsolutePath(@"docs\work");

            // Assert
            action.ShouldThrow<ArgumentException>()
                .WithMessage("Path must start with drive letter or network share.*");
        }

        [Fact]
        private void When_using_self_references_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"C:\docs\.\games");

            // Assert
            path.Components.Should().HaveCount(3);
            path.Components[0].Should().Be("C:");
            path.Components[1].Should().Be("docs");
            path.Components[2].Should().Be("games");
        }

        [Fact]
        private void When_using_parent_references_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"C:\docs\..\games");

            // Assert
            path.Components.Should().HaveCount(2);
            path.Components[0].Should().Be("C:");
            path.Components[1].Should().Be("games");
        }

        [Fact]
        private void When_using_parent_references_on_root_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => new AbsolutePath(@"C:\..\games");

            // Assert
            action.ShouldThrow<ArgumentException>()
                .WithMessage(@"The path 'C:\..\games' is invalid.*");
        }
    }
}
