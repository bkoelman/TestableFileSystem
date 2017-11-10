using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoTimeLastAccessUtcSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime AlternateTimeUtc = 2.February(2034).At(12, 34, 56).AsUtc();

        [Fact]
        private void When_getting_directory_last_access_time_in_UTC_it_must_lazy_load()
        {
            // Arrange
            const string path = @"c:\some\folder";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            fileSystem.Directory.SetLastAccessTimeUtc(path, AlternateTimeUtc);

            // Act
            DateTime time = dirInfo.LastAccessTimeUtc;

            // Assert
            time.Should().Be(AlternateTimeUtc);
        }

        [Fact]
        private void When_getting_directory_last_access_time_in_UTC_it_must_cache()
        {
            // Arrange
            const string path = @"c:\some\folder";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            DateTime beforeTime = dirInfo.LastAccessTimeUtc;

            fileSystem.Directory.SetLastAccessTimeUtc(path, AlternateTimeUtc);

            // Act
            DateTime afterTime = dirInfo.LastAccessTimeUtc;

            // Assert
            beforeTime.Should().Be(DefaultTimeUtc);
            afterTime.Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_getting_directory_last_access_time_in_UTC_after_external_change_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\folder";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            DateTime beforeTime = dirInfo.LastAccessTimeUtc;

            fileSystem.Directory.SetLastAccessTimeUtc(path, AlternateTimeUtc);

            // Act
            dirInfo.Refresh();

            // Assert
            DateTime afterTime = dirInfo.LastAccessTimeUtc;

            beforeTime.Should().Be(DefaultTimeUtc);
            afterTime.Should().Be(AlternateTimeUtc);
        }

        [Fact]
        private void When_changing_directory_last_access_time_in_UTC_it_must_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\folder";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            DateTime beforeTime = dirInfo.LastAccessTimeUtc;

            // Act
            dirInfo.LastAccessTimeUtc = AlternateTimeUtc;

            // Assert
            DateTime afterTime = dirInfo.LastAccessTimeUtc;

            beforeTime.Should().Be(DefaultTimeUtc);
            afterTime.Should().Be(AlternateTimeUtc);
        }
    }
}
