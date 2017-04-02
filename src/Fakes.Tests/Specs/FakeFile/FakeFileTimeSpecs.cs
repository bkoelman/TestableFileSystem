using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FakeFileTimeSpecs
    {
        private static readonly DateTime ZeroFileTime = 1.January(1601).AsUtc().ToLocalTime();
        private static readonly DateTime ZeroFileTimeUtc = 1.January(1601).AsUtc();

        [Fact]
        private void When_getting_file_creation_time_for_missing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            DateTime creationTime = fileSystem.File.GetCreationTime(path);
            DateTime creationTimeUtc = fileSystem.File.GetCreationTimeUtc(path);

            // Assert
            creationTime.Should().Be(ZeroFileTime);
            creationTimeUtc.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_file_creation_time_in_local_timezone_it_must_succeed()
        {
            // Arrange
            DateTime creationTime = 21.January(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetCreationTime(path, creationTime);

            // Assert
            fileSystem.File.GetCreationTime(path).Should().Be(creationTime);
            fileSystem.File.GetCreationTimeUtc(path).Should().NotBe(creationTime);
        }

        [Fact]
        private void When_setting_file_creation_time_in_utc_it_must_succeed()
        {
            // Arrange
            DateTime creationTimeUtc = 21.January(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetCreationTimeUtc(path, creationTimeUtc);

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetCreationTime(path).Should().NotBe(creationTimeUtc);
        }

        [Fact]
        private void When_getting_file_last_write_time_for_missing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            DateTime lastWriteTime = fileSystem.File.GetLastWriteTime(path);
            DateTime lastWriteTimeUtc = fileSystem.File.GetLastWriteTimeUtc(path);

            // Assert
            lastWriteTime.Should().Be(ZeroFileTime);
            lastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_file_last_write_time_in_local_timezone_it_must_succeed()
        {
            // Arrange
            DateTime lastWriteTime = 22.January(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTime(path, lastWriteTime);

            // Assert
            fileSystem.File.GetLastWriteTime(path).Should().Be(lastWriteTime);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().NotBe(lastWriteTime);
        }

        [Fact]
        private void When_setting_file_last_write_time_in_utc_it_must_succeed()
        {
            // Arrange
            DateTime lastWriteTimeUtc = 22.January(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.File.GetLastWriteTime(path).Should().NotBe(lastWriteTimeUtc);
        }

        [Fact]
        private void When_getting_file_last_access_time_for_missing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            DateTime lastAccessTime = fileSystem.File.GetLastAccessTime(path);
            DateTime lastAccessTimeUtc = fileSystem.File.GetLastAccessTimeUtc(path);

            // Assert
            lastAccessTime.Should().Be(ZeroFileTime);
            lastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_file_last_access_time_in_local_timezone_it_must_succeed()
        {
            // Arrange
            DateTime lastAccessTime = 23.January(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastAccessTime(path, lastAccessTime);

            // Assert
            fileSystem.File.GetLastAccessTime(path).Should().Be(lastAccessTime);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().NotBe(lastAccessTime);
        }

        [Fact]
        private void When_setting_file_last_access_time_in_utc_it_must_succeed()
        {
            // Arrange
            DateTime lastAccessTimeUtc = 23.January(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

            // Assert
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(lastAccessTimeUtc);
            fileSystem.File.GetLastAccessTime(path).Should().NotBe(lastAccessTimeUtc);
        }
    }
}
