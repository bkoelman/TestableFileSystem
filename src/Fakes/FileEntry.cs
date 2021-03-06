﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;
using TestableFileSystem.Wrappers;

namespace TestableFileSystem.Fakes
{
    internal sealed class FileEntry : BaseEntry
    {
        private const FileAttributes FileAttributesToDiscard = FileAttributes.Directory | FileAttributes.Device |
            FileAttributes.Normal | FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed |
            FileAttributes.Encrypted | FileAttributes.IntegrityStream;

        [NotNull]
        [ItemNotNull]
        private List<byte[]> blocks = new List<byte[]>();

        public long Size { get; private set; }

        private bool deleteOnClose;

        [CanBeNull]
        private FakeFileStream activeWriter;

        [NotNull]
        [ItemNotNull]
        private readonly IList<FakeFileStream> activeReaders = new List<FakeFileStream>();

        [NotNull]
        private readonly object readerWriterLock = new object();

        [NotNull]
        private readonly LockTracker lockTracker;

        internal override IPathFormatter PathFormatter { get; }

        [NotNull]
        public DirectoryEntry Parent { get; private set; }

        public FileEntry([NotNull] string name, [NotNull] DirectoryEntry parent)
            : base(name, FileAttributes.Archive, parent.ChangeTracker, parent.LoggedOnAccount)
        {
            Guard.NotNull(parent, nameof(parent));

            Parent = parent;
            PathFormatter = new FileEntryPathFormatter(this);
            lockTracker = new LockTracker(this);

            if (parent.IsEncrypted)
            {
                SetEncrypted();
            }

            CreationTimeUtc = LastWriteTimeUtc = LastAccessTimeUtc = parent.SystemClock.UtcNow();
        }

        private void HandleFileContentsAccessed(FileAccessKinds accessKinds, bool notifyTracker)
        {
            DateTime utcNow = Parent.SystemClock.UtcNow();

            if (accessKinds != FileAccessKinds.None)
            {
                LastAccessTimeUtc = utcNow;
            }

            if (accessKinds.HasFlag(FileAccessKinds.Write) || accessKinds.HasFlag(FileAccessKinds.Resize))
            {
                LastWriteTimeUtc = utcNow;
            }

            if (notifyTracker)
            {
                ChangeTracker.NotifyContentsAccessed(PathFormatter, accessKinds);
            }
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
        public IFileStream Open(FileMode mode, FileAccess access, [NotNull] AbsolutePath path, bool isNewlyCreated, bool isAsync,
            bool notifyTracker)
        {
            Guard.NotNull(path, nameof(path));

            bool isReaderOnly = access == FileAccess.Read;
            bool truncate = false;
            bool seekToEnd = false;

            switch (mode)
            {
                case FileMode.CreateNew:
                case FileMode.Create:
                case FileMode.Truncate:
                {
                    truncate = true;
                    isReaderOnly = false;
                    break;
                }
                case FileMode.Append:
                {
                    seekToEnd = true;
                    isReaderOnly = false;
                    break;
                }
            }

            FakeFileStream stream = CreateStream(path, access, isReaderOnly, notifyTracker);
            InitializeStream(stream, seekToEnd, truncate, isNewlyCreated);

            return new FileStreamWrapper(stream, path.GetText, () => isAsync, stream.GetSafeFileHandle, _ => stream.Flush(),
                stream.Lock, stream.Unlock);
        }

        [NotNull]
        private FakeFileStream CreateStream([NotNull] AbsolutePath path, FileAccess access, bool isReaderOnly, bool notifyTracker)
        {
            lock (readerWriterLock)
            {
                if (activeWriter != null)
                {
                    throw ErrorFactory.System.FileIsInUse(path.GetText());
                }

                if (!isReaderOnly && activeReaders.Any())
                {
                    throw ErrorFactory.System.FileIsInUse(path.GetText());
                }

                var stream = new FakeFileStream(this, access, notifyTracker);

                if (isReaderOnly)
                {
                    activeReaders.Add(stream);
                }
                else
                {
                    activeWriter = stream;
                }

                return stream;
            }
        }

        private static void InitializeStream([NotNull] FakeFileStream stream, bool seekToEnd, bool truncate, bool isNewlyCreated)
        {
            if (seekToEnd)
            {
                stream.Seek(0, SeekOrigin.End);
                stream.SetAppendOffsetToCurrentPosition();
            }

            if (truncate)
            {
                stream.SetLength(0);

                if (!isNewlyCreated)
                {
                    stream.EnableAccessKinds(FileAccessKinds.WriteRead);
                }
            }
        }

        public void MoveTo([NotNull] string newName, [NotNull] DirectoryEntry newParent)
        {
            Guard.NotNullNorWhiteSpace(newName, nameof(newName));
            Guard.NotNull(newParent, nameof(newParent));

            Name = newName;
            Parent = newParent;
        }

        public void TransferFrom([NotNull] FileEntry otherFile)
        {
            Guard.NotNull(otherFile, nameof(otherFile));

            TransferContentsFrom(otherFile);
            CopyMetadataFrom(otherFile);
        }

        private void TransferContentsFrom([NotNull] FileEntry otherFile)
        {
            Guard.NotNull(otherFile, nameof(otherFile));

            blocks = otherFile.blocks;
            Size = otherFile.Size;
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
                    Parent.DeleteFile(Name, true);
                }
            }
        }

