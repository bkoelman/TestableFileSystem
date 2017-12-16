using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    public sealed class FileInfoWrapper : FileSystemInfoWrapper, IFileInfo
    {
        [NotNull]
        private readonly FileInfo source;

        public long Length => source.Length;

        public bool IsReadOnly
        {
            get => source.IsReadOnly;
            set => source.IsReadOnly = value;
        }

        public string DirectoryName => source.DirectoryName;

        public IDirectoryInfo Directory => Utilities.WrapOrNull(source.Directory, x => new DirectoryInfoWrapper(x));

        public FileInfoWrapper([NotNull] FileInfo source)
            : base(source)
        {
            Guard.NotNull(source, nameof(source));
            this.source = source;
        }

        public IFileStream Create()
        {
            return new FileStreamWrapper(source.Create());
        }

        public IFileStream Open(FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
        {
            return new FileStreamWrapper(source.Open(mode, access, share));
        }

        public IFileInfo CopyTo(string destFileName, bool overwrite = false)
        {
            return new FileInfoWrapper(source.CopyTo(destFileName, overwrite));
        }

        public void MoveTo(string destFileName)
        {
            source.MoveTo(destFileName);
        }

        public override string ToString()
        {
            return source.ToString();
        }
    }
}
