using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoTimeCreationUtcSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime AlternateTimeUtc = 2.February(2034).At(12, 34, 56).AsUtc();

        [Fact]
        private void When_getting_creation_time_in_UTC_it_must_lazy_load()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            fileSystem.File.SetCreationTimeUtc(path, AlternateTimeUtc);

            // Act
            DateTime time = fileInfo.CreationTimeUtc;

            // Assert
            time.Should().Be(AlternateTimeUtc);
        }

        [Fact]
        private void When_getting_creation_time_in_UTC_it_must_cache()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime beforeTime = fileInfo.CreationTimeUtc;

            fileSystem.File.SetCreationTimeUtc(path, AlternateTimeUtc);

            // Act
            DateTime afterTime = fileInfo.CreationTimeUtc;

            // Assert
            beforeTime.Should().Be(DefaultTimeUtc);
            afterTime.Should().Be(DefaultTimeUtc);
        }

        [Fact]
        private void When_getting_creation_time_in_UTC_after_external_change_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime beforeTime = fileInfo.CreationTimeUtc;

            fileSystem.File.SetCreationTimeUtc(path, AlternateTimeUtc);

            // Act
            fileInfo.Refresh();

            // Assert
            DateTime afterTime = fileInfo.CreationTimeUtc;

            beforeTime.Should().Be(DefaultTimeUtc);
            afterTime.Should().Be(AlternateTimeUtc);
        }

        [Fact]
        private void When_changing_creation_time_in_UTC_it_must_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock { UtcNow = () => DefaultTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime beforeTime = fileInfo.CreationTimeUtc;

            // Act
            fileInfo.CreationTimeUtc = AlternateTimeUtc;

            // Assert
            DateTime afterTime = fileInfo.CreationTimeUtc;

            beforeTime.Should().Be(DefaultTimeUtc);
            afterTime.Should().Be(AlternateTimeUtc);
        }
    }
}
