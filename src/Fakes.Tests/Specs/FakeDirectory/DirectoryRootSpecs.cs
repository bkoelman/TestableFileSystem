using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryRootSpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_directory_root_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.Directory.GetDirectoryRoot(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_directory_root_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetDirectoryRoot(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_directory_root_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetDirectoryRoot(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetDirectoryRoot("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetDirectoryRoot(@"c:\dir?i");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_missing_local_drive_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"e:\");

            // Assert
            root.Should().Be(@"e:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_missing_local_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"d:\some\folder\path");

            // Assert
            root.Should().Be(@"d:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_missing_local_path_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"f:\some\folder\file.txt  ");

            // Assert
            root.Should().Be(@"f:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_directory_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"C:\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\some");

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"\folder");

            // Assert
            root.Should().Be(@"c:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_relative_local_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"x:\folder\to\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"X:\folder\to");

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot("file.txt");

            // Assert
            root.Should().Be(@"X:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(path);

            // Assert
            root.Should().Be(@"C:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"C:\some\file.txt\deeper");

            // Assert
            root.Should().Be(@"C:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_parent_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"C:\some\file.txt\deeper\more");

            // Assert
            root.Should().Be(@"C:\");
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_directory_root_on_missing_network_share_it_must_succeed(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(@"\\ServerName\ShareName", false);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                string root = fileSystem.Directory.GetDirectoryRoot(path);

                // Assert
                root.Should().Be(path);
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_on_file_below_missing_network_share_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"\\ServerName\ShareName\file.txt");

            // Assert
            root.Should().Be(@"\\ServerName\ShareName");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_missing_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"\\server\share\file.txt");

            // Assert
            root.Should().Be(@"\\server\share");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_existing_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"\\server\share\file.txt");

            // Assert
            root.Should().Be(@"\\server\share");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetDirectoryRoot("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\folder")
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"\\?\C:\folder\file.txt");

            // Assert
            root.Should().Be(@"\\?\C:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_root_for_extended_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string root = fileSystem.Directory.GetDirectoryRoot(@"\\?\UNC\server\share\folder\file.txt");

            // Assert
            root.Should().Be(@"\\?\UNC\server\share");
        }
    }
}