        public override string ToString()
        {
            return $"File: {Name} ({Size} bytes)";
        }

        protected override FileAttributes FilterAttributes(FileAttributes attributes)
        {
            FileAttributes filtered = attributes & ~(FileAttributes.Normal | FileAttributesToDiscard);
            return filtered == 0 ? FileAttributes.Normal : filtered;
        }

        private sealed class FakeFileStream : Stream
        {
            private const int BlockSize = 4096;

            [NotNull]
            private readonly FileEntry owner;

            [NotNull]
            [ItemNotNull]
            public readonly Lazy<SafeFileHandle> LazyHandle =
                new Lazy<SafeFileHandle>(() => new SafeFileHandle((IntPtr)(-2), true),
                    LazyThreadSafetyMode.ExecutionAndPublication);

            private readonly bool notifyTracker;

            private long absolutePosition;
            private FileAccessKinds accessKinds = FileAccessKinds.None;
            private readonly FileAccess fileAccess;
            private bool isClosed;

            [CanBeNull]
            private long? appendOffset;

            [CanBeNull]
            private long? newLength;

            public override bool CanRead => fileAccess.HasFlag(FileAccess.Read) && !isClosed;

            public override bool CanWrite => fileAccess.HasFlag(FileAccess.Write) && !isClosed;

            public override bool CanSeek => !isClosed;

            public override long Length
            {
                get
                {
                    AssertNotClosed();
                    return newLength ?? owner.Size;
                }
            }

            public override long Position
            {
                get
                {
                    AssertNotClosed();
                    return absolutePosition;
                }
                set
                {
                    AssertNotClosed();
                    AssertIsNotNegative(value, nameof(value));

                    if (appendOffset != null && value < appendOffset)
                    {
                        throw ErrorFactory.System.CannotSeekToPositionBeforeAppend();
                    }

                    absolutePosition = value;
                }
            }

            public FakeFileStream([NotNull] FileEntry owner, FileAccess access, bool notifyTracker)
            {
                Guard.NotNull(owner, nameof(owner));

                this.owner = owner;
                fileAccess = access;
                this.notifyTracker = notifyTracker;
            }

            [NotNull]
            public SafeFileHandle GetSafeFileHandle()
            {
                AssertNotClosed();
                return LazyHandle.Value;
            }

            public void SetAppendOffsetToCurrentPosition()
            {
                appendOffset = Position;
            }

