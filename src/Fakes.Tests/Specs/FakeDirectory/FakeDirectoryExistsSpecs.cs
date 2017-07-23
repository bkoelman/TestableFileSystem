using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class FakeDirectoryExistsSpecs
    {
        [Fact]
        private void When_getting_directory_existence_for_null_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(null);

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_empty_string_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(string.Empty);

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(" ");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_invalid_root_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists("::");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_invalid_characters_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists("ab>c");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_missing_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"C:\some\folder");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_existing_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(path);

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_directory_existence_for_existing_local_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\FOLDER")
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"c:\SOME\folder");

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_directory_existence_for_existing_local_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"C:\some\folder  ");

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_directory_existence_for_existing_relative_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            bool found = fileSystem.Directory.Exists(@"folder");

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_directory_existence_for_existing_relative_local_directory_on_different_drive_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .IncludingDirectory(@"D:\other\child")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");
            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            bool found = fileSystem.Directory.Exists("D:child");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_local_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\folder.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(path);

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_local_directory_below_existing_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"c:\some\file.txt\subfolder");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_local_directory_whose_parent_does_not_exist_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"C:\other\folder");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_missing_network_share_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"\\teamshare\documents");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_missing_remote_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\teamshare\documents")
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"\\teamshare\documents\sub");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_existing_remote_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"\\teamshare\documents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(path);

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_directory_existence_for_reserved_name_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists("NUL");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_missing_extended_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"\\?\C:\some\folder");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_directory_existence_for_existing_extended_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            bool found = fileSystem.Directory.Exists(@"\\?\C:\some\folder");

            // Assert
            found.Should().BeTrue();
        }
    }
}
