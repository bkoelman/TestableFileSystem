using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal abstract class BaseEntry
    {
        private FileAttributes attributes;

        [NotNull]
        public string Name { get; protected set; }

        [NotNull]
        protected FakeFileSystemChangeTracker ChangeTracker { get; }

        public FileAttributes Attributes
        {
            get => attributes;
            set => attributes = FilterAttributes(value);
        }

        public abstract DateTime CreationTime { get; set; }
        public abstract DateTime CreationTimeUtc { get; set; }
        public abstract DateTime LastAccessTime { get; set; }
        public abstract DateTime LastAccessTimeUtc { get; set; }
        public abstract DateTime LastWriteTime { get; set; }
        public abstract DateTime LastWriteTimeUtc { get; set; }

        protected BaseEntry([NotNull] string name, [NotNull] FakeFileSystemChangeTracker changeTracker)
        {
            Guard.NotNullNorWhiteSpace(name, nameof(name));
            Guard.NotNull(changeTracker, nameof(changeTracker));

            Name = name;
            ChangeTracker = changeTracker;
        }

        protected abstract FileAttributes FilterAttributes(FileAttributes attributes);
    }
}
