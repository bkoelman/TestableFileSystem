using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileExistsSpecs
    {
        [Fact]
        private void When_getting_file_existence_for_null_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.File.Exists(null);

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_empty_string_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.File.Exists(string.Empty);

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.File.Exists(" ");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_invalid_root_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.File.Exists("::");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_invalid_characters_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.File.Exists("ab>c");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_missing_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"C:\some\other.txt");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_existing_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            bool found = fileSystem.File.Exists(path);

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_file_existence_for_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\FILE.txt")
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"c:\Some\file.TXT");

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_file_existence_for_existing_local_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"C:\some\file.txt  ");

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_file_existence_for_existing_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            bool found = fileSystem.File.Exists(@"file.txt");

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void
            When_getting_file_existence_for_existing_relative_local_file_on_different_drive_in_subfolder_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .IncludingEmptyFile(@"D:\other\child.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");
            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            bool found = fileSystem.File.Exists("D:child.txt");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_existing_relative_local_file_on_different_drive_in_root_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .IncludingDirectory(@"D:\other")
                .IncludingEmptyFile(@"D:\child.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");
            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            bool found = fileSystem.File.Exists("D:child.txt");

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_file_existence_for_existing_relative_local_file_on_same_drive_in_subfolder_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"D:\other\child.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            bool found = fileSystem.File.Exists("D:child.txt");

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_file_existence_for_existing_relative_local_file_on_same_drive_in_root_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"D:\other")
                .IncludingEmptyFile(@"D:\child.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            bool found = fileSystem.File.Exists("D:child.txt");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_existing_local_file_that_exists_as_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            bool found = fileSystem.File.Exists(path);

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_below_existing_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"c:\some\file.txt\nested.txt");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_missing_parent_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"C:\other\file.txt");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_missing_network_share_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"\\teamshare\documents\some.doc");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_missing_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\teamshare\documents")
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"\\teamshare\documents\some.doc");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_existing_remote_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\teamshare\documents\work.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            bool found = fileSystem.File.Exists(path);

            // Assert
            found.Should().BeTrue();
        }

        [Fact]
        private void When_getting_file_existence_for_reserved_name_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.File.Exists("NUL");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_missing_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"\\?\C:\some\other.txt");

            // Assert
            found.Should().BeFalse();
        }

        [Fact]
        private void When_getting_file_existence_for_existing_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            bool found = fileSystem.File.Exists(@"\\?\C:\some\file.txt");

            // Assert
            found.Should().BeTrue();
        }
    }
}
