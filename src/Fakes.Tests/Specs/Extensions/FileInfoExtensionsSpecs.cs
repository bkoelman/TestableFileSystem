using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.Extensions
{
    public sealed class FileInfoExtensionsSpecs
    {
        private const string DefaultContents = "ABC";

        [Fact]
        private void When_opening_file_for_reading_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Act
            using (IFileStream stream = info.OpenRead())
            {
                // Assert
                stream.CanRead.Should().BeTrue();
                stream.CanWrite.Should().BeFalse();
            }
        }

        [Fact]
        private void When_opening_file_for_writing_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Act
            using (IFileStream stream = info.OpenWrite())
            {
                // Assert
                stream.CanRead.Should().BeFalse();
                stream.CanWrite.Should().BeTrue();
            }
        }

        [Fact]
        private void When_opening_file_as_text_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Act
            using (StreamReader reader = info.OpenText())
            {
                // Assert
                reader.BaseStream.CanRead.Should().BeTrue();
                reader.BaseStream.CanWrite.Should().BeFalse();
            }
        }

        [Fact]
        private void When_creating_file_as_text_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Act
            using (StreamWriter writer = info.CreateText())
            {
                // Assert
                writer.BaseStream.CanRead.Should().BeFalse();
                writer.BaseStream.CanWrite.Should().BeTrue();
            }
        }

        [Fact]
        private void When_appending_to_file_as_text_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            IFileInfo info = fileSystem.ConstructFileInfo(path);

            // Act
            using (StreamWriter writer = info.AppendText())
            {
                // Assert
                writer.BaseStream.CanRead.Should().BeFalse();
                writer.BaseStream.CanWrite.Should().BeTrue();
            }
        }
    }
}
