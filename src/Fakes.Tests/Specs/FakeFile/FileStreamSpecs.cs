using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileStreamSpecs
    {
        [Fact]
        private void When_requesting_stream_for_new_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(path))
            {
                // Assert
                stream.AsStream().Should().NotBeNull();
                stream.CanRead.Should().BeTrue();
                stream.CanSeek.Should().BeTrue();
                stream.CanWrite.Should().BeTrue();
                stream.Name.Should().Be(path);
                stream.Length.Should().Be(0);
                stream.Position.Should().Be(0);
                stream.IsAsync.Should().BeFalse();
            }
        }

        [Fact]
        private void When_requesting_stream_for_existing_file_in_readonly_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "DATA")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Assert
                stream.AsStream().Should().NotBeNull();
                stream.CanRead.Should().BeTrue();
                stream.CanSeek.Should().BeTrue();
                stream.CanWrite.Should().BeFalse();
                stream.Name.Should().Be(path);
                stream.Length.Should().Be(4);
                stream.Position.Should().Be(0);
                stream.IsAsync.Should().BeFalse();
            }
        }

        [Fact]
        private void When_requesting_stream_for_existing_file_in_append_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "DATA")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Append))
            {
                // Assert
                stream.AsStream().Should().NotBeNull();
                stream.CanRead.Should().BeFalse();
                stream.CanSeek.Should().BeTrue();
                stream.CanWrite.Should().BeTrue();
                stream.Name.Should().Be(path);
                stream.Length.Should().Be(4);
                stream.Position.Should().Be(4);
                stream.IsAsync.Should().BeFalse();
            }
        }

        [Fact]
        private void When_requesting_stream_for_existing_file_in_truncate_mode_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "DATA")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Truncate))
            {
                // Assert
                stream.AsStream().Should().NotBeNull();
                stream.CanRead.Should().BeTrue();
                stream.CanSeek.Should().BeTrue();
                stream.CanWrite.Should().BeTrue();
                stream.Name.Should().Be(path);
                stream.Length.Should().Be(0);
                stream.Position.Should().Be(0);
                stream.IsAsync.Should().BeFalse();
            }
        }

        [Fact]
        private void When_writing_empty_buffer_to_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (IFileStream stream = fileSystem.File.Create(path))
            {
                // Act
                stream.Write(BufferFactory.SingleByte(0xAA), 0, 0);

                // Assert
                stream.Length.Should().Be(0);
                stream.Position.Should().Be(0);
            }
        }

        [Fact]
        private void When_writing_small_buffer_to_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (IFileStream stream = fileSystem.File.Create(path, 128))
            {
                var writer = new StreamWriter(stream.AsStream());

                // Act
                writer.Write("ABC");
                writer.Flush();

                // Assert
                stream.Length.Should().Be(3);
                stream.Position.Should().Be(3);
            }

            string contents = fileSystem.File.ReadAllText(path);
            contents.Should().Be("ABC");
        }

        [Fact]
        private void When_writing_large_buffer_to_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            const int size = 4096 * 16;
            byte[] writeBuffer = CreateBuffer(size);

            using (IFileStream stream = fileSystem.File.Create(path, 2048))
            {
                // Act
                stream.Write(writeBuffer, 0, writeBuffer.Length);

                // Assert
                stream.Length.Should().Be(size);
                stream.Position.Should().Be(size);
            }

            byte[] contents = fileSystem.File.ReadAllBytes(path);
            contents.SequenceEqual(writeBuffer).Should().BeTrue();
        }

        [Fact]
        private void When_async_writing_buffer_to_file_using_TPL_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";
            const string content = "Some-file-contents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            byte[] buffer = Encoding.ASCII.GetBytes(content);
            bool hasCompleted;

            using (IFileStream stream = fileSystem.File.Create(path))
            {
                // Act
                Task task = stream.WriteAsync(buffer, 0, buffer.Length);
                hasCompleted = task.Wait(TimeSpan.FromSeconds(1));
            }

            // Assert
            hasCompleted.Should().BeTrue();
            fileSystem.File.ReadAllText(path).Should().Be(content);
        }

