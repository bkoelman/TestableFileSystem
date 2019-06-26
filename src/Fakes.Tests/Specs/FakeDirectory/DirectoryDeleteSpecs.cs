using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryDeleteSpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_deleting_directory_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.Directory.Delete(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_deleting_directory_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.Delete(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_deleting_directory_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.Delete(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_directory_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_directory_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"c:\dir?i");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_missing_local_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\some\folder");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\folder'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_empty_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.Delete(path);

            // Assert
            fileSystem.Directory.Exists(path).Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_empty_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"C:\some\folder ");

            // Assert
            fileSystem.Directory.Exists(@"C:\some\folder").Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_nonempty_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\folder\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\some\folder");

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The directory is not empty.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_readonly_directory_it_must_fail()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(path);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(@"Access to the path 'c:\some\folder' is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_nonempty_directory_recursively_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder\with\children")
                .IncludingEmptyFile(@"C:\some\folder\file.txt")
                .IncludingEmptyFile(@"C:\some\folder\child\other.txt")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"C:\some\folder", true);

            // Assert
            fileSystem.Directory.Exists(@"C:\some\folder").Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void
            When_deleting_local_nonempty_directory_recursively_that_contains_open_files_it_must_delete_all_others_and_it_fail_on_first_open_file()
        {
            // Arrange
            const string subdirectory = @"C:\some\folder\deeper";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(subdirectory, "fileA.txt"))
                .IncludingEmptyFile(Path.Combine(subdirectory, "fileB.txt"))
                .IncludingEmptyFile(Path.Combine(subdirectory, "fileC.txt"))
                .IncludingEmptyFile(Path.Combine(subdirectory, "fileD.txt"))
                .Build();

            using (fileSystem.File.OpenRead(Path.Combine(subdirectory, "fileB.txt")))
            {
                using (fileSystem.File.OpenRead(Path.Combine(subdirectory, "fileC.txt")))
                {
                    // Act
                    Action action = () => fileSystem.Directory.Delete(@"C:\some\folder", true);

                    // Assert
                    action.Should().ThrowExactly<IOException>().WithMessage(
                        @"The process cannot access the file 'C:\some\folder\deeper\fileB.txt' because it is being used by another process.");

                    fileSystem.File.Exists(Path.Combine(subdirectory, "fileA.txt")).Should().BeFalse();
                    fileSystem.File.Exists(Path.Combine(subdirectory, "fileB.txt")).Should().BeTrue();
                    fileSystem.File.Exists(Path.Combine(subdirectory, "fileC.txt")).Should().BeTrue();
                    fileSystem.File.Exists(Path.Combine(subdirectory, "fileD.txt")).Should().BeFalse();
                    fileSystem.Directory.Exists(subdirectory).Should().BeTrue();
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void
            When_deleting_local_nonempty_directory_recursively_that_contains_readonly_files_it_must_delete_all_others_and_fail_on_first_readonly_file()
        {
            // Arrange
            const string subdirectory = @"C:\some\folder\deeper";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(Path.Combine(subdirectory, "fileA.txt"))
                .IncludingEmptyFile(Path.Combine(subdirectory, "fileB.txt"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(subdirectory, "fileC.txt"), FileAttributes.ReadOnly)
                .IncludingEmptyFile(Path.Combine(subdirectory, "fileD.txt"))
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\some\folder", true);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage(@"Access to the path 'fileB.txt' is denied.");

            fileSystem.File.Exists(Path.Combine(subdirectory, "fileA.txt")).Should().BeFalse();
            fileSystem.File.Exists(Path.Combine(subdirectory, "fileB.txt")).Should().BeTrue();
            fileSystem.File.Exists(Path.Combine(subdirectory, "fileC.txt")).Should().BeTrue();
            fileSystem.File.Exists(Path.Combine(subdirectory, "fileD.txt")).Should().BeFalse();
            fileSystem.Directory.Exists(subdirectory).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void
            When_deleting_local_nonempty_directory_recursively_that_contains_readonly_subdirectories_it_must_delete_all_others_and_fail_on_first_readonly_subdirectory()
        {
            // Arrange
            const string subdirectory = @"C:\some\folder\deeper";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(Path.Combine(subdirectory, "subfolderA"))
                .IncludingDirectory(Path.Combine(subdirectory, "subfolderB"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(subdirectory, "subfolderC"), FileAttributes.ReadOnly)
                .IncludingDirectory(Path.Combine(subdirectory, "subfolderD"))
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\some\folder", true);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                @"Access to the path 'C:\some\folder\deeper\subfolderB' is denied.");

            fileSystem.Directory.Exists(Path.Combine(subdirectory, "subfolderA")).Should().BeFalse();
            fileSystem.Directory.Exists(Path.Combine(subdirectory, "subfolderB")).Should().BeTrue();
            fileSystem.Directory.Exists(Path.Combine(subdirectory, "subfolderC")).Should().BeTrue();
            fileSystem.Directory.Exists(Path.Combine(subdirectory, "subfolderD")).Should().BeFalse();
            fileSystem.Directory.Exists(subdirectory).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\")
                .IncludingDirectory(@"D:\")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            Action action = () => fileSystem.Directory.Delete(@"D:\");

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The directory is not empty.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_drive_recursively_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\")
                .IncludingDirectory(@"D:\")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            Action action = () => fileSystem.Directory.Delete(@"D:\", true);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'd:\'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_below_current_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\store\current\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\store");

            // Act
            fileSystem.Directory.Delete(@"C:\store\current", true);

            // Assert
            fileSystem.Directory.Exists(@"C:\store\current").Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_current_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\store";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(path);

            // Act
            Action action = () => fileSystem.Directory.Delete(path, true);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                @"The process cannot access the file 'C:\store' because it is being used by another process.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_remote_current_directory_it_must_fail()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(path);

            // Act
            Action action = () => fileSystem.Directory.Delete(path, true);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                $"The process cannot access the file '{path}' because it is being used by another process.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_above_current_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\store\current\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\store\current\folder");

            // Act
            Action action = () => fileSystem.Directory.Delete(@"C:\store", true);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                @"The process cannot access the file 'C:\store' because it is being used by another process.");
        }

        [Fact]
        [CanNotRunOnFileSystem(FileSystemSkipReason.DependsOnCurrentDirectory)]
        private void When_deleting_above_remote_current_directory_it_must_fail()
        {
            // Arrange
            string parentPath = PathFactory.NetworkDirectoryAtDepth(1);
            string path = PathFactory.NetworkDirectoryAtDepth(2);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(path);

            // Act
            Action action = () => fileSystem.Directory.Delete(parentPath, true);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                $"The process cannot access the file '{parentPath}' because it is being used by another process.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_above_root_of_drive_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\store")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"C:\..\..\store");

            // Assert
            fileSystem.Directory.Exists(@"C:\store").Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_directory_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingDirectory(@"c:\other")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.Directory.Delete(@"\other");

            // Assert
            fileSystem.Directory.Exists(@"C:\other").Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_relative_empty_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\store\data")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\store");

            // Act
            fileSystem.Directory.Delete(@"data");

            // Assert
            fileSystem.Directory.Exists(@"c:\store\data").Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_empty_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\store\DATA")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"c:\STORE\data");

            // Assert
            fileSystem.Directory.Exists(@"c:\store\DATA").Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_directory_for_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(path);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(@"The directory name is invalid.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_directory_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"c:\some\file.txt\subfolder");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\subfolder'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_local_directory_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.Delete(@"c:\some\file.txt\subfolder\deeper");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\subfolder\deeper'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_remote_empty_directory_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.Directory.Delete(path);

            // Assert
            fileSystem.Directory.Exists(path).Should().BeFalse();
        }

        [Theory]
        [CanRunOnFileSystem(FileSystemRunConditions.RequiresAdministrativeRights)]
        private void When_deleting_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkShare());

                IFileSystem fileSystem = factory.Create()
                    .IncludingDirectory(path)
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.Delete(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage(
                    $"The process cannot access the file '{path}' because it is being used by another process.");
            }
        }

        [Theory]
        [CanRunOnFileSystem(FileSystemRunConditions.RequiresAdministrativeRights)]
        private void When_deleting_network_share_recursively_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkShare());

                IFileSystem fileSystem = factory.Create()
                    .IncludingDirectory(path)
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.Delete(path, true);

                // Assert
                action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                    $"Could not find a part of the path '{path}'.");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_deleting_directory_below_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkDirectoryAtDepth(1), false);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.Delete(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_extended_local_empty_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"\\?\C:\some\folder");

            // Assert
            fileSystem.Directory.Exists(@"C:\some\folder").Should().BeFalse();
        }
    }
}
