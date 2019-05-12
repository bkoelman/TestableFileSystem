using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileMoveSpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_moving_file_for_null_source_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.File.Move(null, @"c:\destination.txt");

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_moving_file_for_null_destination_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.File.Move(@"c:\source.txt", null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_moving_file_for_empty_string_source_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Move(string.Empty, @"c:\destination.txt");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("Empty file name is not legal.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_moving_file_for_empty_string_destination_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Move(@"c:\source.txt", string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("Empty file name is not legal.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_moving_file_for_whitespace_source_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Move(" ", @"c:\destination.txt");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_moving_file_for_whitespace_destination_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Move(@"c:\source.txt", " ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_for_invalid_drive_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move("_:", @"c:\destination.txt");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_for_invalid_drive_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"c:\source.txt", "_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_for_wildcard_characters_in_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move("some?.txt", @"c:\destination.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_for_wildcard_characters_in_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"c:\source.txt", "some?.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_same_location_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.Move(path, path);

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_same_location_with_different_casing_it_must_rename()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\FILE.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            string[] files = fileSystem.Directory.GetFiles(@"C:\some");
            files.Should().ContainSingle(x => x == destinationPath);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_different_name_in_same_directory_it_must_rename()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\old.txt")
                .Build();

            // Act
            fileSystem.File.Move(@"C:\some\old.txt  ", @"C:\some\new.txt  ");

            // Assert
            fileSystem.File.Exists(@"C:\some\old.txt").Should().BeFalse();
            fileSystem.File.Exists(@"C:\some\new.txt").Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingEmptyFile(@"C:\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.Move(@"\file.txt", @"\copied.txt");

            // Assert
            fileSystem.File.Exists(@"C:\file.txt").Should().BeFalse();
            fileSystem.File.Exists(@"C:\copied.txt").Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_from_relative_path_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"D:\other\folder\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"D:\other\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            fileSystem.File.Move(@"some\file.txt", destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_relative_path_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"D:\other\folder\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"D:\other\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            fileSystem.File.Move(sourcePath, @"folder\newname.doc");

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_relative_file_on_different_drive_in_root_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\")
                .IncludingDirectory(@"D:\other")
                .IncludingEmptyFile(@"D:\source.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            fileSystem.File.Move("D:source.txt", "D:destination.txt");

            // Assert
            fileSystem.File.Exists(@"D:\destination.txt").Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_relative_file_on_same_drive_in_subdirectory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"D:\other\source.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            fileSystem.File.Move("D:source.txt", "D:destination.txt");

            // Assert
            fileSystem.File.Exists(@"d:\other\destination.txt").Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_from_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, "newname.doc");

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\subfolder'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationDirectory = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(destinationDirectory)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationDirectory);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("Cannot create a file when that file already exists");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_from_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt\other.txt", "newname.doc");

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>()
                .WithMessage(@"Could not find file 'C:\some\file.txt\other.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .IncludingEmptyFile(@"C:\some\newname.doc")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt", @"C:\some\newname.doc\other.doc");

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The parameter is incorrect");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_from_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt\other.txt\missing.txt", "newname.doc");

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(
                @"Could not find file 'C:\some\file.txt\other.txt\missing.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .IncludingEmptyFile(@"C:\some\newname.doc")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt", @"C:\some\newname.doc\other.doc\more.doc");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage("Could not find a part of the path.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_existing_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingEmptyFile(destinationPath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("Cannot create a file when that file already exists");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_from_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt", @"C:\some\newname.doc");

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_missing_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\other\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_missing_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\some\file.txt", "newname.doc");

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_parent_directory_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\level\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.Move(sourcePath, "renamed.nfo");

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(@"C:\some\renamed.nfo").Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_subdirectory_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\sub\newname.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"c:\some\sub")
                .Build();

            fileSystem.File.WriteAllText(sourcePath, "ABC");

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_different_drive_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"D:\other\folder\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"D:\other\folder")
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_that_is_root_of_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"d:\")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"C:\", @"d:\newname.doc");

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'C:\'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_root_of_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                $"The filename, directory name, or volume label syntax is incorrect. : '{destinationPath}'");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_readonly_file_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"D:\other\folder\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath, FileAttributes.ReadOnly)
                .IncludingDirectory(@"D:\other\folder")
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_an_open_file_to_same_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            using (fileSystem.File.Open(sourcePath, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage(
                    "The process cannot access the file because it is being used by another process.");

                fileSystem.File.Exists(sourcePath).Should().BeTrue();
                fileSystem.File.Exists(destinationPath).Should().BeFalse();
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_an_open_file_to_different_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"X:\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"X:\")
                .Build();

            using (fileSystem.File.Open(sourcePath, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage(
                    "The process cannot access the file because it is being used by another process.");

                fileSystem.File.Exists(sourcePath).Should().BeTrue();
                fileSystem.File.Exists(destinationPath).Should().BeFalse();
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_from_missing_network_share_it_must_fail()
        {
            // Arrange
            string sourcePath = PathFactory.NetworkFileAtDepth(1);
            const string destinationPath = @"C:\docs\mine.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\docs")
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(
                $"Could not find file '{sourcePath}'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_missing_network_share_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\docs\mine.txt";
            string destinationPath = PathFactory.NetworkFileAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{destinationPath}'");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_from_existing_network_share_it_must_succeed()
        {
            // Arrange
            string sourcePath = PathFactory.NetworkFileAtDepth(1);
            const string destinationPath = @"C:\docs\mine.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"C:\docs")
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\docs\mine.txt";
            string destinationPath = PathFactory.NetworkFileAtDepth(1);
            string destinationParentPath = PathFactory.NetworkShare();

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(destinationParentPath)
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_from_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move("com1", @"c:\new.txt");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(@"c:\old.txt", "com1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_from_extended_path_to_extended_path_it_must_succeed()
        {
            // Arrange
            string sourcePath = PathFactory.NetworkFileAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"c:\work")
                .Build();

            // Act
            fileSystem.File.Move(PathFactory.NetworkFileAtDepth(1, true), @"\\?\c:\work\summary.doc");

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeFalse();
            fileSystem.File.Exists(@"c:\work\summary.doc").Should().BeTrue();
        }
    }
}
