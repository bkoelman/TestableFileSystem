using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class FakeDirectoryEnumerateFilesSpecs
    {
        [Fact]
        private void When_enumerating_files_for_null_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.EnumerateFiles(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_enumerating_files_for_empty_string_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_enumerating_files_for_whitespace_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_enumerating_files_for_invalid_root_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_enumerating_files_for_invalid_characters_in_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\dir?i");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.");
        }

        [Fact]
        private void When_enumerating_files_for_null_pattern_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\", null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_enumerating_files_for_empty_string_pattern_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\file.txt")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\", "");

            // Assert
            files.Should().BeEmpty();
        }

        [Fact]
        private void When_enumerating_files_for_whitespace_pattern_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\file.txt")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\", " ");

            // Assert
            files.Should().BeEmpty();
        }

        [Fact]
        private void When_enumerating_files_for_pattern_that_starts_with_path_separator_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\", @"\some");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_that_starts_with_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\", @"c:\some");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_that_starts_with_extended_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\", @"\\?\c:\some");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_that_starts_with_network_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\", @"\\server\share");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_that_contains_empty_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\", @"some\\*.*");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_that_contains_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\folder", @"..\*.*");

            // Assert
            action.ShouldThrow<ArgumentException>()
                .WithMessage(
                    "Search pattern cannot contain '..' to move up directories and can be contained only internally in file/directory names, as in 'a..b'.*");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_that_contains_parent_directory_with_asterisk_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\folder", @"fol*\*.*");

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The filename, directory name, or volume label syntax is incorrect");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_that_contains_parent_directory_with_question_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\folder", @"fol?er\*.*");

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The filename, directory name, or volume label syntax is incorrect");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_that_contains_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\folder", @"<|>");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_asterisk_it_must_match_empty()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .IncludingEmptyFile(@"c:\folder\X")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", @"*f*");

            // Assert
            files.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_asterisk_it_must_match_single_character()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .Build();

            // Act
            // ReSharper disable once RedundantArgumentDefaultValue
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", "*");

            // Assert
            files.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_asterisk_it_must_match_multiple_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\abc.def")
                .Build();

            // Act
            // ReSharper disable once RedundantArgumentDefaultValue
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", "*");

            // Assert
            files.Should().ContainSingle(x => x == @"c:\folder\abc.def");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_question_it_must_match_empty()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", @"?f");

            // Assert
            files.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_question_it_must_match_single_character()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .IncludingEmptyFile(@"c:\folder\Xf")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", "?");

            // Assert
            files.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_question_it_must_not_match_multiple_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\ab")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", "?");

            // Assert
            files.Should().BeEmpty();
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_text_it_must_match_same_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", "file.txt");

            // Assert
            files.Should().ContainSingle(x => x == @"c:\folder\file.txt");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_text_it_must_match_same_characters_ignoring_case()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\FILE.txt")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", "file.TXT");

            // Assert
            files.Should().ContainSingle(x => x == @"c:\folder\FILE.txt");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_text_it_must_not_match_different_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\some")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", "other");

            // Assert
            files.Should().BeEmpty();
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_asterisk_for_filename_it_must_match_same_extension()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .IncludingEmptyFile(@"c:\folder\other.doc")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", "*.txt");

            // Assert
            files.Should().ContainSingle(x => x == @"c:\folder\file.txt");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_asterisk_for_filename_it_must_not_match_different_extension()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.doc")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", "*.txt");

            // Assert
            files.Should().BeEmpty();
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_question_for_extension_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.bak")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\folder", "*.ba?");

            // Assert
            files.Should().ContainSingle(x => x == @"c:\folder\file.bak");
        }

        [Fact]
        private void When_enumerating_files_recursively_for_filename_pattern_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\one.txt")
                .IncludingEmptyFile(@"C:\base\deep\two.txt")
                .IncludingEmptyFile(@"C:\base\more\nested\three.txt")
                .IncludingEmptyFile(@"C:\base\more\nested\four.txt")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\base", "*o*.txt", SearchOption.AllDirectories);

            // Assert
            string[] fileArray = files.Should().HaveCount(3).And.Subject.ToArray();
            fileArray[0].Should().Be(@"c:\base\one.txt");
            fileArray[1].Should().Be(@"c:\base\deep\two.txt");
            fileArray[2].Should().Be(@"c:\base\more\nested\four.txt");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_path_it_must_match()
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
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"C:\", @"base\second\o*.do?");

            // Assert
            files.Should().ContainSingle(x => x == @"C:\base\second\other.doc");
        }

        [Fact]
        private void When_enumerating_files_recursively_for_pattern_with_path_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"e:\some1.txt")
                .IncludingEmptyFile(@"e:\Sub\some2.txt")
                .IncludingEmptyFile(@"e:\Sub\Deeper\some3.txt")
                .IncludingEmptyFile(@"e:\Sub\Deeper\other.txt")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"e:\", @"Sub\s*.txt", SearchOption.AllDirectories);

            // Assert
            string[] fileArray = files.Should().HaveCount(2).And.Subject.ToArray();
            fileArray[0].Should().Be(@"e:\Sub\some2.txt");
            fileArray[1].Should().Be(@"e:\Sub\Deeper\some3.txt");
        }

        [Fact]
        private void When_enumerating_files_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderZ\sub-zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-002.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-1.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-2.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\aaa\n-aaa.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-001.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\sub-aaa.txt")
                .IncludingEmptyFile(@"C:\base\aaa.txt")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\base");

            // Assert
            string[] fileArray = files.Should().HaveCount(2).And.Subject.ToArray();
            fileArray[0].Should().Be(@"c:\base\aaa.txt");
            fileArray[1].Should().Be(@"c:\base\zzz.txt");
        }

        [Fact]
        private void When_enumerating_files_with_pattern_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderZ\sub-zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-002.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-1.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-2.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\aaa\n-aaa.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-001.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\sub-aaa.txt")
                .IncludingEmptyFile(@"C:\base\aaa.txt")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"c:\base", "*.txt");

            // Assert
            string[] fileArray = files.Should().HaveCount(2).And.Subject.ToArray();
            fileArray[0].Should().Be(@"c:\base\aaa.txt");
            fileArray[1].Should().Be(@"c:\base\zzz.txt");
        }

        [Fact]
        private void When_enumerating_files_recursively_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderZ\sub-zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-002.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-1.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-2.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\aaa\n-aaa.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-001.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\sub-aaa.txt")
                .IncludingEmptyFile(@"C:\base\aaa.txt")
                .Build();

            // Act
            IEnumerable<string> files =
                fileSystem.Directory.EnumerateFiles(@"c:\base", searchOption: SearchOption.AllDirectories);

            // Assert
            string[] fileArray = files.Should().HaveCount(9).And.Subject.ToArray();
            fileArray[0].Should().Be(@"c:\base\aaa.txt");
            fileArray[1].Should().Be(@"c:\base\zzz.txt");
            fileArray[2].Should().Be(@"c:\base\subfolderA\sub-aaa.txt");
            fileArray[3].Should().Be(@"c:\base\subfolderA\nested\n-001.txt");
            fileArray[4].Should().Be(@"c:\base\subfolderA\nested\n-002.txt");
            fileArray[5].Should().Be(@"c:\base\subfolderA\nested\aaa\n-aaa.txt");
            fileArray[6].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-1.txt");
            fileArray[7].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-2.txt");
            fileArray[8].Should().Be(@"c:\base\subfolderZ\sub-zzz.txt");
        }

        [Fact]
        private void When_enumerating_files_recursively_for_pattern_with_path_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderZ\sub-zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-002.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-1.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-2.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\aaa\n-aaa.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-001.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\sub-aaa.txt")
                .IncludingEmptyFile(@"C:\base\aaa.txt")
                .Build();

            // Act
            IEnumerable<string> files =
                fileSystem.Directory.EnumerateFiles(@"c:\", @"base\*.txt", SearchOption.AllDirectories);

            // Assert
            string[] fileArray = files.Should().HaveCount(9).And.Subject.ToArray();
            fileArray[0].Should().Be(@"c:\base\aaa.txt");
            fileArray[1].Should().Be(@"c:\base\zzz.txt");
            fileArray[2].Should().Be(@"c:\base\subfolderA\sub-aaa.txt");
            fileArray[3].Should().Be(@"c:\base\subfolderA\nested\n-001.txt");
            fileArray[4].Should().Be(@"c:\base\subfolderA\nested\n-002.txt");
            fileArray[5].Should().Be(@"c:\base\subfolderA\nested\aaa\n-aaa.txt");
            fileArray[6].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-1.txt");
            fileArray[7].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-2.txt");
            fileArray[8].Should().Be(@"c:\base\subfolderZ\sub-zzz.txt");
        }

        [Fact]
        private void When_enumerating_files_for_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"c:\folder");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'c:\folder'.");
        }

        [Fact]
        private void When_enumerating_files_for_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"F:\some\FOLDER\file.TXT")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"f:\SOME\folder");

            // Assert
            files.Should().ContainSingle(x => x == @"f:\SOME\folder\file.TXT");
        }

        [Fact]
        private void When_enumerating_files_for_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"x:\path\to\file.ext")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"x:\path\to  ");

            // Assert
            files.Should().ContainSingle(x => x == @"x:\path\to\file.ext");
        }

        [Fact]
        private void When_enumerating_files_for_pattern_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"x:\path\to\file.ext")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"x:\path\to", "*.ext  ");

            // Assert
            files.Should().ContainSingle(x => x == @"x:\path\to\file.ext");
        }

        [Fact]
        private void When_enumerating_files_for_relative_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\folder\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles("FOLDER");

            // Assert
            files.Should().ContainSingle(x => x == @"FOLDER\file.txt");
        }

        [Fact]
        private void When_enumerating_files_for_relative_directory_with_parent_reference_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\folder\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some\folder");

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"..\FOLDER");

            // Assert
            files.Should().ContainSingle(x => x == @"..\FOLDER\file.txt");
        }

        [Fact]
        private void When_enumerating_files_for_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(path);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The directory name is invalid.");
        }

        [Fact]
        private void When_enumerating_files_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"C:\some\file.txt\deeper");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\file.txt\deeper'.");
        }

        [Fact]
        private void When_enumerating_files_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"P:\")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"P:\folder\sub");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'P:\folder\sub'.");
        }

        [Fact]
        private void When_enumerating_files_for_missing_network_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"\\server\share\team");

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The network path was not found");
        }

        [Fact]
        private void When_enumerating_files_for_missing_remote_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles(@"\\server\share\team");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path '\\server\share\team'.");
        }

        [Fact]
        private void When_enumerating_files_for_existing_remote_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"\\server\share\team\work.doc")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"\\server\share\team");

            // Assert
            files.Should().ContainSingle(x => x == @"\\server\share\team\work.doc");
        }

        [Fact]
        private void When_enumerating_files_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFiles("COM1");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_enumerating_files_for_extended_remote_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"\\server\share\team\work.doc")
                .Build();

            // Act
            IEnumerable<string> files = fileSystem.Directory.EnumerateFiles(@"\\?\UNC\server\share\team");

            // Assert
            files.Should().ContainSingle(x => x == @"\\?\UNC\server\share\team\work.doc");
        }
    }
}
