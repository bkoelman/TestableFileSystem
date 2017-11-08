using System;
using FluentAssertions;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs
{
    public sealed class AbsolutePathSpecs
    {
        // TODO: Replace these specs, then remove InternalsVisibleTo attribute.

        [Fact]
        private void When_creating_drive_letter_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"C:\");

            // Assert
            path.GetText().Should().Be(@"C:\");
            path.Components.Should().HaveCount(1);
            path.Components[0].Should().Be("C:");
        }

        [Fact]
        private void When_creating_path_starting_with_drive_letter_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"C:\docs");

            // Assert
            path.GetText().Should().Be(@"C:\docs");
            path.Components.Should().HaveCount(2);
            path.Components[0].Should().Be("C:");
            path.Components[1].Should().Be("docs");
        }

        [Fact]
        private void When_creating_extended_path_starting_with_drive_letter_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"\\?\C:\docs");

            // Assert
            path.GetText().Should().Be(@"\\?\C:\docs");
            path.Components.Should().HaveCount(2);
            path.Components[0].Should().Be("C:");
            path.Components[1].Should().Be("docs");
        }

        [Fact]
        private void When_creating_network_share_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"\\teamserver\management");

            // Assert
            path.GetText().Should().Be(@"\\teamserver\management");
            path.Components.Should().ContainSingle(x => x == @"\\teamserver\management");
        }

        [Fact]
        private void When_creating_only_server_part_of_network_share_it_must_fail()
        {
            // Act
            Action action = ActionFactory.IgnoreReturnValue(() => new AbsolutePath(@"\\teamserver\"));

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage(@"The UNC path should be of the form \\server\share.");
        }

        [Fact]
        private void When_creating_path_starting_with_network_share_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"\\teamserver\management\reports");

            // Assert
            path.Components.Should().HaveCount(2);
            path.GetText().Should().Be(@"\\teamserver\management\reports");
            path.Components[0].Should().Be(@"\\teamserver\management");
            path.Components[1].Should().Be("reports");
        }

        [Fact]
        private void When_creating_extended_path_starting_with_network_share_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"\\?\UNC\teamserver\management\reports");

            // Assert
            path.GetText().Should().Be(@"\\?\UNC\teamserver\management\reports");
            path.Components.Should().HaveCount(2);
            path.Components[0].Should().Be(@"\\teamserver\management");
            path.Components[1].Should().Be("reports");
        }

        [Fact]
        private void When_network_share_has_invalid_name_it_must_fail()
        {
            // Act
            Action action = ActionFactory.IgnoreReturnValue(() => new AbsolutePath(@"\\team*server"));

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage(@"The UNC path should be of the form \\server\share.*");
        }

        [Fact]
        private void When_directory_has_invalid_name_it_must_fail()
        {
            // Act
            Action action = ActionFactory.IgnoreReturnValue(() => new AbsolutePath(@"c:\games\try?me"));

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage(@"Illegal characters in path.*");
        }

        [Fact]
        private void When_creating_relative_path_it_must_fail()
        {
            // Act
            Action action = ActionFactory.IgnoreReturnValue(() => new AbsolutePath(@"docs\work"));

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.*");
        }

        [Fact]
        private void When_using_self_references_it_must_succeed()
        {
            // Act
            var path = new AbsolutePath(@"C:\docs\.\games");

            // Assert
            path.GetText().Should().Be(@"C:\docs\games");
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
            path.GetText().Should().Be(@"C:\games");
            path.Components.Should().HaveCount(2);
            path.Components[0].Should().Be("C:");
            path.Components[1].Should().Be("games");
        }

        [Fact]
        private void When_using_parent_references_on_root_it_must_ignore_them()
        {
            // Act
            var path = new AbsolutePath(@"C:\..\games");

            // Assert
            path.GetText().Should().Be(@"C:\games");
            path.Components.Should().HaveCount(2);
            path.Components[0].Should().Be("C:");
            path.Components[1].Should().Be("games");
        }

        [Fact]
        private void When_using_reverse_slashes_it_must_normalize_them()
        {
            // Act
            var path = new AbsolutePath(@"C:\docs/in/sub\folder");

            // Assert
            path.GetText().Should().Be(@"C:\docs\in\sub\folder");
            path.Components.Should().HaveCount(5);
            path.Components[0].Should().Be("C:");
            path.Components[1].Should().Be("docs");
            path.Components[2].Should().Be("in");
            path.Components[3].Should().Be("sub");
            path.Components[4].Should().Be("folder");
        }

        [Fact]
        private void When_using_win32_device_namespace_it_must_fail()
        {
            // Act
            Action action = ActionFactory.IgnoreReturnValue(() => new AbsolutePath(@"\\.\COM56"));

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Only Win32 File Namespaces are supported.");
        }

        [Fact]
        private void When_using_NT_namespace_it_must_fail()
        {
            // Act
            Action action = ActionFactory.IgnoreReturnValue(() => new AbsolutePath(@"\\?\GLOBALROOT"));

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Only Win32 File Namespaces are supported.");
        }

        [Fact]
        private void When_using_reserved_name_in_directory_it_must_fail()
        {
            // Act
            Action action = ActionFactory.IgnoreReturnValue(() => new AbsolutePath(@"c:\nul\documents"));

            // Assert
            action.ShouldThrow<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_using_reserved_name_in_file_it_must_fail()
        {
            // Act
            Action action = ActionFactory.IgnoreReturnValue(() => new AbsolutePath("com1"));

            // Assert
            action.ShouldThrow<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }
    }
}
