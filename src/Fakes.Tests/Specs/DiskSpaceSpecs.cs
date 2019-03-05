/*using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs
{
    public sealed class DiskSpaceSpecs
    {
        [Fact]
        private void When_writing_to_file_with_sufficient_space_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .Build();

            // Act
            fileSystem.File.WriteAllBytes(@"C:\file.txt", BufferFactory.Create(1024));

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");

            driveInfo.AvailableFreeSpace.Should().Be(3072);
        }

        [Fact]
        private void When_writing_to_file_with_insufficient_space_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(512))
                .Build();

            // Act
            Action action = () => fileSystem.File.WriteAllBytes(@"C:\file.txt", BufferFactory.Create(1024));

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");
        }
    }
}*/
