using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileInfo : FakeFileSystemInfo, IFileInfo
    {
        public override string Name =>
            AbsolutePath.IsVolumeRoot ? string.Empty : AbsolutePath.Components.Last() + AbsolutePath.TrailingWhiteSpace;

        public override bool Exists => Properties.Exists && !Properties.Attributes.HasFlag(FileAttributes.Directory);

        public long Length
        {
            get
            {
                AssertIsExistingFile();
                return Properties.FileSize;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                Properties.AssertNoError();
                return Properties.Attributes.HasFlag(FileAttributes.ReadOnly);
            }
            set
            {
                if (value)
                {
                    Attributes |= FileAttributes.ReadOnly;
                }
                else
                {
                    Attributes &= ~FileAttributes.ReadOnly;
                }
            }
        }

        public string DirectoryName => AbsolutePath.TryGetParentPath()?.GetText();

        public IDirectoryInfo Directory
        {
            get
            {
                AbsolutePath parentPath = AbsolutePath.TryGetParentPath();
                return parentPath == null ? null : Owner.ConstructDirectoryInfo(parentPath);
            }
        }

        internal FakeFileInfo([NotNull] DirectoryEntry root, [NotNull] FakeFileSystem owner, [NotNull] AbsolutePath path)
            : base(root, owner, path)
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
            return Owner.ConstructFileInfo(destFileName);
        }

        public void MoveTo(string destFileName)
        {
            Owner.File.Move(FullName, destFileName);

            AbsolutePath destinationPath = Owner.ToAbsolutePath(destFileName);
            ChangePath(destinationPath);
        }

        public override void Delete()
        {
            Owner.File.Delete(FullName);
        }

        internal override void SetTimeUtc(FileTimeKind kind, DateTime value)
        {
            switch (kind)
            {
                case FileTimeKind.CreationTime:
                {
                    Owner.File.SetCreationTimeUtc(FullName, value);
                    break;
                }
                case FileTimeKind.LastAccessTime:
                {
                    Owner.File.SetLastAccessTimeUtc(FullName, value);
                    break;
                }
                case FileTimeKind.LastWriteTime:
                {
                    Owner.File.SetLastWriteTimeUtc(FullName, value);
                    break;
                }
                default:
                {
                    throw ErrorFactory.Internal.EnumValueUnsupported(kind);
                }
            }
        }

        private void AssertIsExistingFile()
        {
            Properties.AssertNoError();

            if (Attributes.HasFlag(FileAttributes.Directory))
            {
                throw ErrorFactory.System.FileNotFound(FullName);
            }
        }
    }
}
