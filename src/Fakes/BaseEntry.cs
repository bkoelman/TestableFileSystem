using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public abstract class BaseEntry
    {
        [NotNull]
        private string name;

        private FileAttributes attributes;

        [NotNull]
        public string Name
        {
            get
            {
                return name;
            }
            protected set
            {
                AssertNameIsValid(value);
                name = value;
            }
        }

        public FileAttributes Attributes
        {
            get
            {
                return attributes;
            }
            set
            {
                attributes = FilterAttributes(value);
            }
        }

        public abstract DateTime CreationTime { get; set; }
        public abstract DateTime CreationTimeUtc { get; set; }
        public abstract DateTime LastWriteTime { get; set; }
        public abstract DateTime LastWriteTimeUtc { get; set; }
        public abstract DateTime LastAccessTime { get; set; }
        public abstract DateTime LastAccessTimeUtc { get; set; }

        protected BaseEntry([NotNull] string name)
        {
            Guard.NotNullNorWhiteSpace(name, nameof(name));
            Name = name;
        }

        [AssertionMethod]
        protected abstract void AssertNameIsValid([NotNull] string name);

        protected abstract FileAttributes FilterAttributes(FileAttributes attributes);
    }
}
