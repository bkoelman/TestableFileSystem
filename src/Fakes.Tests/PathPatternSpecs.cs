using System;
using FluentAssertions;
using Xunit;

namespace TestableFileSystem.Fakes.Tests
{
    public sealed class PathPatternSpecs
    {
        [Fact]
        private void When_pattern_is_empty_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => PathPattern.Create(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        private void When_pattern_is_whitespace_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => PathPattern.Create(" ");

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        private void When_pattern_starts_with_path_separator_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => PathPattern.Create(@"\some");

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        private void When_pattern_starts_with_drive_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => PathPattern.Create(@"c:\some");

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        private void When_pattern_starts_with_unc_path_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => PathPattern.Create(@"\\server\share");

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        private void When_pattern_contains_empty_directory_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => PathPattern.Create(@"some\\*.*");

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        private void When_pattern_contains_parent_directory_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => PathPattern.Create(@"..\*.*");

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        private void When_parent_directory_contains_asterisk_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => PathPattern.Create(@"ab*cd\*.*");

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        private void When_parent_directory_contains_question_it_must_fail()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => PathPattern.Create(@"ab?cd\*.*");

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        private void When_pattern_is_asterisk_it_must_match_nothing()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("*X*");

            // Act
            bool result = pattern.IsMatch("X");

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        private void When_pattern_is_asterisk_it_must_match_single()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("*");

            // Act
            bool result = pattern.IsMatch("a");

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        private void When_pattern_is_asterisk_it_must_match_multiple()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("*");

            // Act
            bool result = pattern.IsMatch("abc.def");

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        private void When_pattern_is_question_it_must_not_match_nothing()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("?a");

            // Act
            bool result = pattern.IsMatch("a");

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        private void When_pattern_is_question_it_must_match_single()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("?");

            // Act
            bool result = pattern.IsMatch("a");

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        private void When_pattern_is_question_it_must_not_match_multiple()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("?");

            // Act
            bool result = pattern.IsMatch("ab");

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        private void When_pattern_is_text_it_must_match_same_text()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("some");

            // Act
            bool result = pattern.IsMatch("some");

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        private void When_pattern_is_text_it_must_not_match_other_text()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("some");

            // Act
            bool result = pattern.IsMatch("other");

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        private void When_pattern_is_text_it_must_ignore_case()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("helloWORLD");

            // Act
            bool result = pattern.IsMatch("HELLOworld");

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        private void When_pattern_contains_asterisk_for_file_name_it_must_match_same_extension()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("*.txt");

            // Act
            bool result = pattern.IsMatch("file.txt");

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        private void When_pattern_contains_asterisk_for_file_name_it_must_not_match_other_extension()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("*.txt");

            // Act
            bool result = pattern.IsMatch("file.doc");

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        private void When_pattern_contains_question_for_file_extension_it_must_match()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create("*.ba?");

            // Act
            bool result = pattern.IsMatch("file.bak");

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        private void When_pattern_contains_path_it_must_match_components()
        {
            // Arrange
            PathPattern pattern = PathPattern.Create(@"desktop\file?a.*");

            // Act
            bool result1 = pattern.IsMatch("desktop");
            bool result2 = pattern.SubPattern != null && pattern.SubPattern.IsMatch("file_a.txt");

            // Assert
            result1.Should().Be(true);
            result2.Should().Be(true);
        }
    }
}
