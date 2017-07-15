using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeBuilder
{
    public sealed class FakeBuilderIncludingTextFileSpecs
    {
        private const string DefaultContents = "ABC";
        private const string LongerContents = "ABC...XYZ";

        [Fact]
        private void When_including_text_file_for_null_path_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => builder.IncludingTextFile(null, DefaultContents);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_including_text_file_for_null_contents_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => builder.IncludingTextFile(@"c:\file.txt", null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_including_text_file_for_empty_string_path_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingTextFile(string.Empty, DefaultContents);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("'path' cannot be empty or contain only whitespace.*");
        }

        [Fact]
        private void When_including_text_file_for_empty_string_contents_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingTextFile(@"c:\file.txt", string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("'contents' cannot be empty.*");
        }

        [Fact]
        private void When_including_text_file_for_whitespace_path_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingTextFile(" ", DefaultContents);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("'path' cannot be empty or contain only whitespace.*");
        }

        [Fact]
        private void When_including_text_file_for_invalid_root_path_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingTextFile("::", DefaultContents);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_including_text_file_for_invalid_characters_in_path_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingTextFile("some?.txt", DefaultContents);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_including_local_text_file_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Length.Should().Be(DefaultContents.Length);
        }

        [Fact]
        private void When_including_local_text_file_with_attributes_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingTextFile(path, DefaultContents, FileAttributes.Hidden)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Hidden);
        }

        [Fact]
        private void When_including_existing_local_text_file_it_must_overwrite()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingTextFile(path, LongerContents);

            // Act
            IFileSystem fileSystem = builder
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Length.Should().Be(DefaultContents.Length);
        }

        [Fact]
        private void When_including_existing_local_text_file_with_different_casing_it_must_overwrite()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingTextFile(@"C:\some\FILE.txt", LongerContents);

            // Act
            IFileSystem fileSystem = builder
                .IncludingTextFile(@"c:\SOME\file.TXT", DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(@"C:\some\FILE.txt").Should().BeTrue();

            IFileInfo info = fileSystem.ConstructFileInfo(@"C:\some\FILE.txt");
            info.Length.Should().Be(DefaultContents.Length);
        }

        [Fact]
        private void When_including_local_text_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingTextFile(@"d:\path\to\folder\readme.txt  ", DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_including_relative_local_text_file_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingTextFile(@"some\file.txt", DefaultContents);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_including_local_text_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingDirectory(path);

            // Act
            Action action = () => builder.IncludingTextFile(path, DefaultContents);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"Cannot create 'C:\some\subfolder' because a file or directory with the same name already exists.");
        }

        [Fact]
        private void When_including_remote_text_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\folder\file.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_including_existing_remote_text_file_it_must_overwrite()
        {
            // Arrange
            const string path = @"\\server\share\folder\file.txt";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingTextFile(path, LongerContents);

            // Act
            IFileSystem fileSystem = builder
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Length.Should().Be(DefaultContents.Length);
        }

        [Fact]
        private void When_including_text_file_for_reserved_name_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingTextFile(@"COM1", DefaultContents);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_including_extended_local_text_file_it_must_succeed()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingTextFile(@"\\?\d:\path\to\folder\readme.txt", DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(@"D:\path\to\folder\readme.txt").Should().BeTrue();
            fileSystem.File.GetAttributes(@"D:\path\to\folder\readme.txt").Should().Be(FileAttributes.Normal);
        }
    }
}