            public void EnableAccessKinds(FileAccessKinds kinds)
            {
                accessKinds |= kinds;
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
                    {
                        if (offset < 0)
                        {
                            throw new ArgumentOutOfRangeException(nameof(offset));
                        }

                        Position = offset;
                        break;
                    }
                    case SeekOrigin.Current:
                    {
                        if (Position + offset < 0)
                        {
                            throw new ArgumentOutOfRangeException(nameof(offset));
                        }

                        Position += offset;
                        break;
                    }
                    case SeekOrigin.End:
                    {
                        if (Length + offset < 0)
                        {
                            throw new ArgumentOutOfRangeException(nameof(offset));
                        }

                        Position = Length + offset;
                        break;
                    }
                    default:
                    {
                        throw ErrorFactory.Internal.EnumValueUnsupported(origin);
                    }
                }

                return Position;
            }

            public override void SetLength(long value)
            {
                AssertIsNotNegative(value, nameof(value));
                AssertNotClosed();
                AssertIsWritable();

                if (value == Length)
                {
                    return;
                }

                EnsureCapacity(value);
                newLength = value;

                if (Position > Length)
                {
                    Position = Length;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                AssertNotClosed();
                AssertIsReadable();

                var segment = new ArraySegment<byte>(buffer, offset, count);
                if (segment.Count == 0 || Position >= Length)
                {
                    return 0;
                }

                lock (owner.readerWriterLock)
                {
                    UnsafeAssertRangeNotLocked(Position + offset, count);

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

                    accessKinds |= FileAccessKinds.Read;
                    return sumBytesRead;
                }
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                AssertNotClosed();
                AssertIsWritable();

                var segment = new ArraySegment<byte>(buffer, offset, count);
                if (segment.Count == 0)
                {
                    return;
                }

                EnsureCapacity(Position + count);

                int blockIndex = (int)(Position / BlockSize);
                int bytesFreeInCurrentBlock = BlockSize - (int)(Position % BlockSize);

                while (count > 0)
                {
                    int bytesToWrite = Math.Min(bytesFreeInCurrentBlock, count);

                    int offsetInBlock = BlockSize - bytesFreeInCurrentBlock;
                    Buffer.BlockCopy(buffer, offset, owner.blocks[blockIndex], offsetInBlock, bytesToWrite);

                    offset += bytesToWrite;
                    count -= bytesToWrite;

                    Position += bytesToWrite;

                    blockIndex++;
                    bytesFreeInCurrentBlock = BlockSize;
                }

                if (Position > Length)
                {
                    newLength = Position;
                }

                accessKinds |= FileAccessKinds.WriteRead;
            }

            private void EnsureCapacity(long bytesNeeded)
            {
                if (Length != bytesNeeded)
                {
                    if (!owner.Parent.Root.TryAllocateSpace(bytesNeeded - Length))
                    {
                        throw ErrorFactory.System.NotEnoughSpaceOnDisk();
                    }
                }

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
                    if (!isClosed)
                    {
                        isClosed = true;

                        if (newLength != null && newLength.Value != owner.Size)
                        {
                            owner.Size = newLength.Value;
                            accessKinds |= FileAccessKinds.Resize;
                        }

                        if (accessKinds != FileAccessKinds.None)
                        {
                            owner.HandleFileContentsAccessed(accessKinds, notifyTracker);
                        }

                        if (LazyHandle.IsValueCreated)
                        {
                            LazyHandle.Value.Dispose();
                        }

                        owner.lockTracker.Release(this);
                        owner.CloseStream(this);
                    }
                }

                base.Dispose(disposing);
            }

            private void AssertNotClosed()
            {
                if (isClosed)
                {
                    throw new ObjectDisposedException(string.Empty, "Cannot access a closed file.");
                }
            }

            private void AssertIsReadable()
            {
                if (!CanRead)
                {
                    throw new NotSupportedException("Stream does not support reading.");
                }
            }

            private void AssertIsWritable()
            {
                if (!CanWrite)
                {
                    throw new NotSupportedException("Stream does not support writing.");
                }
            }

