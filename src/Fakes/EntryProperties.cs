using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class EntryProperties
    {
        private const FileAttributes MissingEntryAttributes = (FileAttributes)(-1);

        [CanBeNull]
        private readonly Exception lastError;

        public bool Exists => lastError == null && Attributes != MissingEntryAttributes;

        public FileAttributes Attributes { get; }

        public DateTime CreationTimeUtc { get; }
        public DateTime LastAccessTimeUtc { get; }
        public DateTime LastWriteTimeUtc { get; }

        public long FileSize { get; }

        [NotNull]
        public static readonly EntryProperties Default = new EntryProperties(null);

        private EntryProperties([CanBeNull] Exception error)
            : this(MissingEntryAttributes, PathFacts.ZeroFileTimeUtc, PathFacts.ZeroFileTimeUtc, PathFacts.ZeroFileTimeUtc, -1,
                error)
        {
        }

        private EntryProperties(FileAttributes attributes, DateTime creationTimeUtc, DateTime lastAccessTimeUtc,
            DateTime lastWriteTimeUtc, long fileSize, [CanBeNull] Exception error)
        {
            Attributes = attributes;
            CreationTimeUtc = creationTimeUtc;
            LastAccessTimeUtc = lastAccessTimeUtc;
            LastWriteTimeUtc = lastWriteTimeUtc;
            FileSize = fileSize;
            lastError = error;
        }

        [NotNull]
        public static EntryProperties CreateForError([NotNull] Exception error)
        {
            Guard.NotNull(error, nameof(error));

            return new EntryProperties(error);
        }

        [NotNull]
        public static EntryProperties CreateForSuccess(FileAttributes attributes, DateTime creationTimeUtc,
            DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc, long fileSize)
        {
            return new EntryProperties(attributes, creationTimeUtc, lastAccessTimeUtc, lastWriteTimeUtc, fileSize, null);
        }

        public void AssertNoError()
        {
            if (lastError != null)
            {
                throw lastError;
            }
        }
    }
}
