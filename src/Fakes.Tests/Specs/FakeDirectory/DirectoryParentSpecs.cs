using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryParentSpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_directory_parent_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.Directory.GetParent(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_directory_parent_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetParent(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("Path cannot be the empty string or all whitespace.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_directory_parent_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.GetParent(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetParent("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetParent(@"c:\dir?i");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_missing_local_drive_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"e:\");

            // Assert
            parent.Should().BeNull();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_missing_local_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"d:\some\folder\path");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"d:\some\folder");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_missing_local_path_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"f:\some\folder\file.txt  ");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"f:\some\folder");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_missing_local_path_with_trailing_separator_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"f:\some\folder\sub\");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.Name.Should().Be("folder");
            parentNotNull.FullName.Should().Be(@"f:\some\folder");
            parentNotNull.ToString().Should().Be(@"f:\some\folder");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_directory_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"C:\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\some");

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\folder");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"c:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_relative_local_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"X:\folder\to\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"x:\folder\to");

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent("file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"x:\folder\to");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(path);

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"C:\some");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"C:\some\file.txt\deeper");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"C:\some\file.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_parent_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"C:\some\file.txt\deeper\more");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"C:\some\file.txt\deeper");
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_directory_parent_for_missing_network_share_it_must_succeed(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(@"\\ServerName\ShareName", false);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                IDirectoryInfo parent = fileSystem.Directory.GetParent(path);

                // Assert
                parent.Should().BeNull();
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_directory_below_missing_network_share_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\\ServerName\ShareName\file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"\\ServerName\ShareName");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_missing_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\\server\share\file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"\\server\share");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_existing_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\\server\share\file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"\\server\share");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetParent("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\folder")
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\\?\C:\folder\file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"\\?\C:\folder");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_directory_parent_for_extended_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo parent = fileSystem.Directory.GetParent(@"\\?\UNC\server\share\folder\file.txt");

            // Assert
            IDirectoryInfo parentNotNull = parent.ShouldNotBeNull();
            parentNotNull.FullName.Should().Be(@"\\?\UNC\server\share\folder");
        }
    }
}
