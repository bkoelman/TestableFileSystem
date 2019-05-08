using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoTimeLastWriteUtcSpecs
    {
        private static readonly DateTime DefaultTimeUtc = 1.February(2034).At(12, 34, 56).AsUtc();
        private static readonly DateTime AlternateTimeUtc = 2.February(2034).At(12, 34, 56).AsUtc();

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_file_last_write_time_in_UTC_it_must_lazy_load()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            fileSystem.File.SetLastWriteTimeUtc(path, AlternateTimeUtc);

            // Act
            DateTime time = fileInfo.LastWriteTimeUtc;

            // Assert
            time.Should().Be(AlternateTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_file_last_write_time_in_UTC_it_must_cache()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime beforeTime = fileInfo.LastWriteTimeUtc;

            fileSystem.File.SetLastWriteTimeUtc(path, AlternateTimeUtc);

            // Act
            DateTime afterTime = fileInfo.LastWriteTimeUtc;

            // Assert
            beforeTime.Should().Be(DefaultTimeUtc);
            afterTime.Should().Be(DefaultTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_file_last_write_time_in_UTC_after_external_change_it_must_update_cache_on_refresh()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime beforeTime = fileInfo.LastWriteTimeUtc;

            fileSystem.File.SetLastWriteTimeUtc(path, AlternateTimeUtc);

            // Act
            fileInfo.Refresh();

            // Assert
            DateTime afterTime = fileInfo.LastWriteTimeUtc;

            beforeTime.Should().Be(DefaultTimeUtc);
            afterTime.Should().Be(AlternateTimeUtc);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_changing_file_last_write_time_in_UTC_it_must_refresh_automatically()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            var clock = new SystemClock(() => DefaultTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime beforeTime = fileInfo.LastWriteTimeUtc;

            // Act
            fileInfo.LastWriteTimeUtc = AlternateTimeUtc;

            // Assert
            DateTime afterTime = fileInfo.LastWriteTimeUtc;

            beforeTime.Should().Be(DefaultTimeUtc);
            afterTime.Should().Be(AlternateTimeUtc);
        }
    }
}
