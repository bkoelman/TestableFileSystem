using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.DiskSpace
{
    public sealed class InsufficientSpaceSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_writing_to_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(512))
                .Build();

            // Act
            Action action = () => fileSystem.File.WriteAllBytes(path, BufferFactory.Create(1024));

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");

            AssertFileSize(fileSystem, path, 0);
            AssertFreeSpaceOnDrive(fileSystem, "C:", 512);
        }

        [Fact, InvestigateRunOnFileSystem]
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
            AssertFileSize(fileSystem, path, 0);
        }

        [Fact, InvestigateRunOnFileSystem]
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

            AssertFileSize(fileSystem, path, 64);
        }

        [Fact, InvestigateRunOnFileSystem]
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

            AssertFileSize(fileSystem, path, 64);
        }

        [Fact, InvestigateRunOnFileSystem]
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

            AssertFileSize(fileSystem, path, 64);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_copying_file_to_same_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\source.txt";
            const string targetPath = @"C:\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(1024))
                .IncludingBinaryFile(sourcePath, BufferFactory.Create(768))
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(sourcePath, targetPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");

            AssertFreeSpaceOnDrive(fileSystem, "C:", 256);
            AssertFileSize(fileSystem, sourcePath, 768);
            fileSystem.File.Exists(targetPath).Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_copying_over_existing_file_to_same_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\source.txt";
            const string targetPath = @"C:\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(1024))
                .IncludingBinaryFile(sourcePath, BufferFactory.Create(768))
                .IncludingEmptyFile(targetPath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(sourcePath, targetPath, true);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");

            AssertFreeSpaceOnDrive(fileSystem, "C:", 256);
            AssertFileSize(fileSystem, sourcePath, 768);
            fileSystem.File.Exists(targetPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_copying_file_to_other_drive_it_must_fail()
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
                    .WithFreeSpace(512))
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(sourcePath, targetPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");

            AssertFreeSpaceOnDrive(fileSystem, "C:", 3328);
            AssertFileSize(fileSystem, sourcePath, 768);
            AssertFreeSpaceOnDrive(fileSystem, "D:", 512);
            fileSystem.File.Exists(targetPath).Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_copying_over_existing_file_to_other_drive_it_must_fail()
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
                    .WithFreeSpace(512))
                .IncludingEmptyFile(targetPath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(sourcePath, targetPath, true);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");

            AssertFreeSpaceOnDrive(fileSystem, "C:", 3328);
            AssertFileSize(fileSystem, sourcePath, 768);
            AssertFreeSpaceOnDrive(fileSystem, "D:", 512);
            fileSystem.File.Exists(targetPath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_other_drive_it_must_fail()
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
                    .WithFreeSpace(512))
                .Build();

            // Act
            Action action = () => fileSystem.File.Move(sourcePath, targetPath);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("There is not enough space on the disk.");

            // Assert
            AssertFreeSpaceOnDrive(fileSystem, "C:", 3328);
            AssertFileSize(fileSystem, sourcePath, 768);
            AssertFreeSpaceOnDrive(fileSystem, "D:", 512);
            fileSystem.File.Exists(targetPath).Should().BeFalse();
        }

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