#if !NETCOREAPP1_1
        [Fact]
        private void When_async_writing_buffer_to_file_using_APM_without_callback_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";
            const string content = "Some-file-contents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            byte[] buffer = Encoding.ASCII.GetBytes(content);
            IAsyncResult asyncResult;

            using (IFileStream stream = fileSystem.File.Create(path))
            {
                // Act
                asyncResult = stream.BeginWrite(buffer, 0, buffer.Length, null, null);
                stream.EndWrite(asyncResult);
            }

            // Assert
            asyncResult.IsCompleted.Should().BeTrue();
            fileSystem.File.ReadAllText(path).Should().Be(content);
        }

        [Fact]
        private void When_async_writing_buffer_to_file_using_APM_with_callback_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";
            const string content = "Some-file-contents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            byte[] buffer = Encoding.ASCII.GetBytes(content);
            IAsyncResult outerAsyncResult;
            Exception error = null;
            var completionWaitHandle = new AutoResetEvent(false);
            bool wasSignaled;

            using (IFileStream stream = fileSystem.File.Create(path))
            {
                // Act
                outerAsyncResult = stream.BeginWrite(buffer, 0, buffer.Length, WriteCompleted, null);

                void WriteCompleted(IAsyncResult innerAsyncResult)
                {
                    try
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        stream.EndWrite(innerAsyncResult);
                    }
                    catch (Exception exception)
                    {
                        error = exception;
                    }

                    completionWaitHandle.Set();
                }

                wasSignaled = completionWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
            }

            // Assert
            wasSignaled.Should().BeTrue();
            outerAsyncResult.IsCompleted.Should().BeTrue();
            error.Should().BeNull();
            fileSystem.File.ReadAllText(path).Should().Be(content);
        }
#endif

        [Fact]
        private void When_reading_small_buffer_from_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "DATA")
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                var reader = new StreamReader(stream.AsStream());

                // Act
                string contents = reader.ReadToEnd();

                // Assert
                stream.Length.Should().Be(4);
                stream.Position.Should().Be(4);
                contents.Should().Be("DATA");
            }
        }

        [Fact]
        private void When_reading_large_buffer_from_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            const int size = 4096 * 16;
            byte[] contents = CreateBuffer(size);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, contents)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[size * 2];

                // Act
                int numBytesRead = stream.Read(buffer, 0, buffer.Length);

                // Assert
                stream.Length.Should().Be(size);
                stream.Position.Should().Be(size);

                numBytesRead.Should().Be(size);
                buffer.Take(size).SequenceEqual(contents).Should().BeTrue();
            }
        }

        [Fact]
        private void When_async_reading_buffer_from_file_using_TPL_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";
            const string content = "Some-file-contents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, content)
                .Build();

            var buffer = new byte[1024];

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                Task<int> task = stream.ReadAsync(buffer, 0, buffer.Length);
                int numBytesRead = task.Result;

                // Assert
                numBytesRead.Should().Be(content.Length);
                new string(Encoding.ASCII.GetChars(buffer, 0, numBytesRead)).Should().Be(content);
            }
        }

#if !NETCOREAPP1_1
        [Fact]
        private void When_async_reading_buffer_from_file_using_APM_without_callback_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";
            const string content = "Some-file-contents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, content)
                .Build();

            var buffer = new byte[1024];

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                IAsyncResult asyncResult = stream.BeginRead(buffer, 0, buffer.Length, null, null);
                int numBytesRead = stream.EndRead(asyncResult);

                // Assert
                asyncResult.IsCompleted.Should().BeTrue();
                numBytesRead.Should().Be(content.Length);
                new string(Encoding.ASCII.GetChars(buffer, 0, numBytesRead)).Should().Be(content);
            }
        }

        [Fact]
        private void When_async_reading_buffer_from_file_using_APM_with_callback_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";
            const string content = "Some-file-contents";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, content)
                .Build();

            var buffer = new byte[1024];
            int numBytesRead = -1;
            Exception error = null;
            var completionWaitHandle = new AutoResetEvent(false);

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                IAsyncResult outerAsyncResult = stream.BeginRead(buffer, 0, buffer.Length, ReadCompleted, null);

                void ReadCompleted(IAsyncResult innerAsyncResult)
                {
                    try
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        numBytesRead = stream.EndRead(innerAsyncResult);
                    }
                    catch (Exception exception)
                    {
                        error = exception;
                    }

                    completionWaitHandle.Set();
                }

                bool wasSignaled = completionWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                // Assert
                wasSignaled.Should().BeTrue();
                outerAsyncResult.IsCompleted.Should().BeTrue();
                error.Should().BeNull();
                numBytesRead.Should().Be(content.Length);
                new string(Encoding.ASCII.GetChars(buffer, 0, numBytesRead)).Should().Be(content);
            }
        }
