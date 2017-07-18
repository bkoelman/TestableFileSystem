using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class FakeDirectoryEnumerateFilesSpecs
    {
        [Fact]
        private void When_getting_files_for_null_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.GetFiles(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_getting_files_for_empty_string_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_getting_files_for_whitespace_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_getting_files_for_invalid_root_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_getting_files_for_invalid_characters_in_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(@"c:\dir?i");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.");
        }

        [Fact]
        private void When_getting_files_for_null_pattern_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(@"c:\", null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_getting_files_for_empty_string_pattern_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\file.txt")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\", "");

            // Assert
            files.Should().BeEmpty();
        }

        [Fact]
        private void When_getting_files_for_whitespace_pattern_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\", " ");

            // Assert
            files.Should().BeEmpty();
        }

        [Fact]
        private void When_getting_files_for_pattern_that_starts_with_path_separator_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(@"c:\", @"\some");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_getting_files_for_pattern_that_starts_with_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(@"c:\", @"c:\some");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_getting_files_for_pattern_that_starts_with_extended_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(@"c:\", @"\\?\c:\some");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_getting_files_for_pattern_that_starts_with_network_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(@"c:\", @"\\server\share");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_getting_files_for_pattern_that_contains_empty_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(@"c:\", @"some\\*.*");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_getting_files_for_pattern_that_contains_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(@"c:\folder", @"..\*.*");

            // Assert
            action.ShouldThrow<ArgumentException>()
                .WithMessage(
                    "Search pattern cannot contain '..' to move up directories and can be contained only internally in file/directory names, as in 'a..b'.*");
        }

        [Fact]
        private void When_getting_files_for_pattern_that_contains_parent_directory_with_asterisk_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(@"c:\folder", @"fol*\*.*");

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The filename, directory name, or volume label syntax is incorrect");
        }

        [Fact]
        private void When_getting_files_for_pattern_that_contains_parent_directory_with_question_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.GetFiles(@"c:\folder", @"fol?er\*.*");

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The filename, directory name, or volume label syntax is incorrect");
        }

        [Fact]
        private void When_getting_files_for_pattern_with_asterisk_it_must_match_empty()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .IncludingEmptyFile(@"c:\folder\X")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", @"*f*");

            // Assert
            files.Should().HaveCount(1);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_asterisk_it_must_match_single()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "*");

            // Assert
            files.Should().HaveCount(1);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_asterisk_it_must_match_multiple()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\abc.def")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "*");

            // Assert
            files.Should().HaveCount(1);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_question_it_must_not_match_empty()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", @"?f");

            // Assert
            files.Should().HaveCount(0);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_question_it_must_match_single()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .IncludingEmptyFile(@"c:\folder\Xf")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "?");

            // Assert
            files.Should().HaveCount(1);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_question_it_must_not_match_multiple()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\ab")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "?");

            // Assert
            files.Should().HaveCount(0);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_text_it_must_match_same_text()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "file.txt");

            // Assert
            files.Should().HaveCount(1);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_text_it_must_match_same_text_ignoring_case()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\FILE.txt")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "file.TXT");

            // Assert
            files.Should().HaveCount(1);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_text_it_must_not_match_different_text()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\some")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "other");

            // Assert
            files.Should().HaveCount(0);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_asterisk_for_filename_it_must_match_same_extension()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "*.txt");

            // Assert
            files.Should().HaveCount(1);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_asterisk_for_filename_it_must_not_match_diffrent_extension()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.doc")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "*.txt");

            // Assert
            files.Should().HaveCount(0);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_question_for_extension_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.bak")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "*.ba?");

            // Assert
            files.Should().HaveCount(1);
        }

        [Fact]
        private void When_getting_files_for_pattern_with_path_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\sub\file_a.txt")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", @"sub\file?a.*");

            // Assert
            files.Should().HaveCount(1);
        }

        // TODO: Add specs for recursion.

        // TODO: Add missing specs.
    }
}
