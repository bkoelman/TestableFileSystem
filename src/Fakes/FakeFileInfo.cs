using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileInfo : FakeFileSystemInfo, IFileInfo
    {
        // TODO: Review this naive implementation against MSDN and the actual filesystem.

        public override string Name => Path.GetFileName(FullName);

        public long Length { get; }

        public bool IsReadOnly
        {
            get => (Owner.File.GetAttributes(FullName) & FileAttributes.ReadOnly) != 0;
            set
            {
                FileAttributes attributes = Owner.File.GetAttributes(FullName);

                if (value)
                {
                    attributes |= FileAttributes.ReadOnly;
                }
                else
                {
                    attributes &= ~FileAttributes.ReadOnly;
                }

                Owner.File.SetAttributes(FullName, attributes);
            }
        }

        public string DirectoryName => Path.GetDirectoryName(FullName);

        public IDirectoryInfo Directory => new FakeDirectoryInfo(Owner, DirectoryName);

        public FakeFileInfo([NotNull] IFileSystem owner, [NotNull] string path)
            : base(owner, path)
        {
        }

        public IFileStream Create()
        {
            return Owner.File.Create(FullName);
        }

        public IFileStream Open(FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
        {
            return Owner.File.Open(FullName, mode, access, share);
        }

        public IFileInfo CopyTo(string destFileName, bool overwrite = false)
        {
            Owner.File.Copy(FullName, destFileName, overwrite);
            return new FakeFileInfo(Owner, destFileName);
        }

        public void MoveTo(string destFileName)
        {
            Owner.File.Move(FullName, destFileName);
        }
    }
}
