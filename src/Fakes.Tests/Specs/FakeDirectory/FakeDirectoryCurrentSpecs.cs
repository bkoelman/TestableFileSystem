using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class FakeDirectoryCurrentSpecs
    {
        [Fact]
        private void When_setting_current_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\some");
        }

        [Fact]
        private void When_setting_relative_current_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some\");

            // Act
            fileSystem.Directory.SetCurrentDirectory(@".\folder");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\some\folder");
        }

        [Fact]
        private void When_setting_current_directory_to_missing_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"E:\");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'E:\'.");
        }

        [Fact]
        private void When_setting_current_directory_to_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"C:\other\folder");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'C:\other\folder'.");
        }

        [Fact]
        private void When_setting_current_directory_to_network_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\docserver\teams")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"\\docserver\teams");

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"The specified path is invalid.");
        }
    }
}
