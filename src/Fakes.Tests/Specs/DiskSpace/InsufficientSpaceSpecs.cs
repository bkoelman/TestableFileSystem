using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.DiskSpace
{
    public sealed class InsufficientSpaceSpecs
    {
        [Fact]
        private void When_writing_to_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(512))
                .Build();

            // Act
            Action action = () => fileSystem.File.WriteAllBytes(@"C:\file.txt", BufferFactory.Create(1024));

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");

            AssertFileSize(fileSystem, @"C:\file.txt", 0);
            AssertFreeSpaceOnDrive(fileSystem, "C:", 512);
        }

        [Fact]
        private void When_overwriting_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(3072))
                .IncludingBinaryFile(path, BufferFactory.Create(1024))
                .Build();

            // Act
            Action action = () => fileSystem.File.WriteAllBytes(path, BufferFactory.Create(4000));

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");

            AssertFreeSpaceOnDrive(fileSystem, "C:", 3072);
            AssertFileSize(fileSystem, @"C:\file.txt", 0);
        }

        [Fact]
        private void When_increasing_file_size_using_SetLength_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(512))
                .IncludingBinaryFile(path, BufferFactory.Create(32))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenWrite(path))
            {
                byte[] buffer = BufferFactory.Create(64);
                stream.Write(buffer, 0, buffer.Length);

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.SetLength(1280);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");

                AssertFreeSpaceOnDrive(fileSystem, "C:", 448);
            }

            AssertFileSize(fileSystem, @"C:\file.txt", 64);
        }

        [Fact]
        private void When_seeking_past_end_of_file_it_must_not_allocate_disk_space()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(512))
                .IncludingBinaryFile(path, BufferFactory.Create(32))
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open))
            {
                // Act
                stream.Seek(1024, SeekOrigin.Begin);

                // Assert
                AssertFreeSpaceOnDrive(fileSystem, "C:", 480);
            }

            AssertFileSize(fileSystem, @"C:\file.txt", 32);
        }

        [Fact]
        private void When_moving_position_past_end_of_file_it_must_not_allocate_disk_space()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(512))
                .IncludingBinaryFile(path, BufferFactory.Create(32))
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open))
            {
                // Act
                stream.Position = 1024;

                // Assert
                AssertFreeSpaceOnDrive(fileSystem, "C:", 480);
            }

            AssertFileSize(fileSystem, @"C:\file.txt", 32);
        }

        [Fact]
        private void When_increasing_file_size_using_Seek_followed_by_write_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(512))
                .IncludingBinaryFile(path, BufferFactory.Create(32))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenWrite(path))
            {
                byte[] buffer = BufferFactory.Create(64);
                stream.Write(buffer, 0, buffer.Length);

                stream.Seek(1280, SeekOrigin.Begin);

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.WriteByte(0x33);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");

                AssertFreeSpaceOnDrive(fileSystem, "C:", 448);
            }

            AssertFileSize(fileSystem, @"C:\file.txt", 64);
        }

        [Fact]
        private void When_increasing_file_size_using_Position_followed_by_write_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(512))
                .IncludingBinaryFile(path, BufferFactory.Create(32))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenWrite(path))
            {
                byte[] buffer = BufferFactory.Create(64);
                stream.Write(buffer, 0, buffer.Length);

                stream.Position = 1280;

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.WriteByte(0x33);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");

                AssertFreeSpaceOnDrive(fileSystem, "C:", 448);
            }

            AssertFileSize(fileSystem, @"C:\file.txt", 64);
        }

        // TODO: Add specs for the various I/O operations that reduce available disk space.

        [AssertionMethod]
        private static void AssertFileSize([NotNull] IFileSystem fileSystem, [NotNull] string path, long fileSizeExpected)
        {
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);
            fileInfo.Length.Should().Be(fileSizeExpected);
        }

        [AssertionMethod]
        private static void AssertFreeSpaceOnDrive([NotNull] IFileSystem fileSystem, [NotNull] string driveName,
            long freeSpaceExpected)
        {
#if !NETCOREAPP1_1
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo(driveName);
            driveInfo.AvailableFreeSpace.Should().Be(freeSpaceExpected);
#endif
        }
    }
}
