using JetBrains.Annotations;
using System;
using System.IO;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    public abstract class FileSystemInfoWrapper : IFileSystemInfo
    {
        [NotNull]
        private readonly FileSystemInfo source;

        protected FileSystemInfoWrapper([NotNull] FileSystemInfo source)
        {
            Guard.NotNull(source, nameof(source));
            this.source = source;
        }

        public string Name => source.Name;

        public string Extension => source.Extension;

        public string FullName => source.FullName;

        public FileAttributes Attributes
        {
            get { return source.Attributes; }
            set { source.Attributes = value; }
        }

        public DateTime CreationTime
        {
            get { return source.CreationTime; }
            set { source.CreationTime = value; }
        }

        public DateTime CreationTimeUtc
        {
            get { return source.CreationTimeUtc; }
            set { source.CreationTimeUtc = value; }
        }

        public DateTime LastAccessTime
        {
            get { return source.LastAccessTime; }
            set { source.LastAccessTime = value; }
        }

        public DateTime LastAccessTimeUtc
        {
            get { return source.LastAccessTimeUtc; }
            set { source.LastAccessTimeUtc = value; }
        }

        public DateTime LastWriteTime
        {
            get { return source.LastWriteTime; }
            set { source.LastWriteTime = value; }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return source.LastWriteTimeUtc; }
            set { source.LastWriteTimeUtc = value; }
        }

        public bool Exists => source.Exists;

        public void Refresh()
        {
            source.Refresh();
        }

        public void Delete()
        {
            source.Delete();
        }
    }
}