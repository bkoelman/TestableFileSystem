using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryEnumerateEntriesSpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_enumerating_entries_for_null_path_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_enumerating_entries_for_empty_string_path_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_enumerating_entries_for_whitespace_path_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_invalid_drive_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_wildcard_characters_in_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"c:\dir?i");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_enumerating_entries_for_null_pattern_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"c:\", null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_empty_string_pattern_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\file.txt")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\", "");

            // Assert
            entries.Should().BeEmpty();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_whitespace_pattern_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\file.txt")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\", " ");

            // Assert
            entries.Should().BeEmpty();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_that_starts_with_path_separator_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"c:\", @"\some");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                "Second path fragment must not be a drive or UNC name.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_that_starts_with_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"c:\", @"c:\some");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                "Second path fragment must not be a drive or UNC name.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_that_starts_with_extended_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"c:\", @"\\?\c:\some");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                "Second path fragment must not be a drive or UNC name.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_that_starts_with_network_path_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"c:\", PathFactory.NetworkShare());

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                "Second path fragment must not be a drive or UNC name.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_that_contains_empty_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"c:\", @"some\\*.*");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                "Second path fragment must not be a drive or UNC name.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_that_contains_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", @"..\*.*");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                "Search pattern cannot contain '..' to move up directories and can be contained only internally in file/directory names, as in 'a..b'.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_that_contains_parent_directory_with_asterisk_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder";
            const string pattern = @"fol*\*.*";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(path, pattern);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                $"The filename, directory name, or volume label syntax is incorrect. : '{pattern}'");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_that_contains_parent_directory_with_question_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder";
            const string pattern = @"fol?er\*.*";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(path, pattern);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(
                $"The filename, directory name, or volume label syntax is incorrect. : '{pattern}'");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_that_contains_forbidden_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", @"<|>");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_asterisk_it_must_match_empty()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .IncludingEmptyFile(@"c:\folder\X")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", @"*f*");

            // Assert
            entries.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_asterisk_it_must_match_single_character()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\f")
                .Build();

            // Act
            // ReSharper disable once RedundantArgumentDefaultValue
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", "*");

            // Assert
            entries.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_asterisk_it_must_match_multiple_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\abc.def")
                .IncludingDirectory(@"c:\folder\sub")
                .Build();

            // Act
            // ReSharper disable once RedundantArgumentDefaultValue
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", "*");

            // Assert
            string[] entryArray = entries.Should().HaveCount(2).And.Subject.ToArray();
            entryArray[0].Should().Be(@"c:\folder\abc.def");
            entryArray[1].Should().Be(@"c:\folder\sub");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_question_it_must_match_empty()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder\f")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", @"?f");

            // Assert
            entries.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_question_it_must_match_single_character()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\f")
                .IncludingEmptyFile(@"c:\folder\Xf")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", "?");

            // Assert
            entries.Should().ContainSingle(x => x == @"c:\folder\f");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_question_it_must_not_match_multiple_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\zz")
                .IncludingDirectory(@"c:\folder\ab")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", "?");

            // Assert
            entries.Should().BeEmpty();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_text_it_must_match_same_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", "file.txt");

            // Assert
            entries.Should().ContainSingle(x => x == @"c:\folder\file.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_text_it_must_match_same_characters_ignoring_case()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\FILE.txt")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", "file.TXT");

            // Assert
            entries.Should().ContainSingle(x => x == @"c:\folder\FILE.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_text_it_must_not_match_different_characters()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\some")
                .IncludingDirectory(@"c:\folder\this")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", "other");

            // Assert
            entries.Should().BeEmpty();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_asterisk_for_name_it_must_match_same_extension()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .IncludingDirectory(@"c:\folder\sub.txt")
                .IncludingEmptyFile(@"c:\folder\other.doc")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", "*.txt");

            // Assert
            string[] entryArray = entries.Should().HaveCount(2).And.Subject.ToArray();
            entryArray[0].Should().Be(@"c:\folder\file.txt");
            entryArray[1].Should().Be(@"c:\folder\sub.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_asterisk_for_name_it_must_not_match_different_extension()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.doc")
                .IncludingDirectory(@"c:\folder\sub.doc")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", "*.txt");

            // Assert
            entries.Should().BeEmpty();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_question_for_extension_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\folder\file.bak")
                .IncludingDirectory(@"c:\folder\sub.bas")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder", "*.ba?");

            // Assert
            string[] entryArray = entries.Should().HaveCount(2).And.Subject.ToArray();
            entryArray[0].Should().Be(@"c:\folder\file.bak");
            entryArray[1].Should().Be(@"c:\folder\sub.bas");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_recursively_for_name_pattern_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\one.txt")
                .IncludingDirectory(@"C:\base\deep\two.txt")
                .IncludingEmptyFile(@"C:\base\more\nested\three.txt")
                .IncludingEmptyFile(@"C:\base\more\nested\four.txt")
                .Build();

            // Act
            IEnumerable<string> entries =
                fileSystem.Directory.EnumerateFileSystemEntries(@"c:\base", "*o*.txt", SearchOption.AllDirectories);

            // Assert
            string[] entryArray = entries.Should().HaveCount(3).And.Subject.ToArray();
            entryArray[0].Should().Be(@"c:\base\one.txt");
            entryArray[1].Should().Be(@"c:\base\deep\two.txt");
            entryArray[2].Should().Be(@"c:\base\more\nested\four.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_path_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\root.doc")
                .IncludingEmptyFile(@"C:\base\more.doc")
                .IncludingEmptyFile(@"C:\base\first\where.txt")
                .IncludingDirectory(@"C:\base\second\o.doc")
                .IncludingEmptyFile(@"C:\base\second\other.doc")
                .IncludingEmptyFile(@"C:\base\second\other.txt")
                .IncludingEmptyFile(@"C:\base\second\deeper\skip.doc")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"C:\", @"base\second\o*.do?");

            // Assert
            string[] entryArray = entries.Should().HaveCount(2).And.Subject.ToArray();
            entryArray[0].Should().Be(@"C:\base\second\o.doc");
            entryArray[1].Should().Be(@"C:\base\second\other.doc");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_recursively_for_pattern_with_path_it_must_match()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"e:\some1.txt")
                .IncludingEmptyFile(@"e:\Sub\some2.txt")
                .IncludingDirectory(@"e:\Sub\Deeper\some3.txt")
                .IncludingEmptyFile(@"e:\Sub\Deeper\other.txt")
                .Build();

            // Act
            IEnumerable<string> entries =
                fileSystem.Directory.EnumerateFileSystemEntries(@"e:\", @"Sub\s*.txt", SearchOption.AllDirectories);

            // Assert
            string[] entryArray = entries.Should().HaveCount(2).And.Subject.ToArray();
            entryArray[0].Should().Be(@"e:\Sub\some2.txt");
            entryArray[1].Should().Be(@"e:\Sub\Deeper\some3.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderZ.txt\sub-zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-002.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-1.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-2.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\aaa\n-aaa.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-001.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\sub-aaa.txt")
                .IncludingEmptyFile(@"C:\base\aaa.txt")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\base");

            // Assert
            string[] entryArray = entries.Should().HaveCount(4).And.Subject.ToArray();
            entryArray[0].Should().Be(@"c:\base\aaa.txt");
            entryArray[1].Should().Be(@"c:\base\subfolderA");
            entryArray[2].Should().Be(@"c:\base\subfolderZ.txt");
            entryArray[3].Should().Be(@"c:\base\zzz.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_with_pattern_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderZ.txt\sub-zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-002.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-1.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-2.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\aaa\n-aaa.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-001.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\sub-aaa.txt")
                .IncludingEmptyFile(@"C:\base\aaa.txt")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"c:\base", "*.txt");

            // Assert
            string[] entryArray = entries.Should().HaveCount(3).And.Subject.ToArray();
            entryArray[0].Should().Be(@"c:\base\aaa.txt");
            entryArray[1].Should().Be(@"c:\base\subfolderZ.txt");
            entryArray[2].Should().Be(@"c:\base\zzz.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_recursively_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderZ.txt\sub-zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-002.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-1.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-2.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\aaa\n-aaa.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-001.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\sub-aaa.txt")
                .IncludingEmptyFile(@"C:\base\aaa.txt")
                .Build();

            // Act
            IEnumerable<string> entries =
                fileSystem.Directory.EnumerateFileSystemEntries(@"c:\base", searchOption: SearchOption.AllDirectories);

            // Assert
            string[] entryArray = entries.Should().HaveCount(14).And.Subject.ToArray();
            entryArray[0].Should().Be(@"c:\base\aaa.txt");
            entryArray[1].Should().Be(@"c:\base\subfolderA");
            entryArray[2].Should().Be(@"c:\base\subfolderZ.txt");
            entryArray[3].Should().Be(@"c:\base\zzz.txt");
            entryArray[4].Should().Be(@"c:\base\subfolderA\nested");
            entryArray[5].Should().Be(@"c:\base\subfolderA\sub-aaa.txt");
            entryArray[6].Should().Be(@"c:\base\subfolderA\nested\aaa");
            entryArray[7].Should().Be(@"c:\base\subfolderA\nested\deeper");
            entryArray[8].Should().Be(@"c:\base\subfolderA\nested\n-001.txt");
            entryArray[9].Should().Be(@"c:\base\subfolderA\nested\n-002.txt");
            entryArray[10].Should().Be(@"c:\base\subfolderA\nested\aaa\n-aaa.txt");
            entryArray[11].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-1.txt");
            entryArray[12].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-2.txt");
            entryArray[13].Should().Be(@"c:\base\subfolderZ.txt\sub-zzz.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_recursively_for_pattern_with_path_it_must_return_them_in_order()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\base\zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderZ.txt\sub-zzz.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-002.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-1.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\deeper\ddd-2.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\aaa\n-aaa.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\nested\n-001.txt")
                .IncludingEmptyFile(@"C:\base\subfolderA\sub-aaa.txt")
                .IncludingEmptyFile(@"C:\base\aaa.txt")
                .Build();

            // Act
            IEnumerable<string> entries =
                fileSystem.Directory.EnumerateFileSystemEntries(@"c:\", @"base\*.txt", SearchOption.AllDirectories);

            // Assert
            string[] entryArray = entries.Should().HaveCount(10).And.Subject.ToArray();
            entryArray[0].Should().Be(@"c:\base\aaa.txt");
            entryArray[1].Should().Be(@"c:\base\subfolderZ.txt");
            entryArray[2].Should().Be(@"c:\base\zzz.txt");
            entryArray[3].Should().Be(@"c:\base\subfolderA\sub-aaa.txt");
            entryArray[4].Should().Be(@"c:\base\subfolderA\nested\n-001.txt");
            entryArray[5].Should().Be(@"c:\base\subfolderA\nested\n-002.txt");
            entryArray[6].Should().Be(@"c:\base\subfolderA\nested\aaa\n-aaa.txt");
            entryArray[7].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-1.txt");
            entryArray[8].Should().Be(@"c:\base\subfolderA\nested\deeper\ddd-2.txt");
            entryArray[9].Should().Be(@"c:\base\subfolderZ.txt\sub-zzz.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"c:\folder");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\folder'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"F:\some\FOLDER\file.TXT")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"f:\SOME\folder");

            // Assert
            entries.Should().ContainSingle(x => x == @"f:\SOME\folder\file.TXT");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"x:\path\to\file.ext")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"x:\path\to  ");

            // Assert
            entries.Should().ContainSingle(x => x == @"x:\path\to\file.ext");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_pattern_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"x:\path\to\sub.ext")
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"x:\path\to", "*.ext  ");

            // Assert
            entries.Should().ContainSingle(x => x == @"x:\path\to\sub.ext");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\other\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some\other");

            // Act
            IEnumerable<string> entries =
                fileSystem.Directory.EnumerateFileSystemEntries(@"\some", searchOption: SearchOption.AllDirectories);

            // Assert
            string[] entryArray = entries.Should().HaveCount(2).And.Subject.ToArray();
            entryArray[0].Should().Be(@"\some\other");
            entryArray[1].Should().Be(@"\some\other\file.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_relative_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\folder\file.txt")
                .IncludingDirectory(@"c:\some\folder\sub")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries("FOLDER");

            // Assert
            string[] entryArray = entries.Should().HaveCount(2).And.Subject.ToArray();
            entryArray[0].Should().Be(@"FOLDER\file.txt");
            entryArray[1].Should().Be(@"FOLDER\sub");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_relative_directory_with_parent_reference_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\folder\file.txt")
                .IncludingDirectory(@"c:\some\folder\sub")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some\folder");

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(@"..\FOLDER");

            // Assert
            string[] entryArray = entries.Should().HaveCount(2).And.Subject.ToArray();
            entryArray[0].Should().Be(@"..\FOLDER\file.txt");
            entryArray[1].Should().Be(@"..\FOLDER\sub");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(path);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage(@"The directory name is invalid.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"C:\some\file.txt\deeper");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\file.txt\deeper'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"C:\some\file.txt\deeper\more");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\file.txt\deeper\more'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"P:\")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(@"P:\folder\sub");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'P:\folder\sub'.");
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_enumerating_entries_for_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkShare(), false);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_enumerating_entries_below_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkDirectoryAtDepth(1), false);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_missing_remote_directory_it_must_fail()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);
            string parentPath = PathFactory.NetworkShare();

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(parentPath)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries(path);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                $"Could not find a part of the path '{path}'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_existing_remote_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(PathFactory.NetworkDirectoryAtDepth(2))
                .IncludingEmptyFile(PathFactory.NetworkFileAtDepth(2))
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(PathFactory.NetworkDirectoryAtDepth(1));

            // Assert
            string[] entryArray = entries.Should().HaveCount(2).And.Subject.ToArray();
            entryArray.Should().ContainSingle(x => x == PathFactory.NetworkDirectoryAtDepth(2, false));
            entryArray.Should().ContainSingle(x => x == PathFactory.NetworkFileAtDepth(2, false));
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.EnumerateFileSystemEntries("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_enumerating_entries_for_extended_remote_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(PathFactory.NetworkDirectoryAtDepth(2))
                .IncludingEmptyFile(PathFactory.NetworkFileAtDepth(2))
                .Build();

            // Act
            IEnumerable<string> entries = fileSystem.Directory.EnumerateFileSystemEntries(PathFactory.NetworkDirectoryAtDepth(1, true));

            // Assert
            string[] entryArray = entries.Should().HaveCount(2).And.Subject.ToArray();
            entryArray.Should().ContainSingle(x => x == PathFactory.NetworkDirectoryAtDepth(2, true));
            entryArray.Should().ContainSingle(x => x == PathFactory.NetworkFileAtDepth(2, true));
        }
    }
}
