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

        [NotNull]
        internal abstract IPathFormatter PathFormatter { get; }

        public FileAttributes Attributes
        {
            get => attributes;
            set
            {
                FileAttributes beforeAttributes = attributes;

                attributes = FilterAttributes(value);

                if (attributes != beforeAttributes)
                {
                    ChangeTracker.NotifyAttributesChanged(PathFormatter);
                }
            }
        }

        public abstract DateTime CreationTime { get; set; }
        public abstract DateTime CreationTimeUtc { get; set; }
        public abstract DateTime LastAccessTime { get; set; }
        public abstract DateTime LastAccessTimeUtc { get; set; }
        public abstract DateTime LastWriteTime { get; set; }
        public abstract DateTime LastWriteTimeUtc { get; set; }

        protected BaseEntry([NotNull] string name, FileAttributes attributes, [NotNull] FakeFileSystemChangeTracker changeTracker)
        {
            Guard.NotNullNorWhiteSpace(name, nameof(name));
            Guard.NotNull(changeTracker, nameof(changeTracker));

            Name = name;
            this.attributes = attributes;
            ChangeTracker = changeTracker;
        }

        protected abstract FileAttributes FilterAttributes(FileAttributes attributes);
    }
}
