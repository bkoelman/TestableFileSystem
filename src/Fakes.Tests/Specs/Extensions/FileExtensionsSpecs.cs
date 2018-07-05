using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.Extensions
{
    public sealed class FileExtensionsSpecs
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

            // Act
            using (IFileStream stream = fileSystem.File.OpenRead(path))
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

            // Act
            using (IFileStream stream = fileSystem.File.OpenWrite(path))
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

            // Act
            using (StreamReader reader = fileSystem.File.OpenText(path))
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

            // Act
            using (StreamWriter writer = fileSystem.File.CreateText(path))
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

            // Act
            using (StreamWriter writer = fileSystem.File.AppendText(path))
            {
                // Assert
                writer.BaseStream.CanRead.Should().BeFalse();
                writer.BaseStream.CanWrite.Should().BeTrue();
            }
        }

        [Fact]
        private void When_reading_all_bytes_from_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Act
            byte[] bytes = fileSystem.File.ReadAllBytes(path);

            // Assert
            bytes.Should().HaveCount(DefaultContents.Length);
            new string(Encoding.ASCII.GetChars(bytes)).Should().Be(DefaultContents);
        }

        [Fact]
        private void When_reading_all_lines_from_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents + Environment.NewLine + DefaultContents)
                .Build();

            // Act
            string[] lines = fileSystem.File.ReadAllLines(path);

            // Assert
            lines.Should().HaveCount(2);
        }

        [Fact]
        private void When_reading_all_text_from_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Act
            string text = fileSystem.File.ReadAllText(path);

            // Assert
            text.Should().Be(DefaultContents);
        }

        [Fact]
        private void When_writing_all_bytes_to_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";
            const byte fileContents = 0xEE;

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            fileSystem.File.WriteAllBytes(path, new[]
            {
                fileContents
            });

            // Assert
            fileSystem.File.ReadAllBytes(path).First().Should().Be(fileContents);
        }

        [Fact]
        private void When_writing_all_lines_to_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            fileSystem.File.WriteAllLines(path, new[]
            {
                DefaultContents,
                DefaultContents
            });

            // Assert
            fileSystem.File.ReadAllLines(path).Should().HaveCount(2);
        }

        [Fact]
        private void When_writing_all_text_to_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            fileSystem.File.WriteAllText(path, DefaultContents);

            // Assert
            fileSystem.File.ReadAllText(path).Should().Be(DefaultContents);
        }

        [Fact]
        private void When_appending_all_lines_to_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents + Environment.NewLine)
                .Build();

            // Act
            fileSystem.File.AppendAllLines(path, new[]
            {
                DefaultContents,
                DefaultContents
            });

            // Assert
            fileSystem.File.ReadAllLines(path).Should().HaveCount(3);
        }

        [Fact]
        private void When_appending_all_text_to_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            // Act
            fileSystem.File.AppendAllText(path, DefaultContents);

            // Assert
            fileSystem.File.ReadAllText(path).Should().Be(DefaultContents + DefaultContents);
        }
    }
}
