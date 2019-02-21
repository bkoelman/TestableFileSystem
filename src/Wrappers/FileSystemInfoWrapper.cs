using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Wrappers
{
    internal abstract class FileSystemInfoWrapper : IFileSystemInfo
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
            get => source.Attributes;
            set => source.Attributes = value;
        }

        public DateTime CreationTime
        {
            get => source.CreationTime;
            set => source.CreationTime = value;
        }

        public DateTime CreationTimeUtc
        {
            get => source.CreationTimeUtc;
            set => source.CreationTimeUtc = value;
        }

        public DateTime LastAccessTime
        {
            get => source.LastAccessTime;
            set => source.LastAccessTime = value;
        }

        public DateTime LastAccessTimeUtc
        {
            get => source.LastAccessTimeUtc;
            set => source.LastAccessTimeUtc = value;
        }

        public DateTime LastWriteTime
        {
            get => source.LastWriteTime;
            set => source.LastWriteTime = value;
        }

        public DateTime LastWriteTimeUtc
        {
            get => source.LastWriteTimeUtc;
            set => source.LastWriteTimeUtc = value;
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
