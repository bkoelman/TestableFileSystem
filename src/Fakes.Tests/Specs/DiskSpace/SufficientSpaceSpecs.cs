﻿#if !NETCOREAPP1_1
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.DiskSpace
{
    public sealed class SufficientSpaceSpecs
    {
        // TODO: Add specs for the various I/O operations that influence available disk space.

        [Fact]
        private void When_writing_to_new_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
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
        private void When_increasing_file_size_using_SetLength_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(path, BufferFactory.Create(1024))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenWrite(path))
            {
                // Act
                stream.SetLength(1280);

                // Assert
                IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
                driveInfo.AvailableFreeSpace.Should().Be(2816);
            }
        }

        [Fact]
        private void When_increasing_file_size_using_Seek_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(path, BufferFactory.Create(1024))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenWrite(path))
            {
                // Act
                stream.Seek(1280, SeekOrigin.Begin);

                // Assert
                IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
                driveInfo.AvailableFreeSpace.Should().Be(2816);
            }
        }

        [Fact]
        private void When_increasing_file_size_using_Position_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(path, BufferFactory.Create(1024))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenWrite(path))
            {
                // Act
                stream.Position = 1280;

                // Assert
                IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
                driveInfo.AvailableFreeSpace.Should().Be(2816);
            }
        }

        [Fact]
        private void When_truncating_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(path, BufferFactory.Create(1024))
                .Build();

            // Act
            using (fileSystem.File.Open(path, FileMode.Truncate))
            {
                // Assert
                IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
                driveInfo.AvailableFreeSpace.Should().Be(4096);
            }
        }

        [Fact]
        private void When_decreasing_file_size_using_overwrite_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(path, BufferFactory.Create(1024))
                .Build();

            // Act
            fileSystem.File.WriteAllBytes(path, BufferFactory.Create(256));

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(3840);
        }

        [Fact]
        private void When_decreasing_file_size_using_SetLength_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(path, BufferFactory.Create(1024))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenWrite(path))
            {
                // Act
                stream.SetLength(768);

                // Assert
                IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
                driveInfo.AvailableFreeSpace.Should().Be(3328);
            }
        }

        [Fact]
        private void When_copying_file_to_same_drive_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\source.txt";
            const string targetPath = @"C:\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(sourcePath, BufferFactory.Create(768))
                .Build();

            // Act
            fileSystem.File.Copy(sourcePath, targetPath);

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(2560);
        }

        [Fact]
        private void When_copying_file_to_other_drive_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\source.txt";
            const string targetPath = @"D:\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(sourcePath, BufferFactory.Create(768))
                .IncludingVolume("D:", new FakeVolumeInfoBuilder()
                    .OfCapacity(16384)
                    .WithFreeSpace(6144))
                .Build();

            // Act
            fileSystem.File.Copy(sourcePath, targetPath);

            // Assert
            IDriveInfo driveInfoC = fileSystem.ConstructDriveInfo("C:");
            driveInfoC.AvailableFreeSpace.Should().Be(3328);

            IDriveInfo driveInfoD = fileSystem.ConstructDriveInfo("D:");
            driveInfoD.AvailableFreeSpace.Should().Be(5376);
        }

        [Fact]
        private void When_moving_file_to_same_drive_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\source.txt";
            const string targetPath = @"C:\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(sourcePath, BufferFactory.Create(768))
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, targetPath);

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(3328);
        }

        [Fact]
        private void When_moving_file_to_other_drive_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\source.txt";
            const string targetPath = @"D:\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(sourcePath, BufferFactory.Create(768))
                .IncludingVolume("D:", new FakeVolumeInfoBuilder()
                    .OfCapacity(16384)
                    .WithFreeSpace(6144))
                .Build();

            // Act
            fileSystem.File.Move(sourcePath, targetPath);

            // Assert
            IDriveInfo driveInfoC = fileSystem.ConstructDriveInfo("C:");
            driveInfoC.AvailableFreeSpace.Should().Be(4096);

            IDriveInfo driveInfoD = fileSystem.ConstructDriveInfo("D:");
            driveInfoD.AvailableFreeSpace.Should().Be(5376);
        }

        [Fact]
        private void When_deleting_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(path, BufferFactory.Create(1024))
                .Build();

            // Act
            fileSystem.File.Delete(path);

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(4096);
        }

        [Fact]
        private void When_replacing_file_without_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\source.txt";
            const string targetPath = @"C:\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(sourcePath, BufferFactory.Create(768))
                .IncludingBinaryFile(targetPath, BufferFactory.Create(1280))
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, null);

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(3328);
        }

        [Fact]
        private void When_replacing_file_with_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\source.txt";
            const string targetPath = @"C:\target.txt";
            const string backupPath = @"C:\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(sourcePath, BufferFactory.Create(768))
                .IncludingBinaryFile(targetPath, BufferFactory.Create(1280))
                .IncludingBinaryFile(backupPath, BufferFactory.Create(128))
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(2048);
        }
    }
}
#endif