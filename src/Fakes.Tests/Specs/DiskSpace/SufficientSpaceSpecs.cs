#if !NETCOREAPP1_1
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.DiskSpace
{
    public sealed class SufficientSpaceSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
        private void When_writing_to_new_file_with_DeleteOnClose_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(@"C:\file.txt", options: FileOptions.DeleteOnClose))
            {
                byte[] buffer = BufferFactory.Create(1024);
                stream.Write(buffer, 0, buffer.Length);
            }

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(4096);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_overwriting_file_it_must_succeed()
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
            fileSystem.File.WriteAllBytes(path, BufferFactory.Create(4000));

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(96);
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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
                IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
                driveInfo.AvailableFreeSpace.Should().Be(480);
            }
        }

        [Fact, InvestigateRunOnFileSystem]
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
                IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
                driveInfo.AvailableFreeSpace.Should().Be(480);
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_increasing_file_size_using_Seek_followed_by_write_it_must_succeed()
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
                stream.WriteByte(0x33);

                // Assert
                IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
                driveInfo.AvailableFreeSpace.Should().Be(2815);
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_increasing_file_size_using_Position_followed_by_write_it_must_succeed()
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
                stream.WriteByte(0x33);

                // Assert
                IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
                driveInfo.AvailableFreeSpace.Should().Be(2815);
            }
        }

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
        private void When_renaming_file_it_must_succeed()
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

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_file_to_same_drive_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\src\source.txt";
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
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

        [Fact, InvestigateRunOnFileSystem]
        private void When_encrypting_file_it_must_succeed()
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
            fileSystem.File.Encrypt(path);

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(3072);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_decrypting_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(path, BufferFactory.Create(1024))
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Decrypt(path);

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(3072);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_without_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\source.txt";
            const string targetPath = @"C:\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(2112))
                .IncludingBinaryFile(sourcePath, BufferFactory.Create(768))
                .IncludingBinaryFile(targetPath, BufferFactory.Create(1280))
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, null);

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(1344);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_replacing_file_with_backup_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\source.txt";
            const string targetPath = @"C:\target.txt";
            const string backupPath = @"C:\backup.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(2240))
                .IncludingBinaryFile(sourcePath, BufferFactory.Create(768))
                .IncludingBinaryFile(targetPath, BufferFactory.Create(1280))
                .IncludingBinaryFile(backupPath, BufferFactory.Create(128))
                .Build();

            // Act
            fileSystem.File.Replace(sourcePath, targetPath, backupPath);

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(192);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\folder\subtree";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .Build();

            // Act
            fileSystem.Directory.CreateDirectory(path);

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(4096);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_deleting_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingDirectory(@"C:\some\folder\subtree")
                .Build();

            // Act
            fileSystem.Directory.Delete(@"C:\some", true);

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(4096);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_renaming_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(@"c:\source\src.txt", BufferFactory.Create(64))
                .IncludingBinaryFile(@"c:\source\nested\src.txt", BufferFactory.Create(256))
                .IncludingDirectory(@"c:\source\subfolder")
                .Build();

            // Act
            fileSystem.Directory.Move(@"C:\source", @"C:\target");

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(3776);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_moving_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .IncludingBinaryFile(@"c:\source\src.txt", BufferFactory.Create(64))
                .IncludingBinaryFile(@"c:\source\nested\src.txt", BufferFactory.Create(256))
                .IncludingDirectory(@"c:\source\subfolder")
                .IncludingDirectory(@"c:\target\container")
                .Build();

            // Act
            fileSystem.Directory.Move(@"C:\source", @"c:\target\container\newname");

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(3776);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_temporary_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("C:", new FakeVolumeInfoBuilder()
                    .OfCapacity(8192)
                    .WithFreeSpace(4096))
                .WithTempDirectory(@"c:\")
                .Build();

            // Act
            fileSystem.Path.GetTempFileName();

            // Assert
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("C:");
            driveInfo.AvailableFreeSpace.Should().Be(4096);
        }
    }
}
#endif
