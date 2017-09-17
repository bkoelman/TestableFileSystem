using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoTimeLastAccessSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime DefaultTime = DefaultTimeUtc.ToLocalTime();

        private static readonly DateTime AlternateTimeUtc = 2.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime AlternateTime = AlternateTimeUtc.ToLocalTime();

        [Fact]
        private void When_getting_file_last_access_time_in_local_zone_it_must_lazy_load()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock { UtcNow = () => DefaultTime };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            fileSystem.File.SetLastAccessTime(path, AlternateTime);

            // Act
            DateTime time = fileInfo.LastAccessTime;

            // Assert
            time.Should().Be(AlternateTime);
        }

        [Fact]
        private void When_getting_file_last_access_time_in_local_zone_it_must_cache()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock { UtcNow = () => DefaultTime };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime beforeTime = fileInfo.LastAccessTime;

            fileSystem.File.SetLastAccessTime(path, AlternateTime);

            // Act
            DateTime afterTime = fileInfo.LastAccessTime;

            // Assert
            beforeTime.Should().Be(DefaultTime);
            afterTime.Should().Be(DefaultTime);
        }

        [Fact]
        private void When_getting_file_last_access_time_in_local_zone_after_external_change_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock { UtcNow = () => DefaultTime };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime beforeTime = fileInfo.LastAccessTime;

            fileSystem.File.SetLastAccessTime(path, AlternateTime);

            // Act
            fileInfo.Refresh();

            // Assert
            DateTime afterTime = fileInfo.LastAccessTime;

            beforeTime.Should().Be(DefaultTime);
            afterTime.Should().Be(AlternateTime);
        }

        [Fact]
        private void When_changing_file_last_access_time_in_local_zone_it_must_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock { UtcNow = () => DefaultTime };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime beforeTime = fileInfo.LastAccessTime;

            // Act
            fileInfo.LastAccessTime = AlternateTime;

            // Assert
            DateTime afterTime = fileInfo.LastAccessTime;

            beforeTime.Should().Be(DefaultTime);
            afterTime.Should().Be(AlternateTime);
        }
    }
}
