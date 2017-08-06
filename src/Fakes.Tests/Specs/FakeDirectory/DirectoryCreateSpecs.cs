using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryCreateSpecs
    {
        [Fact]
        private void When_creating_directory_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.CreateDirectory(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_creating_directory_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Path cannot be the empty string or all whitespace.*");
        }

        [Fact]
        private void When_creating_directory_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_creating_directory_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_creating_directory_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory(@"c:\dir?i");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_creating_existing_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(path);

            // Assert
            info.Should().NotBeNull();
            fileSystem.Directory.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_creating_missing_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(path);

            // Assert
            info.Should().NotBeNull();
            fileSystem.Directory.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_creating_missing_local_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(@"C:\some ");

            // Assert
            info.Should().NotBeNull();
            fileSystem.Directory.Exists(@"C:\some").Should().BeTrue();
        }

        [Fact]
        private void When_creating_missing_local_directory_tree_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\folder\tree";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(path);

            // Assert
            info.Should().NotBeNull();
            info.FullName.Should().Be(path);
            fileSystem.Directory.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_creating_existing_drive_it_must_succeed()
        {
            // Arrange
            const string drive = @"D:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(drive)
                .Build();

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(drive);

            // Assert
            info.Should().NotBeNull();
            fileSystem.Directory.Exists(drive).Should().BeTrue();
        }

        [Fact]
        private void When_creating_drive_it_must_fail()
        {
            // Arrange
            const string drive = @"X:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory(drive);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'X:\'.");
        }

        [Fact]
        private void When_creating_directory_on_missing_drive_it_must_fail()
        {
            // Arrange
            const string drive = @"X:\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory(drive);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'X:\folder'.");
        }

        [Fact]
        private void When_creating_current_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\store";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(path);

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(path);

            // Assert
            info.Should().NotBeNull();
            fileSystem.Directory.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_creating_remote_current_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\documents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(path);

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(path);

            // Assert
            info.Should().NotBeNull();
            fileSystem.Directory.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_creating_above_root_of_drive_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(@"C:\..\..\store");

            // Assert
            info.Should().NotBeNull();
            info.FullName.Should().Be(@"C:\store");
            fileSystem.Directory.Exists(@"C:\store").Should().BeTrue();
        }

        [Fact]
        private void When_creating_directory_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.Directory.CreateDirectory(@"\other");

            // Assert
            fileSystem.Directory.Exists(@"C:\other").Should().BeTrue();
        }

        [Fact]
        private void When_creating_existing_local_relative_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\store\data")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\store\");

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(@"data");

            // Assert
            info.Should().NotBeNull();
            fileSystem.Directory.Exists(@"c:\store\data").Should().BeTrue();
        }

        [Fact]
        private void When_creating_missing_local_relative_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\store\")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\store\");

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(@"data");

            // Assert
            info.Should().NotBeNull();
            fileSystem.Directory.Exists(@"c:\store\data").Should().BeTrue();
        }

        [Fact]
        private void When_creating_local_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            fileSystem.Directory.CreateDirectory(@"c:\STORE-data");

            // Assert
            fileSystem.Directory.Exists(@"C:\store-DATA").Should().BeTrue();
        }

        [Fact]
        private void When_creating_local_directory_for_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory(path);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"Cannot create 'c:\some\file.txt' because a file or directory with the same name already exists.");
        }

        [Fact]
        private void When_creating_local_directory_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory(@"C:\some\file.txt\sub");

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"Cannot create 'C:\some\file.txt' because a file or directory with the same name already exists.");
        }

        [Fact]
        private void When_creating_local_directory_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory(@"C:\some\file.txt\sub\deeper");

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"Cannot create 'C:\some\file.txt' because a file or directory with the same name already exists.");
        }

        [Fact]
        private void When_creating_only_server_part_of_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\fileserver";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory(path);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage(@"The UNC path should be of the form \\server\share.");
        }

        [Fact]
        private void When_creating_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\fileserver\documents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory(path);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path '\\fileserver\documents'.");
        }

        [Fact]
        private void When_creating_directory_below_missing_network_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.CreateDirectory(@"\\server\share\team");

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The network path was not found");
        }

        [Fact]
        private void When_creating_remote_directory_tree_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\teamshare\folder")
                .Build();

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(@"\\teamshare\folder\documents\for\us");

            // Assert
            info.Should().NotBeNull();
            fileSystem.Directory.Exists(@"\\teamshare\folder\documents\for\us").Should().BeTrue();
        }

        [Fact]
        private void When_creating_extended_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo info = fileSystem.Directory.CreateDirectory(@"\\?\C:\some\folder");

            // Assert
            info.Should().NotBeNull();
            fileSystem.Directory.Exists(@"C:\some\folder").Should().BeTrue();
        }
    }
}
