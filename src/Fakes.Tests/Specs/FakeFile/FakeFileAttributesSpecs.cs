using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FakeFileAttributesSpecs
    {
        [Fact]
        private void When_getting_attributes_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.GetAttributes(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_setting_attributes_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.SetAttributes(null, FileAttributes.Normal);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_getting_attributes_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_setting_attributes_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(string.Empty, FileAttributes.Normal);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_getting_attributes_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_setting_attributes_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(" ", FileAttributes.Normal);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_getting_attributes_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_setting_attributes_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes("::", FileAttributes.Archive);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_getting_atributes_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"c:\dir?i");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.");
        }

        [Fact]
        private void When_setting_atributes_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(@"c:\dir?i", FileAttributes.Archive);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.");
        }

        [Fact]
        private void When_getting_attributes_for_missing_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"C:\some\file.txt");

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
        }

        [Fact]
        private void When_setting_attributes_for_missing_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(@"C:\some\file.txt", FileAttributes.Archive);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
        }

        [Fact]
        private void When_getting_attributes_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"C:\some\file.txt");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\file.txt'.");
        }

        [Fact]
        private void When_setting_attributes_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(@"C:\some\file.txt", FileAttributes.Archive);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\file.txt'.");
        }

        [Fact]
        private void When_setting_attributes_for_existing_file_to_normal_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Normal);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact]
        private void When_setting_attributes_for_existing_file_to_archive_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path + "  ", FileAttributes.Archive);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Archive);
        }

        [Fact]
        private void When_setting_attributes_for_existing_file_in_readonly_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder", FileAttributes.ReadOnly)
                .IncludingEmptyFile(@"C:\folder\some.txt")
                .Build();

            // Act
            fileSystem.File.SetAttributes(@"C:\folder\some.txt", FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(@"C:\folder\some.txt").Should().Be(FileAttributes.Hidden);
        }

        [Fact]
        private void When_setting_attributes_for_existing_file_to_zero_it_must_reset_to_normal()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, 0);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact]
        private void When_setting_attributes_for_existing_file_to_a_discarded_value_it_must_reset_to_normal()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Encrypted);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact]
        private void When_setting_attributes_for_existing_file_to_all_it_must_filter_and_succeed()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path,
                FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory |
                FileAttributes.Archive | FileAttributes.Device | FileAttributes.Normal | FileAttributes.Temporary |
                FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed | FileAttributes.Offline |
                FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.IntegrityStream |
                FileAttributes.NoScrubData);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.ReadOnly | FileAttributes.Hidden |
                FileAttributes.System | FileAttributes.Archive | FileAttributes.Temporary | FileAttributes.Offline |
                FileAttributes.NotContentIndexed | FileAttributes.NoScrubData);
        }

        [Fact]
        private void When_getting_attributes_for_drive_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(path);

            // Assert
            attributes.Should().Be(FileAttributes.Directory | FileAttributes.System | FileAttributes.Hidden);
        }

        [Fact]
        private void When_setting_attributes_for_drive_it_must_preserve_minimum_set()
        {
            // Arrange
            const string path = @"C:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory | FileAttributes.System |
                FileAttributes.Hidden | FileAttributes.ReadOnly);
        }

        [Fact]
        private void When_setting_attributes_for_current_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            fileSystem.File.SetAttributes(@"c:\folder", FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(@"C:\folder").Should().Be(FileAttributes.Hidden | FileAttributes.Directory);
        }

        [Fact]
        private void When_getting_attributes_for_exising_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\folder\some.txt", FileAttributes.Hidden)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(@"some.txt");

            // Assert
            attributes.Should().Be(FileAttributes.Hidden);
        }

        [Fact]
        private void When_setting_attributes_for_exising_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\folder\some.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            fileSystem.File.SetAttributes(@"some.txt", FileAttributes.ReadOnly);

            // Assert
            fileSystem.File.GetAttributes(@"C:\folder\some.txt").Should().Be(FileAttributes.ReadOnly);
        }

        [Fact]
        private void When_getting_file_attributes_for_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\FOLDER\some.TXT", FileAttributes.ReadOnly)
                .Build();

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(@"C:\folder\SOME.txt");

            // Assert
            attributes.Should().Be(FileAttributes.ReadOnly);
        }

        [Fact]
        private void When_setting_file_attributes_for_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\FOLDER\some.TXT", FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.File.SetAttributes(@"C:\folder\SOME.txt", FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(@"C:\FOLDER\some.TXT").Should().Be(FileAttributes.Hidden);
        }

        [Fact]
        private void When_getting_file_attributes_for_existing_remote_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\personal.docx";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(path);

            // Assert
            attributes.Should().Be(FileAttributes.ReadOnly);
        }

        [Fact]
        private void When_setting_file_attributes_for_existing_remote_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\personal.docx";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Hidden);
        }

        [Fact]
        private void When_getting_file_attributes_for_missing_remote_file_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(path);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file '\\server\share\missing.txt'.");
        }

        [Fact]
        private void When_setting_file_attributes_for_missing_remote_file_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing.docx";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(path, FileAttributes.Hidden);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file '\\server\share\missing.docx'.");
        }

        [Fact]
        private void When_getting_file_attributes_for_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(path);

            // Assert
            attributes.Should().Be(FileAttributes.Directory);
        }

        [Fact]
        private void When_setting_file_attributes_for_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Directory);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory);
        }

        [Fact]
        private void When_getting_file_attributes_for_missing_extended_local_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"\\?\C:\some\missing.txt");

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file '\\?\C:\some\missing.txt'.");
        }

        [Fact]
        private void When_setting_file_attributes_for_missing_extended_local_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(@"\\?\C:\some\missing.txt", FileAttributes.ReadOnly);

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file '\\?\C:\some\missing.txt'.");
        }

        [Fact]
        private void When_getting_file_attributes_for_existing_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\other.txt", FileAttributes.ReadOnly)
                .Build();

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(@"\\?\C:\some\other.txt");

            // Assert
            attributes.Should().Be(FileAttributes.ReadOnly);
        }

        [Fact]
        private void When_setting_file_attributes_for_existing_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\other.txt")
                .Build();

            // Act
            fileSystem.File.SetAttributes(@"\\?\C:\some\other.txt", FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(@"\\?\C:\some\other.txt").Should().Be(FileAttributes.Hidden);
        }
    }
}
