﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Wrappers;

namespace TestableFileSystem.Fakes
{
    // TODO: Change accessbility to 'internal' after specs replacement.
    public sealed class FileEntry : BaseEntry
    {
        private const FileAttributes FileAttributesToDiscard = FileAttributes.Directory | FileAttributes.Device |
            FileAttributes.Normal | FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed |
            FileAttributes.Encrypted | FileAttributes.IntegrityStream;

        [NotNull]
        private static readonly char[] FileNameCharsInvalid = Path.GetInvalidFileNameChars();

        [NotNull]
        [ItemNotNull]
        private readonly List<byte[]> blocks = new List<byte[]>();

        private long length;

        [CanBeNull]
        private FakeFileStream activeWriter;

        private bool deleteOnClose;

        [NotNull]
        [ItemNotNull]
        private readonly IList<FakeFileStream> activeReaders = new List<FakeFileStream>();

        [NotNull]
        private readonly object readerWriterLock = new object();

        private long creationTimeStampUtc;
        private long lastWriteTimeStampUtc;
        private long lastAccessTimeStampUtc;

        [NotNull]
        public DirectoryEntry Parent { get; private set; }

        public override DateTime CreationTime
        {
            get => DateTime.FromFileTime(creationTimeStampUtc);
            set => creationTimeStampUtc = value.ToFileTime();
        }

        public override DateTime CreationTimeUtc
        {
            get => DateTime.FromFileTimeUtc(creationTimeStampUtc);
            set => creationTimeStampUtc = value.ToFileTimeUtc();
        }

        public override DateTime LastWriteTime
        {
            get => DateTime.FromFileTime(lastWriteTimeStampUtc);
            set => lastWriteTimeStampUtc = value.ToFileTime();
        }

        public override DateTime LastWriteTimeUtc
        {
            get => DateTime.FromFileTimeUtc(lastWriteTimeStampUtc);
            set => lastWriteTimeStampUtc = value.ToFileTimeUtc();
        }

        public override DateTime LastAccessTime
        {
            get => DateTime.FromFileTime(lastAccessTimeStampUtc);
            set => lastAccessTimeStampUtc = value.ToFileTime();
        }

        public override DateTime LastAccessTimeUtc
        {
            get => DateTime.FromFileTimeUtc(lastAccessTimeStampUtc);
            set => lastAccessTimeStampUtc = value.ToFileTimeUtc();
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
                throw new ArgumentException("File cannot exist at the root of the filesystem.", nameof(parent));
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

        public bool IsOpen()
        {
            lock (readerWriterLock)
            {
                return activeWriter != null || activeReaders.Any();
            }
        }

        public void EnableDeleteOnClose()
        {
            deleteOnClose = true;
        }

        [NotNull]
        public string GetAbsolutePath()
        {
            return Path.Combine(Parent.GetAbsolutePath(), Name);
        }

        [NotNull]
        public IFileStream Open(FileMode mode, FileAccess access)
        {
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

            FakeFileStream stream;

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

                stream = new FakeFileStream(this, isReader);

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

            string path = GetAbsolutePath();
            return new FileStreamWrapper(stream, () => path, () => false, () => throw new NotSupportedException(),
                _ => stream.Flush());
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

        private void CloseStream([NotNull] FakeFileStream stream)
        {
            lock (readerWriterLock)
            {
                if (activeWriter == stream)
                {
                    activeWriter = null;
                }

                activeReaders.Remove(stream);

                if (deleteOnClose && !IsOpen())
                {
                    Parent.DeleteFile(this);
                }
            }
        }

        public override string ToString()
        {
            return $"File: {Name} ({length} bytes)";
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
            FileAttributes filtered = attributes & ~(FileAttributes.Normal | FileAttributesToDiscard);
            return filtered == 0 ? FileAttributes.Normal : filtered;
        }

        private sealed class FakeFileStream : Stream
        {
            [NotNull]
            private readonly FileEntry owner;

            private const int BlockSize = 4096;
            private long position;
            private bool isClosed;

            public override bool CanRead => true;

            public override bool CanSeek => true;

            public override bool CanWrite { get; }

            public override long Length => owner.length;

            public override long Position
            {
                get => position;
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

            public FakeFileStream([NotNull] FileEntry owner, bool isReader)
            {
                Guard.NotNull(owner, nameof(owner));

                this.owner = owner;
                CanWrite = !isReader;
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
                owner.length = value;

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

                int blockIndex = (int)(Position / BlockSize);
                int offsetInCurrentBlock = (int)(Position % BlockSize);

                while (count > 0 && Position < Length)
                {
                    int bytesToRead = Math.Min(BlockSize - offsetInCurrentBlock, count);
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

                int blockIndex = (int)(Position / BlockSize);
                int bytesFreeInCurrentBlock = BlockSize - (int)(Position % BlockSize);

                long newPosition = position;

                while (count > 0)
                {
                    int bytesToWrite = Math.Min(bytesFreeInCurrentBlock, count);

                    int offsetInBlock = BlockSize - bytesFreeInCurrentBlock;
                    Buffer.BlockCopy(buffer, offset, owner.blocks[blockIndex], offsetInBlock, bytesToWrite);

                    offset += bytesToWrite;
                    count -= bytesToWrite;

                    newPosition += bytesToWrite;

                    blockIndex++;
                    bytesFreeInCurrentBlock = BlockSize;
                }

                Position = newPosition;
                owner.HandleFileChanged(true);
            }

            private void EnsureCapacity(long bytesNeeded)
            {
                long bytesAvailable = owner.blocks.Count * BlockSize;
                while (bytesAvailable < bytesNeeded)
                {
                    owner.blocks.Add(new byte[BlockSize]);
                    bytesAvailable += BlockSize;
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
                    throw new ObjectDisposedException(nameof(FakeFileStream));
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
