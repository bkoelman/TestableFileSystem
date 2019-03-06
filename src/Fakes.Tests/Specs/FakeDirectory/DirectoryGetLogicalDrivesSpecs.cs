#if !NETCOREAPP1_1
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryGetLogicalDrivesSpecs
    {
        [Fact]
        private void When_getting_logical_drives_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingVolume("D:", new FakeVolumeInfoBuilder())
                .IncludingVolume("m:", new FakeVolumeInfoBuilder())
                .IncludingDirectory(@"N:\")
                .IncludingDirectory(@"e:\")
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            string[] drives = fileSystem.Directory.GetLogicalDrives();

            // Assert
            drives.Should().NotBeNull();
            drives.Should().HaveCount(5);

            drives[0].Should().Be(@"C:\");
            drives[1].Should().Be(@"D:\");
            drives[2].Should().Be(@"E:\");
            drives[3].Should().Be(@"M:\");
            drives[4].Should().Be(@"N:\");
        }
    }
}
#endif
