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
            files.Should().ContainSingle(@"c:\folder\f");
        }

        [Fact]
        private void When_getting_files_for_pattern_with_asterisk_it_must_match_single_character()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "*");

            // Assert
            files.Should().ContainSingle(@"c:\folder\f");
        }

        [Fact]
        private void When_getting_files_for_pattern_with_asterisk_it_must_match_multiple_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\abc.def")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "*");

            // Assert
            files.Should().ContainSingle(@"c:\folder\abc.def");
        }

        [Fact]
        private void When_getting_files_for_pattern_with_question_it_must_match_empty()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", @"?f");

            // Assert
            files.Should().ContainSingle(@"c:\folder\f");
        }

        [Fact]
        private void When_getting_files_for_pattern_with_question_it_must_match_single_character()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .IncludingEmptyFile(@"c:\folder\Xf")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "?");

            // Assert
            files.Should().ContainSingle(@"c:\folder\f");
        }

        [Fact]
        private void When_getting_files_for_pattern_with_question_it_must_not_match_multiple_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\ab")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "?");

            // Assert
            files.Should().BeEmpty();
        }

        [Fact]
        private void When_getting_files_for_pattern_with_text_it_must_match_same_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "file.txt");

            // Assert
            files.Should().ContainSingle(@"c:\folder\file.txt");
        }

        [Fact]
        private void When_getting_files_for_pattern_with_text_it_must_match_same_characters_ignoring_case()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\FILE.txt")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "file.TXT");

            // Assert
            files.Should().ContainSingle(@"c:\folder\FILE.txt");
        }

        [Fact]
        private void When_getting_files_for_pattern_with_text_it_must_not_match_different_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\some")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "other");

            // Assert
            files.Should().BeEmpty();
        }

        [Fact]
        private void When_getting_files_for_pattern_with_asterisk_for_filename_it_must_match_same_extension()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .IncludingEmptyFile(@"C:\folder\other.doc")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "*.txt");

            // Assert
            files.Should().ContainSingle(@"c:\folder\file.txt");
        }

        [Fact]
        private void When_getting_files_for_pattern_with_asterisk_for_filename_it_must_not_match_different_extension()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.doc")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\folder", "*.txt");

            // Assert
            files.Should().BeEmpty();
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
            files.Should().ContainSingle(@"c:\folder\file.bak");
        }

        [Fact]
        private void When_getting_files_recursively_for_filename_pattern_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\one.txt")
                .IncludingEmptyFile(@"C:\base\deep\two.txt")
                .IncludingEmptyFile(@"C:\base\more\nested\three.txt")
                .IncludingEmptyFile(@"C:\base\more\nested\four.txt")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\base", "*o*.txt", SearchOption.AllDirectories);

            // Assert
            files.Should().HaveCount(3);
            files[0].Should().Be(@"C:\base\one.txt");
            files[1].Should().Be(@"C:\base\deep\two.txt");
            files[2].Should().Be(@"C:\base\more\nested\four.txt");
        }

        [Fact]
        private void When_getting_files_for_pattern_with_path_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\root.doc")
                .IncludingEmptyFile(@"C:\base\more.doc")
                .IncludingEmptyFile(@"C:\base\first\where.txt")
                .IncludingEmptyFile(@"C:\base\second\other.doc")
                .IncludingEmptyFile(@"C:\base\second\other.txt")
                .IncludingEmptyFile(@"C:\base\second\deeper\skip.doc")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"C:\", @"base\second\o*.do?");

            // Assert
            files.Should().ContainSingle(@"C:\base\second\other.doc");
        }

        [Fact]
        private void When_getting_files_recursively_for_pattern_with_path_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"e:\some1.txt")
                .IncludingEmptyFile(@"e:\Sub\some2.txt")
                .IncludingEmptyFile(@"e:\Sub\Deeper\some3.txt")
                .IncludingEmptyFile(@"e:\Sub\Deeper\other.txt")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"e:\", @"Sub\s*.txt", SearchOption.AllDirectories);

            // Assert
            files.Should().HaveCount(2);
            files[0].Should().Be(@"e:\Sub\some2.txt");
            files[1].Should().Be(@"e:\Sub\Deeper\some3.txt");
        }

        [Fact]
        private void When_getting_files_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"d:\folder\file001.txt")
                .IncludingEmptyFile(@"d:\folder\file003.txt")
                .IncludingEmptyFile(@"d:\folder\file002.txt")
                .IncludingEmptyFile(@"d:\folder\a..b.txt")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"d:\folder");

            // Assert
            files.Should().HaveCount(4);
            files[0].Should().Be(@"d:\folder\file001.txt");
            files[1].Should().Be(@"d:\folder\file003.txt");
            files[2].Should().Be(@"d:\folder\file002.txt");
            files[3].Should().Be(@"d:\folder\a..b.txt");
        }

        [Fact]
        private void When_getting_files_recursively_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\where.txt")
                .IncludingEmptyFile(@"C:\base\more\nested\file.txt")
                .IncludingEmptyFile(@"C:\base\more\nested\deepest\some.txt")
                .IncludingEmptyFile(@"C:\base\other.txt")
                .Build();

            // Act
            string[] files = fileSystem.Directory.GetFiles(@"c:\base", searchOption: SearchOption.AllDirectories);

            // Assert
            files.Should().HaveCount(4);
            files[0].Should().Be(@"C:\base\where.txt");
            files[1].Should().Be(@"C:\base\other.txt");
            files[2].Should().Be(@"C:\base\more\nested\file.txt");
            files[3].Should().Be(@"C:\base\more\nested\deepest\some.txt");
        }

        // TODO: Add missing specs.
    }
}
