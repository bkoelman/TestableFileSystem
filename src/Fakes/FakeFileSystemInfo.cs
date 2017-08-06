using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public abstract class FakeFileSystemInfo : IFileSystemInfo
    {
        // TODO: Review this naive implementation against MSDN and the actual filesystem.

        [NotNull]
        protected FakeFileSystem Owner { get; }

        public abstract string Name { get; }

        public string Extension { get; }

        public string FullName { get; }

        public FileAttributes Attributes
        {
            get => Owner.File.GetAttributes(FullName);
            set => Owner.File.SetAttributes(FullName, value);
        }

        public DateTime CreationTime
        {
            get => Owner.File.GetCreationTime(FullName);
            set => Owner.File.SetCreationTime(FullName, value);
        }

        public DateTime CreationTimeUtc
        {
            get => Owner.File.GetCreationTimeUtc(FullName);
            set => Owner.File.SetCreationTimeUtc(FullName, value);
        }

        public DateTime LastAccessTime
        {
            get => Owner.File.GetLastAccessTime(FullName);
            set => Owner.File.SetLastAccessTime(FullName, value);
        }

        public DateTime LastAccessTimeUtc
        {
            get => Owner.File.GetLastAccessTimeUtc(FullName);
            set => Owner.File.SetLastAccessTimeUtc(FullName, value);
        }

        public DateTime LastWriteTime
        {
            get => Owner.File.GetLastWriteTime(FullName);
            set => Owner.File.SetLastWriteTime(FullName, value);
        }

        public DateTime LastWriteTimeUtc
        {
            get => Owner.File.GetLastWriteTimeUtc(FullName);
            set => Owner.File.SetLastWriteTimeUtc(FullName, value);
        }

        public bool Exists => Owner.File.Exists(FullName);

        internal FakeFileSystemInfo([NotNull] FakeFileSystem owner, [NotNull] AbsolutePath path)
        {
            Guard.NotNull(owner, nameof(owner));
            Guard.NotNull(path, nameof(path));

            Owner = owner;

            FullName = path.GetText();
            Extension = Path.GetExtension(FullName);
        }

        public void Refresh()
        {
        }

        public void Delete()
        {
            Owner.File.Delete(FullName);
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
