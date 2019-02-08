﻿#if !NETCOREAPP1_1
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDriveInfo
{
    public sealed class FileSystemGetDrivesSpecs
    {
        [Fact]
        private void When_getting_drives_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("D:", new FakeVolumeBuilder())
                .IncludingVolume("m:", new FakeVolumeBuilder())
                .IncludingDirectory(@"N:\")
                .IncludingDirectory(@"e:\")
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            IDriveInfo[] driveInfos = fileSystem.GetDrives();

            // Assert
            driveInfos.Should().NotBeNull();
            driveInfos.Should().HaveCount(5);

            driveInfos[0].Name.Should().Be(@"C:\");
            driveInfos[1].Name.Should().Be(@"D:\");
            driveInfos[2].Name.Should().Be(@"E:\");
            driveInfos[3].Name.Should().Be(@"M:\");
            driveInfos[4].Name.Should().Be(@"N:\");
        }
    }
}
#endif
