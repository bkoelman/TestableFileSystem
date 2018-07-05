using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoTimeLastWriteSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime DefaultTime = DefaultTimeUtc.ToLocalTime();

        private static readonly DateTime AlternateTimeUtc = 2.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime AlternateTime = AlternateTimeUtc.ToLocalTime();

        [Fact]
        private void When_getting_directory_last_write_time_in_local_zone_it_must_lazy_load()
        {
            // Arrange
            const string path = @"c:\some\folder";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            fileSystem.Directory.SetLastWriteTime(path, AlternateTime);

            // Act
            DateTime time = dirInfo.LastWriteTime;

            // Assert
            time.Should().Be(AlternateTime);
        }

        [Fact]
        private void When_getting_directory_last_write_time_in_local_zone_it_must_cache()
        {
            // Arrange
            const string path = @"c:\some\folder";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            DateTime beforeTime = dirInfo.LastWriteTime;

            fileSystem.Directory.SetLastWriteTime(path, AlternateTime);

            // Act
            DateTime afterTime = dirInfo.LastWriteTime;

            // Assert
            beforeTime.Should().Be(DefaultTime);
            afterTime.Should().Be(DefaultTime);
        }

        [Fact]
        private void When_getting_directory_last_write_time_in_local_zone_after_external_change_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\folder";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            DateTime beforeTime = dirInfo.LastWriteTime;

            fileSystem.Directory.SetLastWriteTime(path, AlternateTime);

            // Act
            dirInfo.Refresh();

            // Assert
            DateTime afterTime = dirInfo.LastWriteTime;

            beforeTime.Should().Be(DefaultTime);
            afterTime.Should().Be(AlternateTime);
        }

        [Fact]
        private void When_changing_directory_last_write_time_in_local_zone_it_must_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\folder";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            DateTime beforeTime = dirInfo.LastWriteTime;

            // Act
            dirInfo.LastWriteTime = AlternateTime;

            // Assert
            DateTime afterTime = dirInfo.LastWriteTime;

            beforeTime.Should().Be(DefaultTime);
            afterTime.Should().Be(AlternateTime);
        }
    }
}