#endif

        [Fact]
        private void When_appending_small_buffer_to_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            const int size = 4096 - 5;
            string existingText = new string('X', size);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, existingText)
                .Build();

            const string textToAppend = "ZYXWVUTSRQPONMLKJIHGFEDCBA";

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Append))
            {
                var writer = new StreamWriter(stream.AsStream());

                // Act
                writer.Write(textToAppend);
                writer.Flush();

                // Assert
                stream.Length.Should().Be(existingText.Length + textToAppend.Length);
                stream.Position.Should().Be(existingText.Length + textToAppend.Length);
            }

            string contents = fileSystem.File.ReadAllText(path);
            contents.Should().Be(existingText + textToAppend);
        }

        [Fact]
        private void When_seeking_from_begin_to_before_start_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ABC")
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Seek(-1, SeekOrigin.Begin);

                // Assert
                action.Should().Throw<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        private void When_seeking_from_current_to_before_start_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ABC")
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                stream.Seek(2, SeekOrigin.Begin);

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Seek(-3, SeekOrigin.Current);

                // Assert
                action.Should().Throw<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        private void When_seeking_from_end_to_before_start_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ABC")
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Seek(-4, SeekOrigin.End);

                // Assert
                action.Should().Throw<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        private void When_seeking_from_begin_to_past_end_it_must_add_extra_zero_bytes()
        {
            // Arrange
            const string path = @"C:\file.txt";
            const string data = "ABC";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, data)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.ReadWrite))
            {
                // Act
                stream.Seek(data.Length + 10, SeekOrigin.Begin);

                // Assert
                stream.Length.Should().Be(data.Length + 10);
            }

            string contents = fileSystem.File.ReadAllText(path);
            contents.Substring(data.Length).Should().Be(new string('\0', 10));
        }

        [Fact]
        private void When_seeking_from_current_to_past_end_it_must_add_extra_zero_bytes()
        {
            // Arrange
            const string path = @"C:\file.txt";
            const string data = "ABC";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, data)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.ReadWrite))
            {
                stream.Seek(1, SeekOrigin.Begin);

                // Act
                stream.Seek(2 + 10, SeekOrigin.Current);

                // Assert
                stream.Length.Should().Be(data.Length + 10);
            }

            string contents = fileSystem.File.ReadAllText(path);
            contents.Substring(data.Length).Should().Be(new string('\0', 10));
        }

        [Fact]
        private void When_seeking_from_end_to_past_end_it_must_add_extra_zero_bytes()
        {
            // Arrange
            const string path = @"C:\file.txt";
            const string data = "ABC";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, data)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.ReadWrite))
            {
                // Act
                stream.Seek(10, SeekOrigin.End);

                // Assert
                stream.Length.Should().Be(data.Length + 10);
            }

            string contents = fileSystem.File.ReadAllText(path);
            contents.Substring(data.Length).Should().Be(new string('\0', 10));
        }

        [Fact]
        private void When_seeking_from_end_to_past_end_of_readonly_stream_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ABC")
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Seek(10, SeekOrigin.End);

                // Assert
                action.Should().Throw<NotSupportedException>();
            }
        }

        [Fact]
        private void When_seeking_to_before_end_in_stream_opened_in_Append_mode_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ABC")
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Append))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Seek(-1, SeekOrigin.Current);

                // Assert
                action.Should().Throw<IOException>()
                    .WithMessage(
                        "Unable seek backward to overwrite data that previously existed in a file opened in Append mode.");
            }
        }

        [Fact]
        private void When_setting_position_to_negative_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ABC")
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Position = -1;

                // Assert
                action.Should().Throw<ArgumentOutOfRangeException>()
                    .WithMessage("Specified argument was out of the range of valid values.*");
            }
        }

        [Fact]
        private void When_reducing_length_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ABCDEF")
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.ReadWrite))
            {
                stream.Position = 5;

                // Act
                stream.SetLength(3);

                // Assert
                stream.Length.Should().Be(3);
                stream.Position.Should().Be(3);
            }

            string contents = fileSystem.File.ReadAllText(path);
            contents.Should().Be("ABC");
        }

        [Fact]
        private void When_seeking_in_closed_stream_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                stream.Dispose();

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Seek(0, SeekOrigin.Begin);

                // Assert
                action.Should().Throw<ObjectDisposedException>().WithMessage("Cannot access a closed file.");
            }
        }

        [Fact]
        private void When_setting_length_in_closed_stream_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                stream.Dispose();

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.SetLength(2);

                // Assert
                action.Should().Throw<ObjectDisposedException>().WithMessage("Cannot access a closed file.");
            }
        }

        [Fact]
        private void When_reading_from_closed_stream_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                stream.Dispose();

                var buffer = new byte[50];

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Read(buffer, 0, buffer.Length);

                // Assert
                action.Should().Throw<ObjectDisposedException>().WithMessage("Cannot access a closed file.");
            }
        }

        [Fact]
        private void When_writing_to_closed_stream_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.ReadWrite))
            {
                stream.Dispose();

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Write(BufferFactory.SingleByte(0xFF), 0, 1);

                // Assert
                action.Should().Throw<ObjectDisposedException>().WithMessage("Cannot access a closed file.");
            }
        }

        [Fact]
        private void When_writing_to_readonly_stream_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Write(BufferFactory.SingleByte(0xFF), 0, 1);

                // Assert
                action.Should().Throw<NotSupportedException>().WithMessage("Stream does not support writing.");
            }
        }

        [Fact]
        private void When_reading_from_writeonly_stream_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Write))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.ReadByte();

                // Assert
                action.Should().Throw<NotSupportedException>().WithMessage("Stream does not support reading.");
            }
        }

        [Fact]
        private void When_writer_is_active_it_must_fail_to_open()
        {
            // Arrange
            const string path = @"C:\some\sheet.xls";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Write))
            {
                // Act
                Action action = () => fileSystem.File.Open(path, FileMode.Open, FileAccess.Read);

                // Assert
                action.Should().Throw<IOException>().WithMessage(
                    @"The process cannot access the file 'C:\some\sheet.xls' because it is being used by another process.");
            }
        }

        [Fact]
        private void When_readers_are_active_it_must_fail_to_open_for_writing()
        {
            // Arrange
            const string path = @"C:\some\sheet.xls";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    // Act
                    Action action = () => fileSystem.File.Open(path, FileMode.Open, FileAccess.Write);

                    // Assert
                    action.Should().Throw<IOException>().WithMessage(
                        @"The process cannot access the file 'C:\some\sheet.xls' because it is being used by another process.");
                }
            }
        }