            [AssertionMethod]
            private void AssertIsNotNegative(long value, [NotNull] [InvokerParameterName] string paramName)
            {
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException(paramName, "Non-negative number required.");
                }
            }

            private void UnsafeAssertRangeNotLocked(long offset, long count)
            {
                if (owner.lockTracker.UnsafeIsLocked(offset, count, this))
                {
                    throw ErrorFactory.System.CannotAccessFileProcessHasLocked();
                }
            }

            public void Lock(long position, long length)
            {
                AssertNotClosed();
                AssertIsNotNegative(position, nameof(position));
                AssertIsNotNegative(length, nameof(length));

                owner.lockTracker.Add(position, length, this);
            }

            public void Unlock(long position, long length)
            {
                AssertNotClosed();
                AssertIsNotNegative(position, nameof(position));
                AssertIsNotNegative(length, nameof(length));

                owner.lockTracker.Remove(position, length, this);
            }
        }

        [DebuggerDisplay("{GetPath().GetText()}")]
        private sealed class FileEntryPathFormatter : IPathFormatter
        {
            [NotNull]
            private readonly FileEntry fileEntry;

            public FileEntryPathFormatter([NotNull] FileEntry fileEntry)
            {
                Guard.NotNull(fileEntry, nameof(fileEntry));
                this.fileEntry = fileEntry;
            }

            public AbsolutePath GetPath()
            {
                AbsolutePath parentDirectoryPath = fileEntry.Parent.PathFormatter.GetPath();
                AbsolutePath filePath = parentDirectoryPath.Append(fileEntry.Name);
                return filePath;
            }
        }

        private sealed class LockTracker
        {
            [NotNull]
            private readonly FileEntry owner;

            [NotNull]
            [ItemNotNull]
            private readonly List<LockRange> rangesLocked = new List<LockRange>();

            public LockTracker([NotNull] FileEntry owner)
            {
                Guard.NotNull(owner, nameof(owner));
                this.owner = owner;
            }

            public bool UnsafeIsLocked(long position, long length, [NotNull] FakeFileStream stream)
            {
                Guard.NotNull(stream, nameof(stream));

                return rangesLocked.Where(x => x.Owner != stream).Any(range => range.IntersectsWith(position, length));
            }

            public void Add(long position, long length, [NotNull] FakeFileStream stream)
            {
                Guard.NotNull(owner, nameof(owner));

                lock (owner.readerWriterLock)
                {
                    if (rangesLocked.Any(range => range.IntersectsWith(position, length)))
                    {
                        throw ErrorFactory.System.CannotAccessFileProcessHasLocked();
                    }

                    rangesLocked.Add(new LockRange(position, length, stream));
                }
            }

            public void Remove(long position, long length, [NotNull] FakeFileStream stream)
            {
                Guard.NotNull(stream, nameof(stream));

                lock (owner.readerWriterLock)
                {
                    for (int index = rangesLocked.Count - 1; index >= 0; index--)
                    {
                        LockRange range = rangesLocked[index];
                        if (range.Position == position && range.Length == length && range.Owner == stream)
                        {
                            rangesLocked.RemoveAt(index);
                            return;
                        }
                    }
                }

                throw ErrorFactory.System.SegmentIsAlreadyUnlocked();
            }

            public void Release([NotNull] FakeFileStream stream)
            {
                Guard.NotNull(stream, nameof(stream));

                lock (owner.readerWriterLock)
                {
                    for (int index = rangesLocked.Count - 1; index >= 0; index--)
                    {
                        if (rangesLocked[index].Owner == stream)
                        {
                            rangesLocked.RemoveAt(index);
                        }
                    }
                }
            }

            private sealed class LockRange
            {
                public long Position { get; }
                public long Length { get; }

                [NotNull]
                public FakeFileStream Owner { get; }

                public LockRange(long position, long length, [NotNull] FakeFileStream owner)
                {
                    Position = position;
                    Length = length;
                    Owner = owner;
                }

                public bool IntersectsWith(long position, long length)
                {
                    return Position + Length > position && position + length > Position;
                }
            }
        }
    }
}
