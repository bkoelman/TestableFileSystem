using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs
{
    public sealed class AbsolutePathSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_drive_letter_it_must_succeed()
        {
            // Arrange
            const string path = @"E:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Assert
            info.FullName.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_path_starting_with_drive_letter_it_must_succeed()
        {
            // Arrange
            const string path = @"E:\Documents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Assert
            info.FullName.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_extended_path_starting_with_drive_letter_it_must_succeed()
        {
            // Arrange
            const string path = @"\\?\E:\Documents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Assert
            info.FullName.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_network_share_with_hostname_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkShare();

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Assert
            info.FullName.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_network_share_with_IPv4_address_it_must_succeed()
        {
            // Arrange
            const string path = @"\\192.168.0.1\management";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Assert
            info.FullName.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_network_share_with_escaped_IPv6_address_it_must_succeed()
        {
            // https://msdn.microsoft.com/en-us/library/aa385353.aspx
            // The IPv6 literal format must be used so that the IPv6 address is parsed correctly. An IPv6 literal address is of the form:
            // ipv6-address with the ':' characters replaced by '-' characters, followed by the ".ipv6-literal.net" string.

            // Arrange
            string address = "fe80::ddad:d5e:41a2:43e7%5".Replace(":", "-") + ".ipv6-literal.net";
            string path = @"\\" + address + @"\management";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Assert
            info.FullName.Should().Be(path);
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_creating_network_host_without_share_it_must_succeed(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkHostWithoutShare() + @"\";

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                IFileInfo info = fileSystem.ConstructFileInfo(path);

                // Assert
                info.FullName.Should().Be(path);
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_remote_path_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkFileAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Assert
            info.FullName.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_extended_remote_path_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkFileAtDepth(1, true);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Assert
            info.FullName.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_network_share_has_wildcard_characters_it_must_fail()
            {
                // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                    .Build();

                // Act
            Action action = () => fileSystem.ConstructFileInfo(PathFactory.NetworkHostWithoutShare() + "*");

                // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(@"Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_directory_has_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"c:\games\try?me");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(@"Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_path_with_whitespace_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"c:\some\  \other");

            // Assert
            info.FullName.Should().Be(@"c:\some\other");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_path_with_multiple_separators_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"c:\some\\\\\other");

            // Assert
            info.FullName.Should().Be(@"c:\some\other");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_relative_path_it_must_fail()
        {
            // Arrange
            var fileSystemBuilder = new FakeFileSystemBuilder();

            // Act
            Action action = () => fileSystemBuilder.IncludingEmptyFile(@"docs\work");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_self_references_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"C:\docs\. \games\.");

            // Assert
            info.FullName.Should().Be(@"C:\docs\games");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_parent_references_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"C:\docs\.. \games\other\..");

            // Assert
            info.FullName.Should().Be(@"C:\games");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_parent_reference_above_root_it_must_ignore_them()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"C:\..\games");

            // Assert
            info.FullName.Should().Be(@"C:\games");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_reverse_slashes_it_must_normalize_them()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"C:\docs/in/sub/\folder/");

            // Assert
            info.FullName.Should().Be(@"C:\docs\in\sub\folder\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_win32_device_namespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"\\.\COM56");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("Only Win32 File Namespaces are supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_NT_namespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"\\?\GLOBALROOT");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("Only Win32 File Namespaces are supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_reserved_name_in_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"c:\nul\documents");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_reserved_name_in_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"com1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_trailing_space_it_must_trim()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"c:\some \a ");

            // Assert
            info.FullName.Should().Be(@"c:\some\a");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_trailing_thin_space_it_must_preserve()
        {
            // Arrange
            const char thinSpace = (char)0x2009;

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"c:\some\a" + thinSpace);

            // Assert
            info.FullName.Should().Be(@"c:\some\a" + thinSpace);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_trailing_dot_it_must_trim()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"c:\some.\a.");

            // Assert
            info.FullName.Should().Be(@"c:\some\a");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_using_trailing_dots_and_spaces_it_must_trim()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"c:\some\. . \a. . ");

            // Assert
            info.FullName.Should().Be(@"c:\some\a");
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_using_null_terminator_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.ConstructFileInfo(@"c:\" + '\0' + "doc.txt");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
            }
        }
    }
}
