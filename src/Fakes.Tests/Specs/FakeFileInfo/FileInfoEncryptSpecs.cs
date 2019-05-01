#if !NETCOREAPP1_1
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoEncryptSpecs
    {
        [Fact]
        private void When_encrypting_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Act
            fileInfo.Encrypt();

            // Assert
            fileInfo.Attributes.Should().HaveFlag(FileAttributes.Encrypted);
            fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
        }
    }
}
#endif
