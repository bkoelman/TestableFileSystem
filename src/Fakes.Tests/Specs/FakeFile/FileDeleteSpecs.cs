using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileDeleteSpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_deleting_file_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.File.Delete(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_deleting_file_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Delete(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_deleting_file_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Delete(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_file_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_file_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(@"c:\dir?i");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_missing_local_file_it_must_succeed()
        {
            // Arrange
            const string existingPath = @"C:\some\file.txt";
            const string missingPath = @"C:\some\other.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(existingPath)
                .Build();

            // Act
            fileSystem.File.Delete(missingPath);

            // Assert
            fileSystem.File.Exists(existingPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_existing_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.Delete(path);

            // Assert
            fileSystem.File.Exists(path).Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_existing_local_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.Delete(path + "  ");

            // Assert
            fileSystem.File.Exists(path).Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_readonly_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(path);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage(
                @"Access to the path 'C:\some\file.txt' is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_file_from_missing_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\other\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(path);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\other\file.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_file_that_is_in_use_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Delete(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage(
                    @"The process cannot access the file 'C:\some\file.txt' because it is being used by another process.");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_file_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingEmptyFile(@"C:\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.Delete(@"\file.txt");

            // Assert
            fileSystem.File.Exists(@"C:\file.txt").Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_relative_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\store\data.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\store");

            // Act
            fileSystem.File.Delete(@"data.txt");

            // Assert
            fileSystem.File.Exists(@"c:\store\data.txt").Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\store\DATA.txt")
                .Build();

            // Act
            fileSystem.File.Delete(@"c:\STORE\data.TXT");

            // Assert
            fileSystem.File.Exists(@"c:\store\DATA.txt").Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_file_for_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(path);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage(
                @"Access to the path 'C:\some\subfolder' is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_file_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(@"c:\some\file.txt\nested.txt");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_file_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(@"c:\some\file.txt\nested.txt\other.txt");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt\other.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_remote_file_on_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\teamshare\folder\doc.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Delete(path);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The network path was not found.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_remote_file_on_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\teamshare\folder\doc.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.Delete(path);

            // Assert
            fileSystem.File.Exists(path).Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_extended_existing_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            fileSystem.File.Delete(@"\\?\C:\some\file.txt");

            // Assert
            fileSystem.File.Exists(@"C:\some\file.txt").Should().BeFalse();
        }
    }
}
