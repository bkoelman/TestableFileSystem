using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FileEntry : BaseEntry
    {
        private const int DefaultBufferSize = 4096;

        private const FileAttributes FileAttributesToDiscard = FileAttributes.Directory | FileAttributes.Device |
            FileAttributes.Normal | FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed |
            FileAttributes.Encrypted | FileAttributes.IntegrityStream;

        [NotNull]
        private static readonly char[] FileNameCharsInvalid = Path.GetInvalidFileNameChars();

        [NotNull]
        [ItemNotNull]
        private readonly List<byte[]> blocks = new List<byte[]>();

        public long Length { get; private set; }

        [CanBeNull]
        private MemoryFileStream activeWriter;

        [NotNull]
        [ItemNotNull]
        private readonly IList<MemoryFileStream> activeReaders = new List<MemoryFileStream>();

        [NotNull]
        private readonly object readerWriterLock = new object();

        private long creationTimeStampUtc;
        private long lastWriteTimeStampUtc;
        private long lastAccessTimeStampUtc;

        [NotNull]
        public DirectoryEntry Parent { get; private set; }

        public override DateTime CreationTime
        {
            get
            {
                return DateTime.FromFileTime(creationTimeStampUtc);
            }
            set
            {
                creationTimeStampUtc = value.ToFileTime();
            }
        }

        public override DateTime CreationTimeUtc
        {
            get
            {
                return DateTime.FromFileTimeUtc(creationTimeStampUtc);
            }
            set
            {
                creationTimeStampUtc = value.ToFileTimeUtc();
            }
        }

        public override DateTime LastWriteTime
        {
            get
            {
                return DateTime.FromFileTime(lastWriteTimeStampUtc);
            }
            set
            {
                lastWriteTimeStampUtc = value.ToFileTime();
            }
        }

        public override DateTime LastWriteTimeUtc
        {
            get
            {
                return DateTime.FromFileTimeUtc(lastWriteTimeStampUtc);
            }
            set
            {
                lastWriteTimeStampUtc = value.ToFileTimeUtc();
            }
        }

        public override DateTime LastAccessTime
        {
            get
            {
                return DateTime.FromFileTime(lastAccessTimeStampUtc);
            }
            set
            {
                lastAccessTimeStampUtc = value.ToFileTime();
            }
        }

        public override DateTime LastAccessTimeUtc
        {
            get
            {
                return DateTime.FromFileTimeUtc(lastAccessTimeStampUtc);
            }
            set
            {
                lastAccessTimeStampUtc = value.ToFileTimeUtc();
            }
        }

        public bool IsOpen()
        {
            lock (readerWriterLock)
            {
                return activeWriter != null || activeReaders.Any();
            }
        }

        public event EventHandler ContentChanged;

        public FileEntry([NotNull] string name, [NotNull] DirectoryEntry parent)
            : base(name)
        {
            Guard.NotNull(parent, nameof(parent));
            AssertParentIsValid(parent);

            Parent = parent;
            Attributes = FileAttributes.Normal;

            CreationTimeUtc = SystemClock.UtcNow();
            HandleFileChanged(false);
        }

        [AssertionMethod]
        private void AssertParentIsValid([NotNull] DirectoryEntry parent)
        {
            if (parent.Parent == null)
            {
                throw new ArgumentException(nameof(parent));
            }
        }

        private void HandleFileChanged(bool raiseChangeEvent)
        {
            HandleFileAccessed();
            LastWriteTimeUtc = LastAccessTimeUtc;

            if (raiseChangeEvent)
            {
                ContentChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void HandleFileAccessed()
        {
            LastAccessTimeUtc = SystemClock.UtcNow();
        }

        [NotNull]
        public string GetAbsolutePath()
        {
            return Path.Combine(Parent.GetAbsolutePath(), Name);
        }

        [NotNull]
        public IFileStream Open(FileMode mode, FileAccess access, int bufferSize = DefaultBufferSize)
        {
            if (bufferSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            bool isReader = access == FileAccess.Read;
            bool truncate = false;
            bool seekToEnd = false;

            switch (mode)
            {
                case FileMode.CreateNew:
                case FileMode.Create:
                case FileMode.Truncate:
                    truncate = true;
                    isReader = false;
                    break;
                case FileMode.Append:
                    seekToEnd = true;
                    isReader = false;
                    break;
            }

            MemoryFileStream stream;

            lock (readerWriterLock)
            {
                if (activeWriter != null)
                {
                    throw GetErrorForFileInUse();
                }

                if (!isReader && activeReaders.Any())
                {
                    throw GetErrorForFileInUse();
                }

                stream = new MemoryFileStream(this, isReader, bufferSize);

                if (isReader)
                {
                    activeReaders.Add(stream);
                }
                else
                {
                    activeWriter = stream;
                }
            }

            if (seekToEnd)
            {
                stream.Seek(0, SeekOrigin.End);
            }

            if (truncate)
            {
                stream.SetLength(0);
            }

            return new StreamWrapper(stream, Name);
        }

        public void MoveTo([NotNull] string newName, [NotNull] DirectoryEntry newParent)
        {
            Guard.NotNullNorWhiteSpace(newName, nameof(newName));
            Guard.NotNull(newParent, nameof(newParent));

            AssertParentIsValid(newParent);

            Name = newName;
            Parent = newParent;
        }

        [NotNull]
        private Exception GetErrorForFileInUse()
        {
            string path = GetAbsolutePath();
            return ErrorFactory.FileIsInUse(path);
        }

        private void CloseStream([NotNull] MemoryFileStream stream)
        {
            lock (readerWriterLock)
            {
                if (activeWriter == stream)
                {
                    activeWriter = null;
                }

                activeReaders.Remove(stream);
            }
        }

        public override string ToString()
        {
            return $"File: {Name} ({Length} bytes)";
        }

        [AssertionMethod]
        protected override void AssertNameIsValid(string name)
        {
            foreach (char ch in name)
            {
                if (FileNameCharsInvalid.Contains(ch))
                {
                    throw ErrorFactory.PathIsNotLegal(nameof(name));
                }
            }
        }

        protected override FileAttributes FilterAttributes(FileAttributes attributes)
        {
            var filtered = attributes & ~(FileAttributes.Normal | FileAttributesToDiscard);
            return filtered == 0 ? FileAttributes.Normal : filtered;
        }

        private sealed class MemoryFileStream : Stream
        {
            [NotNull]
            private readonly FileEntry owner;

            private readonly int blockSize;
            private long position;
            private bool isClosed;

            public override bool CanRead => true;

            public override bool CanSeek => true;

            public override bool CanWrite { get; }

            public override long Length => owner.Length;

            public override long Position
            {
                get
                {
                    return position;
                }
                set
                {
                    AssertNotClosed();

                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }

                    if (value > Length)
                    {
                        SetLength(value);
                    }

                    position = value;
                }
            }

            public MemoryFileStream([NotNull] FileEntry owner, bool isReader, int blockSize)
            {
                Guard.NotNull(owner, nameof(owner));

                this.owner = owner;
                CanWrite = !isReader;
                this.blockSize = blockSize;
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                AssertNotClosed();

                switch (origin)
                {
                    case SeekOrigin.Begin:
                        if (offset < 0)
                        {
                            throw new ArgumentOutOfRangeException(nameof(offset));
                        }

                        Position = offset;
                        break;

                    case SeekOrigin.Current:
                        if (Position + offset < 0)
                        {
                            throw new ArgumentOutOfRangeException(nameof(offset));
                        }

                        Position += offset;
                        break;

                    case SeekOrigin.End:
                        if (Length + offset < 0)
                        {
                            throw new ArgumentOutOfRangeException(nameof(offset));
                        }

                        Position = Length + offset;
                        break;
                }

                return Position;
            }

            public override void SetLength(long value)
            {
                AssertNotClosed();
                AssertIsWriteable();

                if (value == Length)
                {
                    return;
                }

                EnsureCapacity(value);
                owner.Length = value;

                if (Position > Length)
                {
                    Position = Length;
                }

                owner.HandleFileChanged(true);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                AssertNotClosed();

                var segment = new ArraySegment<byte>(buffer, offset, count);
                if (segment.Count == 0 || Position == Length)
                {
                    return 0;
                }

                int sumBytesRead = 0;

                int blockIndex = (int)(Position / blockSize);
                int offsetInCurrentBlock = (int)(Position % blockSize);

                while (count > 0 && Position < Length)
                {
                    int bytesToRead = Math.Min(blockSize - offsetInCurrentBlock, count);
                    bytesToRead = Math.Min(bytesToRead, (int)(Length - Position));

                    Buffer.BlockCopy(owner.blocks[blockIndex], offsetInCurrentBlock, buffer, offset, bytesToRead);

                    offset += bytesToRead;
                    count -= bytesToRead;

                    Position += bytesToRead;
                    sumBytesRead += bytesToRead;

                    blockIndex++;
                    offsetInCurrentBlock = 0;
                }

                owner.HandleFileAccessed();

                return sumBytesRead;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                AssertNotClosed();
                AssertIsWriteable();

                var segment = new ArraySegment<byte>(buffer, offset, count);
                if (segment.Count == 0)
                {
                    return;
                }

                EnsureCapacity(Position + count);

                int blockIndex = (int)(Position / blockSize);
                int bytesFreeInCurrentBlock = blockSize - (int)(Position % blockSize);

                long newPosition = position;

                while (count > 0)
                {
                    int bytesToWrite = Math.Min(bytesFreeInCurrentBlock, count);

                    int offsetInBlock = blockSize - bytesFreeInCurrentBlock;
                    Buffer.BlockCopy(buffer, offset, owner.blocks[blockIndex], offsetInBlock, bytesToWrite);

                    offset += bytesToWrite;
                    count -= bytesToWrite;

                    newPosition += bytesToWrite;

                    blockIndex++;
                    bytesFreeInCurrentBlock = blockSize;
                }

                Position = newPosition;
                owner.HandleFileChanged(true);
            }

            private void EnsureCapacity(long bytesNeeded)
            {
                long bytesAvailable = owner.blocks.Count * blockSize;
                while (bytesAvailable < bytesNeeded)
                {
                    owner.blocks.Add(new byte[blockSize]);
                    bytesAvailable += blockSize;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    isClosed = true;
                    owner.CloseStream(this);
                }

                base.Dispose(disposing);
            }

            private void AssertNotClosed()
            {
                if (isClosed)
                {
                    throw new ObjectDisposedException(nameof(MemoryFileStream));
                }
            }

            private void AssertIsWriteable()
            {
                if (!CanWrite)
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