#if !NETCOREAPP1_1
        [Fact]
        private void When_locking_for_negative_position_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Lock(-1, 4096);

                // Assert
                action.Should().Throw<ArgumentOutOfRangeException>().WithMessage("Non-negative number required.*");
            }
        }

        [Fact]
        private void When_unlocking_for_negative_position_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Unlock(-1, 4096);

                // Assert
                action.Should().Throw<ArgumentOutOfRangeException>().WithMessage("Non-negative number required.*");
            }
        }

        [Fact]
        private void When_locking_for_negative_length_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Lock(0, -1);

                // Assert
                action.Should().Throw<ArgumentOutOfRangeException>().WithMessage("Non-negative number required.*");
            }
        }

        [Fact]
        private void When_unlocking_for_negative_length_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Unlock(0, -1);

                // Assert
                action.Should().Throw<ArgumentOutOfRangeException>().WithMessage("Non-negative number required.*");
            }
        }

        [Fact]
        private void When_locking_in_closed_stream_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                stream.Dispose();

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Lock(0, 1024);

                // Assert
                action.Should().Throw<ObjectDisposedException>().WithMessage("Cannot access a closed file.");
            }
        }

        [Fact]
        private void When_unlocking_in_closed_stream_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                stream.Dispose();

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Unlock(0, 1024);

                // Assert
                action.Should().Throw<ObjectDisposedException>().WithMessage("Cannot access a closed file.");
            }
        }

        [Fact]
        private void When_internally_reading_from_locked_range_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                stream.Lock(4096 * 1, 4096);

                using (var reader = new StreamReader(stream.AsStream()))
                {
                    // Act
                    reader.ReadToEnd();
                }
            }
        }

        [Fact]
        private void When_internally_writing_to_locked_range_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            byte[] buffer = CreateBuffer(8192);
            using (IFileStream stream = fileSystem.File.OpenWrite(path))
            {
                stream.Lock(0, 4096);

                // Act
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        [Fact]
        private void When_externally_reading_before_locked_range_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            var buffer = new byte[4096];
            using (IFileStream outerStream = fileSystem.File.OpenRead(path))
            {
                outerStream.Lock(4096 * 2, 4096);

                using (IFileStream innerStream = fileSystem.File.OpenRead(path))
                {
                    innerStream.Seek(4096, SeekOrigin.Begin);

                    // Act
                    innerStream.Read(buffer, 0, buffer.Length);
                }
            }
        }

        [Fact]
        private void When_externally_reading_after_locked_range_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            var buffer = new byte[4096];
            using (IFileStream outerStream = fileSystem.File.OpenRead(path))
            {
                outerStream.Lock(4096, 4096);

                using (IFileStream innerStream = fileSystem.File.OpenRead(path))
                {
                    innerStream.Seek(4096 * 2, SeekOrigin.Begin);

                    // Act
                    innerStream.Read(buffer, 0, buffer.Length);
                }
            }
        }

        [Fact]
        private void When_externally_reading_subset_of_locked_range_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            var buffer = new byte[4096];
            using (IFileStream outerStream = fileSystem.File.OpenRead(path))
            {
                outerStream.Lock(4096, 4096 * 3);

                using (IFileStream innerStream = fileSystem.File.OpenRead(path))
                {
                    innerStream.Seek(4096 * 2, SeekOrigin.Begin);

                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => innerStream.Read(buffer, 0, buffer.Length);

                    // Assert
                    action.Should().Throw<IOException>().WithMessage(
                        "The process cannot access the file because another process has locked a portion of the file");
                }
            }
        }

        [Fact]
        private void When_externally_reading_superset_of_locked_range_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            var buffer = new byte[4096 * 3];
            using (IFileStream outerStream = fileSystem.File.OpenRead(path))
            {
                outerStream.Lock(4096 * 2, 4096);

                using (IFileStream innerStream = fileSystem.File.OpenRead(path))
                {
                    innerStream.Seek(4096, SeekOrigin.Begin);

                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => innerStream.Read(buffer, 0, buffer.Length);

                    // Assert
                    action.Should().Throw<IOException>().WithMessage(
                        "The process cannot access the file because another process has locked a portion of the file");
                }
            }
        }

        [Fact]
        private void When_externally_reading_left_overlap_of_locked_range_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            var buffer = new byte[4096];
            using (IFileStream outerStream = fileSystem.File.OpenRead(path))
            {
                outerStream.Lock(0, 4096 * 3);

                using (IFileStream innerStream = fileSystem.File.OpenRead(path))
                {
                    innerStream.Seek(4096 * 2, SeekOrigin.Begin);

                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => innerStream.Read(buffer, 0, buffer.Length);

                    // Assert
                    action.Should().Throw<IOException>().WithMessage(
                        "The process cannot access the file because another process has locked a portion of the file");
                }
            }
        }

        [Fact]
        private void When_externally_reading_right_overlap_of_locked_range_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            var buffer = new byte[4096 * 3];
            using (IFileStream outerStream = fileSystem.File.OpenRead(path))
            {
                outerStream.Lock(4096 * 2, 4096);

                using (IFileStream innerStream = fileSystem.File.OpenRead(path))
                {
                    innerStream.Seek(0, SeekOrigin.Begin);

                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => innerStream.Read(buffer, 0, buffer.Length);

                    // Assert
                    action.Should().Throw<IOException>().WithMessage(
                        "The process cannot access the file because another process has locked a portion of the file");
                }
            }
        }

        [Fact]
        private void When_closing_stream_with_locks_they_must_be_released()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                stream.Lock(4096 * 1, 4096);
                stream.Lock(4096 * 3, 4096 * 2);
            }

            // Act
            fileSystem.File.ReadAllBytes(path);
        }

        [Fact]
        private void When_internally_locking_same_range_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                stream.Lock(0, 4096);

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Lock(0, 4096);

                // Assert
                action.Should().Throw<IOException>().WithMessage(
                    "The process cannot access the file because another process has locked a portion of the file");
            }
        }

        [Fact]
        private void When_externally_locking_same_range_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream outerStream = fileSystem.File.OpenRead(path))
            {
                outerStream.Lock(0, 4096);

                using (IFileStream innerStream = fileSystem.File.OpenRead(path))
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => innerStream.Lock(0, 4096);

                    // Assert
                    action.Should().Throw<IOException>().WithMessage(
                        "The process cannot access the file because another process has locked a portion of the file");
                }
            }
        }

        [Fact]
        private void When_internally_locking_overlapping_range_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                stream.Lock(0, 4096);

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Lock(1024, 4096);

                // Assert
                action.Should().Throw<IOException>().WithMessage(
                    "The process cannot access the file because another process has locked a portion of the file");
            }
        }

        [Fact]
        private void When_externally_locking_overlapping_range_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream outerStream = fileSystem.File.OpenRead(path))
            {
                outerStream.Lock(0, 4096);

                using (IFileStream innerStream = fileSystem.File.OpenRead(path))
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => innerStream.Lock(1024, 4096);

                    // Assert
                    action.Should().Throw<IOException>().WithMessage(
                        "The process cannot access the file because another process has locked a portion of the file");
                }
            }
        }

        [Fact]
        private void When_internally_locking_non_overlapping_ranges_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                stream.Lock(0, 4096);

                // Act
                stream.Lock(4096, 4096);
                stream.Lock(4096 * 3, 4096);
            }
        }

        [Fact]
        private void When_externally_locking_non_overlapping_ranges_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream outerStream = fileSystem.File.OpenRead(path))
            {
                outerStream.Lock(0, 4096);

                using (IFileStream innerStream = fileSystem.File.OpenRead(path))
                {
                    // Act
                    innerStream.Lock(4096, 4096);
                    outerStream.Lock(4096 * 3, 4096);
                }
            }
        }

        [Fact]
        private void When_internally_locking_past_end_of_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                // Act
                stream.Lock(4096 * 16, 4096);
            }
        }

        [Fact]
        private void When_internally_unlocking_locked_range_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream outerStream = fileSystem.File.OpenRead(path))
            {
                outerStream.Lock(4096, 4096);

                // Act
                outerStream.Unlock(4096, 4096);

                using (IFileStream innerStream = fileSystem.File.OpenRead(path))
                {
                    using (var reader = new StreamReader(innerStream.AsStream()))
                    {
                        reader.ReadToEnd();
                    }
                }
            }
        }

        [Fact]
        private void When_externally_unlocking_locked_range_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream outerStream = fileSystem.File.OpenRead(path))
            {
                outerStream.Lock(4096, 4096);

                using (IFileStream innerStream = fileSystem.File.OpenRead(path))
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => innerStream.Unlock(4096, 4096);

                    // Assert
                    action.Should().Throw<IOException>().WithMessage("The segment is already unlocked");
                }
            }
        }

        [Fact]
        private void When_internally_unlocking_locked_range_multiple_times_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                stream.Lock(4096, 4096);
                stream.Unlock(4096, 4096);

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Unlock(4096, 4096);

                // Assert
                action.Should().Throw<IOException>().WithMessage("The segment is already unlocked");
            }
        }

        [Fact]
        private void When_internally_unlocking_overlapping_range_it_must_fail()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingBinaryFile(path, CreateBuffer(4096 * 8))
                .Build();

            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                stream.Lock(0, 4096);
                stream.Lock(4096, 4096);

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => stream.Unlock(0, 4096 * 2);

                // Assert
                action.Should().Throw<IOException>().WithMessage("The segment is already unlocked");
            }
        }
#endif

        [NotNull]
        private static byte[] CreateBuffer(int size)
        {
            var buffer = new byte[size];

            for (int index = 0; index < size; index++)
            {
                buffer[index] = (byte)((index + 1) % 0xFF);
            }

            return buffer;
        }
    }
}
