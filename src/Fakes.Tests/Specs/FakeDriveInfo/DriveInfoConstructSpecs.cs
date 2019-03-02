#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDriveInfo
{
    public sealed class DriveInfoConstructSpecs
    {
        private const long OneGigabyte = 1024 * 1024 * 1024;

        [Fact]
        private void When_constructing_drive_info_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.ConstructDriveInfo(null);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        private void When_constructing_drive_info_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructDriveInfo(string.Empty);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("_")]
        [InlineData("_:")]
        [InlineData(@"_:\")]
        [InlineData("c ")]
        [InlineData(@"\folder")]
        [InlineData("file.txt")]
        [InlineData(@"\\server\share")]
        [InlineData("COM1")]
        [InlineData(@"\\?\c")]
        [InlineData(@"\\?\c:")]
        [InlineData(@"\\?\c:\")]
        private void When_constructing_drive_info_for_invalid_value_it_must_fail([NotNull] string driveName)
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructDriveInfo(driveName);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                @"Drive name must be a root directory ('C:\\') or a drive letter ('C').*");
        }

        [Theory]
        [InlineData("c")]
        [InlineData("c:")]
        [InlineData("c: ")]
        [InlineData(@"c:\")]
        [InlineData(@"c:\ ")]
        [InlineData(@"c:\*?")]
        [InlineData(@"c:\MissingFolder")]
        [InlineData(@"c:\MissingFolder ")]
        [InlineData(@"c:\MissingFolder\MissingFile.txt ")]
        private void When_constructing_drive_info_for_existing_drive_it_must_succeed([NotNull] string driveName)
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo(driveName);

            // Assert
            driveInfo.Name.Should().Be(@"c:\");
            driveInfo.IsReady.Should().BeTrue();
            driveInfo.AvailableFreeSpace.Should().Be(OneGigabyte);
            driveInfo.TotalFreeSpace.Should().Be(OneGigabyte);
            driveInfo.TotalSize.Should().Be(OneGigabyte);
            driveInfo.DriveType.Should().Be(DriveType.Fixed);
            driveInfo.DriveFormat.Should().Be("NTFS");
            driveInfo.VolumeLabel.Should().BeEmpty();
            driveInfo.ToString().Should().Be(@"c:\");

            IDirectoryInfo directoryInfo = driveInfo.RootDirectory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"c:\");
            directoryInfo.Exists.Should().BeTrue();
        }

        [Theory]
        [InlineData("X")]
        [InlineData("X:")]
        [InlineData("X: ")]
        [InlineData(@"X:\")]
        [InlineData(@"X:\ ")]
        [InlineData(@"X:\MissingFolder")]
        [InlineData(@"X:\MissingFolder ")]
        [InlineData(@"X:\MissingFolder\MissingFile.txt")]
        private void When_constructing_drive_info_for_missing_drive_it_must_succeed([NotNull] string driveName)
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo(driveName);

            // Assert
            driveInfo.Name.Should().Be(@"X:\");
            driveInfo.IsReady.Should().BeFalse();
            ActionFactory.IgnoreReturnValue(() => driveInfo.AvailableFreeSpace).Should().ThrowExactly<DriveNotFoundException>()
                .WithMessage(@"Could not find the drive 'X:\'. The drive might not be ready or might not be mapped.");
            ActionFactory.IgnoreReturnValue(() => driveInfo.TotalFreeSpace).Should().ThrowExactly<DriveNotFoundException>()
                .WithMessage(@"Could not find the drive 'X:\'. The drive might not be ready or might not be mapped.");
            ActionFactory.IgnoreReturnValue(() => driveInfo.TotalSize).Should().ThrowExactly<DriveNotFoundException>()
                .WithMessage(@"Could not find the drive 'X:\'. The drive might not be ready or might not be mapped.");
            driveInfo.DriveType.Should().Be(DriveType.NoRootDirectory);
            ActionFactory.IgnoreReturnValue(() => driveInfo.DriveFormat).Should().ThrowExactly<DriveNotFoundException>()
                .WithMessage(@"Could not find the drive 'X:\'. The drive might not be ready or might not be mapped.");
            ActionFactory.IgnoreReturnValue(() => driveInfo.VolumeLabel).Should().ThrowExactly<DriveNotFoundException>()
                .WithMessage(@"Could not find the drive 'X:\'. The drive might not be ready or might not be mapped.");
            driveInfo.ToString().Should().Be(@"X:\");

            IDirectoryInfo directoryInfo = driveInfo.RootDirectory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"X:\");
            directoryInfo.Exists.Should().BeFalse();
        }

        [Fact]
        private void When_constructing_drive_info_it_must_return_custom_values()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("P:", new FakeVolumeBuilder()
                    .OfCapacity(8 * OneGigabyte)
                    .WithFreeSpace(3 * OneGigabyte)
                    .OfType(DriveType.Network)
                    .InFormat("FAT32")
                    .Labeled("CompanyShare"))
                .Build();

            // Act
            IDriveInfo driveInfo = fileSystem.ConstructDriveInfo("p");

            // Assert
            driveInfo.Name.Should().Be(@"p:\");
            driveInfo.IsReady.Should().BeTrue();
            driveInfo.AvailableFreeSpace.Should().Be(3 * OneGigabyte);
            driveInfo.TotalFreeSpace.Should().Be(3 * OneGigabyte);
            driveInfo.TotalSize.Should().Be(8 * OneGigabyte);
            driveInfo.DriveType.Should().Be(DriveType.Network);
            driveInfo.DriveFormat.Should().Be("FAT32");
            driveInfo.VolumeLabel.Should().Be("CompanyShare");
            driveInfo.ToString().Should().Be(@"p:\");

            IDirectoryInfo directoryInfo = driveInfo.RootDirectory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"p:\");
            directoryInfo.Exists.Should().BeTrue();
        }
    }
}
#endif
