using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs
{
    public sealed class AbsolutePathSpecs
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        private void When_creating_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\teamserver\management";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Assert
            info.FullName.Should().Be(path);
        }

        [Fact]
        private void When_creating_only_server_part_of_network_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"\\teamserver\");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage(@"The UNC path should be of the form \\server\share.");
        }

        [Fact]
        private void When_creating_path_starting_with_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\teamserver\management\reports";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Assert
            info.FullName.Should().Be(path);
        }

        [Fact]
        private void When_creating_extended_path_starting_with_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\?\UNC\teamserver\management\reports";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Assert
            info.FullName.Should().Be(path);
        }

        [Fact]
        private void When_network_share_has_invalid_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"\\team*server");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage(@"The UNC path should be of the form \\server\share.*");
        }

        [Fact]
        private void When_directory_has_invalid_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"c:\games\try?me");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage(@"Illegal characters in path.*");
        }

        [Fact]
        private void When_creating_relative_path_it_must_fail()
        {
            // Arrange
            var fileSystemBuilder = new FakeFileSystemBuilder();

            // Act
            Action action = () => fileSystemBuilder.IncludingEmptyFile(@"docs\work");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.*");
        }

        [Fact]
        private void When_using_self_references_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"C:\docs\.\games");

            // Assert
            info.FullName.Should().Be(@"C:\docs\games");
        }

        [Fact]
        private void When_using_parent_references_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"C:\docs\..\games");

            // Assert
            info.FullName.Should().Be(@"C:\games");
        }

        [Fact]
        private void When_using_parent_references_on_root_it_must_ignore_them()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"C:\..\games");

            // Assert
            info.FullName.Should().Be(@"C:\games");
        }

        [Fact]
        private void When_using_reverse_slashes_it_must_normalize_them()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo info = fileSystem.ConstructFileInfo(@"C:\docs/in/sub\folder");

            // Assert
            info.FullName.Should().Be(@"C:\docs\in\sub\folder");
        }

        [Fact]
        private void When_using_win32_device_namespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"\\.\COM56");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Only Win32 File Namespaces are supported.");
        }

        [Fact]
        private void When_using_NT_namespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"\\?\GLOBALROOT");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Only Win32 File Namespaces are supported.");
        }

        [Fact]
        private void When_using_reserved_name_in_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"c:\nul\documents");

            // Assert
            action.ShouldThrow<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_using_reserved_name_in_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(@"com1");

            // Assert
            action.ShouldThrow<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }
    }
}
