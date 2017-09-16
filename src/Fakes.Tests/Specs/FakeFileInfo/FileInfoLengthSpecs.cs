using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoLengthSpecs
    {
        private const string DefaultContents = "ABC";
        private const string AlternateContents = "ABC...XYZ";

        [Fact]
        private void When_getting_length_it_must_lazy_load()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            fileSystem.File.WriteAllText(path, AlternateContents);

            // Act
            long length = fileInfo.Length;

            // Assert
            length.Should().Be(AlternateContents.Length);
        }

        [Fact]
        private void When_getting_length_it_must_cache()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            long beforeLength = fileInfo.Length;

            fileSystem.File.WriteAllText(path, AlternateContents);

            // Act
            long afterLength = fileInfo.Length;

            // Assert
            beforeLength.Should().Be(DefaultContents.Length);
            afterLength.Should().Be(DefaultContents.Length);
        }

        [Fact]
        private void When_getting_length_after_external_change_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            long beforeLength = fileInfo.Length;

            fileSystem.File.WriteAllText(path, AlternateContents);

            // Act
            fileInfo.Refresh();

            // Assert
            long afterLength = fileInfo.Length;

            beforeLength.Should().Be(DefaultContents.Length);
            afterLength.Should().Be(AlternateContents.Length);
        }

        [Fact]
        private void When_changing_length_it_must_not_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, DefaultContents)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            long beforeLength = fileInfo.Length;

            // Act
            using (fileInfo.CreateText())
            {
            }

            // Assert
            long afterLength = fileInfo.Length;

            beforeLength.Should().Be(DefaultContents.Length);
            afterLength.Should().Be(DefaultContents.Length);
        }
    }
}
