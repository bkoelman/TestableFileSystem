using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Fakes.Tests.EventMonitors;
using TestableFileSystem.Fakes.Tests.Utilities;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs
{
    public sealed class FileEntrySpecs
    {
        private const int BlockSize = 4096;

        [NotNull]
        private static readonly DirectoryEntry DriveC = new DirectoryTreeBuilder()
            .IncludingDirectory("C:")
            .Build()
            .Directories["C:"];

        [Fact]
        private void When_creating_file_entry_it_must_fail_on_empty_name()
        {
            // Arrange
            const string fileName = "";

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => new FileEntry(fileName, DriveC);

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        private void When_creating_file_entry_it_must_fail_on_invalid_characters_in_name()
        {
            // Arrange
            const string fileName = ":::";

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => new FileEntry(fileName, DriveC);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_creating_file_entry_it_must_set_properties()
        {
            // Arrange
            const string fileName = "some.txt";

            DateTime time = 1.January(2017).At(22, 14);
            SystemClock.UtcNow = () => time;

            // Act
            var entry = new FileEntry(fileName, DriveC);

            // Assert
            entry.Name.Should().Be(fileName);
            entry.CreationTimeUtc.Should().Be(time);
            entry.LastWriteTimeUtc.Should().Be(time);
            entry.LastAccessTimeUtc.Should().Be(time);
            entry.Length.Should().Be(0);
            entry.Attributes.Should().Be(FileAttributes.Normal);
            entry.IsOpen().Should().Be(false);
        }

        [Fact]
        private void When_setting_file_attributes_to_normal_it_must_succeed()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC)
            {
                Attributes = FileAttributes.ReadOnly
            };

            // Act
            entry.Attributes = FileAttributes.Normal;

            // Assert
            entry.Attributes.Should().Be(FileAttributes.Normal);
        }

        [Fact]
        private void When_setting_file_attributes_to_all_it_must_filter_and_succeed()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            // Act
            entry.Attributes = FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System |
                FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Device | FileAttributes.Normal |
                FileAttributes.Temporary | FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed |
                FileAttributes.Offline | FileAttributes.NotContentIndexed | FileAttributes.Encrypted |
                FileAttributes.IntegrityStream | FileAttributes.NoScrubData;

            // Assert
            entry.Attributes.Should().Be(FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System |
                FileAttributes.Archive | FileAttributes.Temporary | FileAttributes.Offline | FileAttributes.NotContentIndexed |
                FileAttributes.NoScrubData);
        }

        [Fact]
        private void When_writing_small_buffer_to_file_it_must_succeed()
        {
            // Arrange
            DateTime createTime = 2.January(2017).At(22, 14);
            SystemClock.UtcNow = () => createTime;

            var entry = new FileEntry("some.txt", DriveC);

            DateTime writeTime = 3.January(2017).At(23, 11);
            SystemClock.UtcNow = () => writeTime;

            using (var monitor = new FileEntryMonitor(entry))
            {
                // Act
                using (var stream = entry.Open(FileMode.Open, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(stream.InnerStream))
                    {
                        writer.Write("ABC");
                    }
                }

                // Assert
                entry.CreationTimeUtc.Should().Be(createTime);
                entry.LastWriteTimeUtc.Should().Be(writeTime);
                entry.LastAccessTimeUtc.Should().Be(writeTime);
                entry.Length.Should().Be(3);
                monitor.HasContentChanged.Should().BeTrue();

                string contents = entry.GetFileContentsAsString();
                contents.Should().Be("ABC");
            }
        }

        [Fact]
        private void When_writing_large_buffer_to_file_it_must_succeed()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            const int size = BlockSize * 16;
            byte[] writeBuffer = CreateBuffer(size);

            // Act
            using (var stream = entry.Open(FileMode.Open, FileAccess.Write))
            {
                stream.Write(writeBuffer, 0, writeBuffer.Length);
            }

            // Assert
            entry.Length.Should().Be(size);

            string contents = BufferToHexString(entry.GetFileContentsAsByteArray());
            contents.Should().Be(BufferToHexString(writeBuffer));
        }

        [Fact]
        private void When_reading_small_buffer_from_file_it_must_succeed()
        {
            // Arrange
            SystemClock.UtcNow = () => 4.January(2015).At(12, 44);

            var entry = new FileEntry("some.txt", DriveC);

            entry.WriteToFile("ABC");

            DateTime accessTime = 5.January(2017).At(23, 11);
            SystemClock.UtcNow = () => accessTime;

            using (var stream = entry.Open(FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream.InnerStream))
                {
                    // Act
                    string content = reader.ReadToEnd();

                    // Assert
                    entry.LastAccessTimeUtc.Should().Be(accessTime);
                    entry.IsOpen().Should().Be(true);
                    content.Should().Be("ABC");
                }
            }
        }

        [Fact]
        private void When_reading_large_buffer_from_file_it_must_succeed()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            const int size = BlockSize * 16 + 1;
            byte[] writeBuffer = CreateBuffer(size);
            entry.WriteToFile(writeBuffer);

            var readBuffer = new byte[size * 2];

            using (var monitor = new FileEntryMonitor(entry))
            {
                // Act
                int count;
                using (var stream = entry.Open(FileMode.Open, FileAccess.Read))
                {
                    count = stream.Read(readBuffer, 0, readBuffer.Length);
                }

                // Assert
                count.Should().Be(writeBuffer.Length);

                string writeString = BufferToHexString(writeBuffer);
                string readString = BufferToHexString(readBuffer, 0, count);
                readString.Should().Be(writeString);

                monitor.HasContentChanged.Should().BeFalse();
            }
        }

        [Fact]
        private void When_appending_small_buffer_to_file_it_must_succeed()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            const int size = BlockSize - 5;
            string existingText = new string('X', size);
            entry.WriteToFile(existingText);

            const string textToAppend = "ZYXWVUTSRQPONMLKJIHGFEDCBA";

            // Act
            using (var stream = entry.Open(FileMode.Append, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream.InnerStream))
                {
                    writer.Write(textToAppend);
                }
            }

            // Assert
            entry.Length.Should().Be(size + textToAppend.Length);

            string contents = entry.GetFileContentsAsString();
            contents.Should().Be(existingText + textToAppend);
        }

        [Fact]
        private void When_truncating_file_it_must_succeed()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            using (var monitor = new FileEntryMonitor(entry))
            {
                entry.WriteToFile("ABC");

                // Act
                entry.Open(FileMode.Truncate, FileAccess.Read);

                // Assert
                entry.Length.Should().Be(0);
                entry.IsOpen().Should().Be(true);
                monitor.HasContentChanged.Should().BeTrue();
            }
        }

        [Fact]
        private void When_writer_is_active_it_must_fail_to_open()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            entry.Open(FileMode.Open, FileAccess.Write);

            // Act
            Action action = () => entry.Open(FileMode.Open, FileAccess.Read);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(
                @"The process cannot access the file 'C:\some.txt' because it is being used by another process.");
        }

        [Fact]
        private void When_readers_are_active_it_must_fail_to_open_for_writing()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            entry.Open(FileMode.Open, FileAccess.Read);
            entry.Open(FileMode.Open, FileAccess.Read);

            // Act
            Action action = () => entry.Open(FileMode.Open, FileAccess.Write);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(
                @"The process cannot access the file 'C:\some.txt' because it is being used by another process.");
        }

        [Fact]
        private void When_seeking_from_begin_to_before_start_it_must_fail()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            entry.WriteToFile("ABC");

            using (var stream = entry.Open(FileMode.Open, FileAccess.Write))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Seek(-1, SeekOrigin.Begin);

                // Assert
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        private void When_seeking_from_current_to_before_start_it_must_fail()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            entry.WriteToFile("ABC");

            using (var stream = entry.Open(FileMode.Open, FileAccess.Read))
            {
                stream.Seek(2, SeekOrigin.Begin);

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Seek(-3, SeekOrigin.Current);

                // Assert
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        private void When_seeking_from_end_to_before_start_it_must_fail()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            entry.WriteToFile("ABC");

            using (var stream = entry.Open(FileMode.Open, FileAccess.Read))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Seek(-4, SeekOrigin.End);

                // Assert
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        private void When_seeking_from_begin_to_past_end_it_must_add_extra_zero_bytes()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            using (var monitor = new FileEntryMonitor(entry))
            {
                entry.WriteToFile("ABC");

                DateTime accessTime = 6.January(2017).At(23, 11);
                SystemClock.UtcNow = () => accessTime;

                using (var stream = entry.Open(FileMode.Open, FileAccess.Write))
                {
                    // Act
                    stream.Seek("ABC".Length + 10, SeekOrigin.Begin);
                }

                // Assert
                entry.Length.Should().Be("ABC".Length + 10);
                monitor.HasContentChanged.Should().BeTrue();
                entry.LastAccessTimeUtc.Should().Be(accessTime);

                string contents = entry.GetFileContentsAsString();
                contents.Substring("ABC".Length).Should().Be(new string('\0', 10));
            }
        }

        [Fact]
        private void When_seeking_from_current_to_past_end_it_must_add_extra_zero_bytes()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            entry.WriteToFile("ABC");

            using (var stream = entry.Open(FileMode.Open, FileAccess.Write))
            {
                stream.Seek(1, SeekOrigin.Begin);

                // Act
                stream.Seek("BC".Length + 10, SeekOrigin.Current);
            }

            // Assert
            entry.Length.Should().Be("ABC".Length + 10);

            string contents = entry.GetFileContentsAsString();
            contents.Substring("ABC".Length).Should().Be(new string('\0', 10));
        }

        [Fact]
        private void When_seeking_from_end_to_past_end_it_must_add_extra_zero_bytes()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            entry.WriteToFile("ABC");

            using (var stream = entry.Open(FileMode.Open, FileAccess.Write))
            {
                // Act
                stream.Seek(10, SeekOrigin.End);
            }

            // Assert
            entry.Length.Should().Be("ABC".Length + 10);

            string contents = entry.GetFileContentsAsString();
            contents.Substring("ABC".Length).Should().Be(new string('\0', 10));
        }

        [Fact]
        private void When_reducing_length_it_must_succeed()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            using (var monitor = new FileEntryMonitor(entry))
            {
                entry.WriteToFile("ABCDEF");

                using (var stream = entry.Open(FileMode.Open, FileAccess.Write))
                {
                    // Act
                    stream.SetLength(3);
                }

                // Assert
                entry.Length.Should().Be(3);
                monitor.HasContentChanged.Should().BeTrue();

                string contents = entry.GetFileContentsAsString();
                contents.Should().Be("ABC");
            }
        }

        [Fact]
        private void When_seeking_in_closed_stream_it_must_fail()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            using (var stream = entry.Open(FileMode.Create, FileAccess.Write))
            {
                stream.Dispose();

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Seek(0, SeekOrigin.Begin);

                // Assert
                action.ShouldThrow<ObjectDisposedException>();
            }
        }

        [Fact]
        private void When_setting_length_in_closed_stream_it_must_fail()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            using (var stream = entry.Open(FileMode.Create, FileAccess.Write))
            {
                stream.Dispose();

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.SetLength(2);

                // Assert
                action.ShouldThrow<ObjectDisposedException>();
            }
        }

        [Fact]
        private void When_reading_from_closed_stream_it_must_fail()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            entry.WriteToFile("ABC");

            using (var stream = entry.Open(FileMode.Open, FileAccess.Read))
            {
                stream.Dispose();

                var buffer = new byte[50];

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Read(buffer, 0, buffer.Length);

                // Assert
                action.ShouldThrow<ObjectDisposedException>();
            }
        }

        [Fact]
        private void When_writing_to_closed_stream_it_must_fail()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            using (var stream = entry.Open(FileMode.Create, FileAccess.Write))
            {
                stream.Dispose();

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Write(new byte[] { 0xFF }, 0, 1);

                // Assert
                action.ShouldThrow<ObjectDisposedException>();
            }
        }

        [Fact]
        private void When_writing_to_readonly_stream_it_must_fail()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            using (var stream = entry.Open(FileMode.Open, FileAccess.Read))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Write(new byte[] { 0xFF }, 0, 1);

                // Assert
                action.ShouldThrow<NotSupportedException>();
            }
        }

        [Fact]
        private void When_seeking_past_end_of_readonly_stream_it_must_fail()
        {
            // Arrange
            var entry = new FileEntry("some.txt", DriveC);

            using (var stream = entry.Open(FileMode.Open, FileAccess.Read))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Seek(10, SeekOrigin.End);

                // Assert
                action.ShouldThrow<NotSupportedException>();
            }
        }

        [NotNull]
        private static byte[] CreateBuffer(int size)
        {
            var buffer = new byte[size];

            for (int index = 0; index < size; index++)
            {
                buffer[index] = (byte) ((index + 1) % 0xFF);
            }

            return buffer;
        }

        [NotNull]
        private static string BufferToHexString([NotNull] byte[] buffer, [CanBeNull] int? offset = null,
            [CanBeNull] int? count = null)
        {
            string hexString = BitConverter.ToString(buffer, offset ?? 0, count ?? buffer.Length);
            return hexString.Replace("-", "");
        }
    }
}
