using System;
using System.IO;
using System.Text;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeBuilder
{
    public sealed class BuilderIncludingBinaryFileSpecs
    {
        [NotNull]
        private static readonly byte[] DefaultContents = Encoding.ASCII.GetBytes("ABC");

        [NotNull]
        private static readonly byte[] LongerContents = Encoding.ASCII.GetBytes("ABC...XYZ");

        [Fact]
        private void When_including_binary_file_for_null_path_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => builder.IncludingBinaryFile(null, DefaultContents);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_including_binary_file_for_null_contents_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => builder.IncludingBinaryFile(@"c:\file.txt", null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_including_binary_file_for_empty_string_path_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingBinaryFile(string.Empty, DefaultContents);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("'path' cannot be empty or contain only whitespace.*");
        }

        [Fact]
        private void When_including_binary_file_for_empty_array_contents_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingBinaryFile(@"c:\file.txt", new byte[0]);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("'contents' cannot be empty.*");
        }

        [Fact]
        private void When_including_binary_file_for_whitespace_path_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingBinaryFile(" ", DefaultContents);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("'path' cannot be empty or contain only whitespace.*");
        }

        [Fact]
        private void When_including_binary_file_for_invalid_root_path_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingBinaryFile("::", DefaultContents);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_including_binary_file_for_invalid_characters_in_path_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingBinaryFile("some?.txt", DefaultContents);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_including_local_binary_file_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingBinaryFile(path, DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Archive);

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Length.Should().Be(DefaultContents.Length);
        }

        [Fact]
        private void When_including_local_binary_file_with_attributes_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingBinaryFile(path, DefaultContents, FileAttributes.Hidden)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Hidden);
        }

        [Fact]
        private void When_including_existing_local_binary_file_it_must_overwrite()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, LongerContents);

            // Act
            IFileSystem fileSystem = builder
                .IncludingBinaryFile(path, DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Length.Should().Be(DefaultContents.Length);
        }

        [Fact]
        private void When_including_existing_local_binary_file_with_different_casing_it_must_overwrite()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingBinaryFile(@"C:\some\FILE.txt", LongerContents);

            // Act
            IFileSystem fileSystem = builder
                .IncludingBinaryFile(@"c:\SOME\file.TXT", DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(@"C:\some\FILE.txt").Should().BeTrue();

            IFileInfo info = fileSystem.ConstructFileInfo(@"C:\some\FILE.txt");
            info.Length.Should().Be(DefaultContents.Length);
        }

        [Fact]
        private void When_including_local_binary_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\path\to\folder\readme.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingBinaryFile(@"d:\path\to\folder\readme.txt  ", DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_including_relative_local_binary_file_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingBinaryFile(@"some\file.txt", DefaultContents);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_including_local_binary_file_that_is_drive_letter_it_must_fail()
        {
            // Arrange
            const string path = @"C:\";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingDirectory(path);

            // Act
            Action action = () => builder.IncludingBinaryFile(path, DefaultContents);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"Could not find a part of the path 'C:\'.");
        }

        [Fact]
        private void When_including_local_binary_file_for_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingDirectory(path);

            // Act
            Action action = () => builder.IncludingBinaryFile(path, DefaultContents);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"Cannot create 'C:\some\subfolder' because a file or directory with the same name already exists.");
        }

        [Fact]
        private void When_including_local_binary_file_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt");

            // Act
            Action action = () => builder.IncludingBinaryFile(@"c:\some\file.txt\nested.txt", DefaultContents);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"Cannot create 'C:\some\file.txt' because a file or directory with the same name already exists.");
        }

        [Fact]
        private void When_including_local_binary_file_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt");

            // Act
            Action action = () => builder.IncludingBinaryFile(@"c:\some\file.txt\nested.txt\deeper.txt", DefaultContents);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"Cannot create 'C:\some\file.txt' because a file or directory with the same name already exists.");
        }

        [Fact]
        private void When_including_remote_binary_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\folder\file.txt";

            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingBinaryFile(path, DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_including_existing_remote_binary_file_it_must_overwrite()
        {
            // Arrange
            const string path = @"\\server\share\folder\file.txt";

            FakeFileSystemBuilder builder = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, LongerContents);

            // Act
            IFileSystem fileSystem = builder
                .IncludingBinaryFile(path, DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Length.Should().Be(DefaultContents.Length);
        }

        [Fact]
        private void When_including_binary_file_for_reserved_name_it_must_fail()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            Action action = () => builder.IncludingBinaryFile("COM1", DefaultContents);

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_including_extended_local_binary_file_it_must_succeed()
        {
            // Arrange
            var builder = new FakeFileSystemBuilder();

            // Act
            IFileSystem fileSystem = builder
                .IncludingBinaryFile(@"\\?\d:\path\to\folder\readme.txt", DefaultContents)
                .Build();

            // Assert
            fileSystem.File.Exists(@"D:\path\to\folder\readme.txt").Should().BeTrue();
            fileSystem.File.GetAttributes(@"D:\path\to\folder\readme.txt").Should().Be(FileAttributes.Archive);
        }
    }
}
