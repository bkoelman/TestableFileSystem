#if !NETCOREAPP1_1
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoDecryptSpecs
    {
        [Fact]
        private void When_decrypting_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            fileSystem.File.Encrypt(path);

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Act
            fileInfo.Decrypt();

            // Assert
            fileInfo.Attributes.Should().NotHaveFlag(FileAttributes.Encrypted);
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }
    }
}
#endif
