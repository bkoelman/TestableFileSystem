using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeBuilder
{
    public sealed class FileSystemBuilderIncludingDirectorySpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_including_directory_for_null_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => builder.IncludingDirectory(null);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_directory_for_empty_string_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingDirectory(string.Empty);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("'path' cannot be empty or contain only whitespace.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_directory_for_whitespace_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingDirectory(" ");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("'path' cannot be empty or contain only whitespace.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_directory_for_invalid_drive_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingDirectory("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_directory_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingDirectory("some?.txt");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_drive_it_must_succeed()
        {
            // Arrange
            const string drive = "F:";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingDirectory(drive)
                .Build();

            // Assert
            fileSystem.Directory.Exists(drive).Should().BeTrue();
            fileSystem.Directory.Exists(@"F:\").Should().BeTrue();

            fileSystem.File.GetAttributes(drive).Should().Be(FileAttributes.Directory | FileAttributes.Hidden |
                FileAttributes.System);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_above_root_of_drive_it_must_succeed()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingDirectory(@"C:\..\..\store")
                .Build();

            // Assert
            fileSystem.Directory.Exists(@"C:\store").Should().BeTrue();
            fileSystem.File.GetAttributes(@"C:\store").Should().Be(FileAttributes.Directory);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingDirectory(path)
                .Build();

            // Assert
            fileSystem.Directory.Exists(path).Should().BeTrue();
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_local_directory_with_attributes_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingDirectory(path, FileAttributes.Hidden)
                .Build();

            // Assert
            fileSystem.Directory.Exists(path).Should().BeTrue();
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory | FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_existing_local_directory_it_must_overwrite()
        {
            // Arrange
            const string path = @"d:\path\to\folder";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingDirectory(path);

            // Act
            IFileSystem fileSystem = builder
                .IncludingDirectory(path, FileAttributes.Hidden)
                .Build();

            // Assert
            fileSystem.Directory.Exists(path).Should().BeTrue();
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory | FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_existing_local_directory_with_different_casing_it_must_overwrite()
        {
            // Arrange
            const string path = @"C:\some\FOLDER";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingDirectory(path);

            // Act
            IFileSystem fileSystem = builder
                .IncludingDirectory(@"c:\SOME\folder", FileAttributes.ReadOnly)
                .Build();

            // Assert
            fileSystem.Directory.Exists(path).Should().BeTrue();
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory | FileAttributes.ReadOnly);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_local_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingDirectory(@"d:\path\to\folder\readme.txt  ")
                .Build();

            // Assert
            fileSystem.Directory.Exists(path).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_relative_local_directory_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingDirectory(@"some\folder");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_local_directory_for_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path);

            // Act
            Action action = () => builder.IncludingDirectory(path);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                @"Cannot create 'C:\some\file.txt' because a file or directory with the same name already exists.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_local_directory_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt");

            // Act
            Action action = () => builder.IncludingDirectory(@"c:\some\file.txt\subfolder");

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                @"Cannot create 'C:\some\file.txt' because a file or directory with the same name already exists.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_local_directory_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt");

            // Act
            Action action = () => builder.IncludingDirectory(@"c:\some\file.txt\subfolder\deeper");

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                @"Cannot create 'C:\some\file.txt' because a file or directory with the same name already exists.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_network_host_without_share_it_must_fail()
        {
            // Arrange
            string path = PathFactory.NetworkHostWithoutShare();

            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => { builder.IncludingDirectory(path); };

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(@"The UNC path should be of the form \\server\share.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_network_share_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkShare();

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingDirectory(path)
                .Build();

            // Assert
            fileSystem.Directory.Exists(path).Should().BeTrue();
            fileSystem.Directory.Exists(path + @"\").Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_remote_directory_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(2);

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingDirectory(path)
                .Build();

            // Assert
            fileSystem.Directory.Exists(path).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_existing_remote_directory_it_must_overwrite()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingDirectory(path);

            // Act
            IFileSystem fileSystem = builder
                .IncludingDirectory(path, FileAttributes.Archive)
                .Build();

            // Assert
            fileSystem.Directory.Exists(path).Should().BeTrue();
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory | FileAttributes.Archive);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_directory_for_reserved_name_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingDirectory(@"c:\some\COM1\folder");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_including_extended_local_directory_it_must_succeed()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingDirectory(@"\\?\d:\path\to\folder")
                .Build();

            // Assert
            fileSystem.Directory.Exists(@"D:\path\to\folder").Should().BeTrue();
            fileSystem.File.GetAttributes(@"D:\path\to\folder").Should().Be(FileAttributes.Directory);
        }
    }
}
