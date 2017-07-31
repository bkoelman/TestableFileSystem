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
    public sealed class DirectoryEnumerateDirectoriesSpecs
    {
        [Fact]
        private void When_enumerating_directories_for_null_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.EnumerateDirectories(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_enumerating_directories_for_empty_string_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_enumerating_directories_for_whitespace_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_enumerating_directories_for_invalid_root_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_enumerating_directories_for_invalid_characters_in_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\dir?i");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_enumerating_directories_for_null_pattern_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\", null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_enumerating_directories_for_empty_string_pattern_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\", "");

            // Assert
            directories.Should().BeEmpty();
        }

        [Fact]
        private void When_enumerating_directories_for_whitespace_pattern_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\", " ");

            // Assert
            directories.Should().BeEmpty();
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_that_starts_with_path_separator_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\", @"\some");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_that_starts_with_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\", @"c:\some");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_that_starts_with_extended_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\", @"\\?\c:\some");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_that_starts_with_network_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\", @"\\server\share");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_that_contains_empty_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\", @"some\\*.*");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Second path fragment must not be a drive or UNC name.*");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_that_contains_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\folder", @"..\*.*");

            // Assert
            action.ShouldThrow<ArgumentException>()
                .WithMessage(
                    "Search pattern cannot contain '..' to move up directories and can be contained only internally in file/directory names, as in 'a..b'.*");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_that_contains_parent_directory_with_asterisk_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\folder", @"fol*\*.*");

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The filename, directory name, or volume label syntax is incorrect");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_that_contains_parent_directory_with_question_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\folder", @"fol?er\*.*");

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The filename, directory name, or volume label syntax is incorrect");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_that_contains_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\folder", @"<|>");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_asterisk_it_must_match_empty()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\f")
                .IncludingDirectory(@"c:\folder\X")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", @"*f*");

            // Assert
            directories.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_asterisk_it_must_match_single_character()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\f")
                .Build();

            // Act
            // ReSharper disable once RedundantArgumentDefaultValue
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", "*");

            // Assert
            directories.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_asterisk_it_must_match_multiple_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\abc.def")
                .Build();

            // Act
            // ReSharper disable once RedundantArgumentDefaultValue
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", "*");

            // Assert
            directories.Should().ContainSingle(x => x == @"c:\folder\abc.def");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_question_it_must_match_empty()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\f")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", @"?f");

            // Assert
            directories.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_question_it_must_match_single_character()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\f")
                .IncludingDirectory(@"c:\folder\Xf")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", "?");

            // Assert
            directories.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_question_it_must_not_match_multiple_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\ab")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", "?");

            // Assert
            directories.Should().BeEmpty();
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_text_it_must_match_same_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\sub")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", "sub");

            // Assert
            directories.Should().ContainSingle(x => x == @"c:\folder\sub");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_text_it_must_match_same_characters_ignoring_case()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\SUB.text")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", "sub.TEXT");

            // Assert
            directories.Should().ContainSingle(x => x == @"c:\folder\SUB.text");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_text_it_must_not_match_different_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\some")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", "other");

            // Assert
            directories.Should().BeEmpty();
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_asterisk_for_directory_name_it_must_match_same_extension()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\sub.txt")
                .IncludingDirectory(@"c:\folder\other.doc")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", "*.txt");

            // Assert
            directories.Should().ContainSingle(x => x == @"c:\folder\sub.txt");
        }

        [Fact]
        private void
            When_enumerating_directories_for_pattern_with_asterisk_for_directory_name_it_must_not_match_different_extension()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\sub.doc")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", "*.txt");

            // Assert
            directories.Should().BeEmpty();
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_question_for_extension_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\files.bak")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\folder", "*.ba?");

            // Assert
            directories.Should().ContainSingle(x => x == @"c:\folder\files.bak");
        }

        [Fact]
        private void When_enumerating_directories_recursively_for_directory_name_pattern_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\base\deep\xxx1")
                .IncludingDirectory(@"C:\base\more\nested\xxx2")
                .IncludingEmptyFile(@"C:\base\more\nested\extra\xxx3\xxxFILE")
                .IncludingDirectory(@"C:\base\xxx4")
                .Build();

            // Act
            IEnumerable<string> directories =
                fileSystem.Directory.EnumerateDirectories(@"c:\base", "xxx*", SearchOption.AllDirectories);

            // Assert
            string[] directoryArray = directories.Should().HaveCount(4).And.Subject.ToArray();
            directoryArray[0].Should().Be(@"c:\base\xxx4");
            directoryArray[1].Should().Be(@"c:\base\deep\xxx1");
            directoryArray[2].Should().Be(@"c:\base\more\nested\xxx2");
            directoryArray[3].Should().Be(@"c:\base\more\nested\extra\xxx3");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_path_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\root")
                .IncludingDirectory(@"C:\base\more")
                .IncludingDirectory(@"C:\base\first\where")
                .IncludingDirectory(@"C:\base\second\miss")
                .IncludingDirectory(@"C:\base\second\other")
                .IncludingDirectory(@"C:\base\second\deeper\skip")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"C:\", @"base\second\o*");

            // Assert
            directories.Should().ContainSingle(x => x == @"C:\base\second\other");
        }

        [Fact]
        private void When_enumerating_directories_recursively_for_pattern_with_path_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\base\deep\xxx1")
                .IncludingDirectory(@"C:\base\more\nested\xxx2")
                .IncludingEmptyFile(@"C:\base\more\nested\extra\xxx3\xxxFILE")
                .IncludingDirectory(@"C:\base\xxx4")
                .Build();

            // Act
            IEnumerable<string> directories =
                fileSystem.Directory.EnumerateDirectories(@"c:\", @"base\xxx*", SearchOption.AllDirectories);

            // Assert
            string[] directoryArray = directories.Should().HaveCount(4).And.Subject.ToArray();
            directoryArray[0].Should().Be(@"c:\base\xxx4");
            directoryArray[1].Should().Be(@"c:\base\deep\xxx1");
            directoryArray[2].Should().Be(@"c:\base\more\nested\xxx2");
            directoryArray[3].Should().Be(@"c:\base\more\nested\extra\xxx3");
        }

        [Fact]
        private void When_enumerating_directories_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\base\zzz.dir")
                .IncludingDirectory(@"C:\base\subfolderZ\sub-zzz.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\n-002.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\deeper\ddd-1.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\deeper\ddd-2.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\aaa\n-aaa.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\n-001.dir")
                .IncludingDirectory(@"C:\base\subfolderA\sub-aaa.dir")
                .IncludingDirectory(@"C:\base\aaa.dir")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\base");

            // Assert
            string[] directoryArray = directories.Should().HaveCount(4).And.Subject.ToArray();
            directoryArray[0].Should().Be(@"c:\base\aaa.dir");
            directoryArray[1].Should().Be(@"c:\base\subfolderA");
            directoryArray[2].Should().Be(@"c:\base\subfolderZ");
            directoryArray[3].Should().Be(@"c:\base\zzz.dir");
        }

        [Fact]
        private void When_enumerating_directories_with_pattern_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\base\zzz.dir")
                .IncludingDirectory(@"C:\base\subfolderZ\sub-zzz.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\n-002.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\deeper\ddd-1.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\deeper\ddd-2.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\aaa\n-aaa.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\n-001.dir")
                .IncludingDirectory(@"C:\base\subfolderA\sub-aaa.dir")
                .IncludingDirectory(@"C:\base\aaa.dir")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"c:\base", "*.dir");

            // Assert
            string[] directoryArray = directories.Should().HaveCount(2).And.Subject.ToArray();
            directoryArray[0].Should().Be(@"c:\base\aaa.dir");
            directoryArray[1].Should().Be(@"c:\base\zzz.dir");
        }

        [Fact]
        private void When_enumerating_directories_recursively_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\base\zzz.dir")
                .IncludingDirectory(@"C:\base\subfolderZ\sub-zzz.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\n-002.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\deeper\ddd-1.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\deeper\ddd-2.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\aaa\n-aaa.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\n-001.dir")
                .IncludingDirectory(@"C:\base\subfolderA\sub-aaa.dir")
                .IncludingDirectory(@"C:\base\aaa.dir")
                .Build();

            // Act
            IEnumerable<string> directories =
                fileSystem.Directory.EnumerateDirectories(@"c:\base", searchOption: SearchOption.AllDirectories);

            // Assert
            string[] directoryArray = directories.Should().HaveCount(14).And.Subject.ToArray();
            directoryArray[0].Should().Be(@"c:\base\aaa.dir");
            directoryArray[1].Should().Be(@"c:\base\subfolderA");
            directoryArray[2].Should().Be(@"c:\base\subfolderZ");
            directoryArray[3].Should().Be(@"c:\base\zzz.dir");
            directoryArray[4].Should().Be(@"c:\base\subfolderA\nested");
            directoryArray[5].Should().Be(@"c:\base\subfolderA\sub-aaa.dir");
            directoryArray[6].Should().Be(@"c:\base\subfolderA\nested\aaa");
            directoryArray[7].Should().Be(@"c:\base\subfolderA\nested\deeper");
            directoryArray[8].Should().Be(@"c:\base\subfolderA\nested\n-001.dir");
            directoryArray[9].Should().Be(@"c:\base\subfolderA\nested\n-002.dir");
            directoryArray[10].Should().Be(@"c:\base\subfolderA\nested\aaa\n-aaa.dir");
            directoryArray[11].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-1.dir");
            directoryArray[12].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-2.dir");
            directoryArray[13].Should().Be(@"c:\base\subfolderZ\sub-zzz.dir");
        }

        [Fact]
        private void When_enumerating_directories_recursively_for_pattern_with_path_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\base\zzz.dir")
                .IncludingDirectory(@"C:\base\subfolderZ\sub-zzz.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\n-002.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\deeper\ddd-1.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\deeper\ddd-2.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\aaa\n-aaa.dir")
                .IncludingDirectory(@"C:\base\subfolderA\nested\n-001.dir")
                .IncludingDirectory(@"C:\base\subfolderA\sub-aaa.dir")
                .IncludingDirectory(@"C:\base\aaa.dir")
                .Build();

            // Act
            IEnumerable<string> directories =
                fileSystem.Directory.EnumerateDirectories(@"c:\", @"base\*.dir", SearchOption.AllDirectories);

            // Assert
            string[] directoryArray = directories.Should().HaveCount(9).And.Subject.ToArray();
            directoryArray[0].Should().Be(@"c:\base\aaa.dir");
            directoryArray[1].Should().Be(@"c:\base\zzz.dir");
            directoryArray[2].Should().Be(@"c:\base\subfolderA\sub-aaa.dir");
            directoryArray[3].Should().Be(@"c:\base\subfolderA\nested\n-001.dir");
            directoryArray[4].Should().Be(@"c:\base\subfolderA\nested\n-002.dir");
            directoryArray[5].Should().Be(@"c:\base\subfolderA\nested\aaa\n-aaa.dir");
            directoryArray[6].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-1.dir");
            directoryArray[7].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-2.dir");
            directoryArray[8].Should().Be(@"c:\base\subfolderZ\sub-zzz.dir");
        }

        [Fact]
        private void When_enumerating_directories_for_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"c:\folder");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'c:\folder'.");
        }

        [Fact]
        private void When_enumerating_directories_for_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"F:\some\FOLDER\sub")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"f:\SOME\folder");

            // Assert
            directories.Should().ContainSingle(x => x == @"f:\SOME\folder\sub");
        }

        [Fact]
        private void When_enumerating_directories_for_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"x:\path\to\sub.ext")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"x:\path\to  ");

            // Assert
            directories.Should().ContainSingle(x => x == @"x:\path\to\sub.ext");
        }

        [Fact]
        private void When_enumerating_directories_for_pattern_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"x:\path\to\sub.ext")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"x:\path\to", "*.ext  ");

            // Assert
            directories.Should().ContainSingle(x => x == @"x:\path\to\sub.ext");
        }

        [Fact]
        private void When_enumerating_directories_for_relative_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some\folder\sub")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories("FOLDER");

            // Assert
            directories.Should().ContainSingle(x => x == @"FOLDER\sub");
        }

        [Fact]
        private void When_enumerating_directories_for_relative_directory_with_parent_reference_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some\folder\sub")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some\folder");

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"..\FOLDER");

            // Assert
            directories.Should().ContainSingle(x => x == @"..\FOLDER\sub");
        }

        [Fact]
        private void When_enumerating_directories_for_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(path);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The directory name is invalid.");
        }

        [Fact]
        private void When_enumerating_directories_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"C:\some\file.txt\deeper");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\file.txt\deeper'.");
        }

        [Fact]
        private void When_enumerating_directories_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"C:\some\file.txt\deeper\more");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\file.txt\deeper\more'.");
        }

        [Fact]
        private void When_enumerating_directories_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"P:\")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"P:\folder\sub");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'P:\folder\sub'.");
        }

        [Fact]
        private void When_enumerating_directories_for_missing_network_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"\\server\share\team");

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The network path was not found");
        }

        [Fact]
        private void When_enumerating_directories_for_missing_remote_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories(@"\\server\share\team");

            // Assert
            //System.IO.IOException: 'The network path was not found'
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path '\\server\share\team'.");
        }

        [Fact]
        private void When_enumerating_directories_for_existing_remote_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share\team\work")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"\\server\share\team");

            // Assert
            directories.Should().ContainSingle(x => x == @"\\server\share\team\work");
        }

        [Fact]
        private void When_enumerating_directories_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateDirectories("COM1");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_enumerating_directories_for_extended_remote_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share\team\work")
                .Build();

            // Act
            IEnumerable<string> directories = fileSystem.Directory.EnumerateDirectories(@"\\?\UNC\server\share\team");

            // Assert
            directories.Should().ContainSingle(x => x == @"\\?\UNC\server\share\team\work");
        }
    }
}
