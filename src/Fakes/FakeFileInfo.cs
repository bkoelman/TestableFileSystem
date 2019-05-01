using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class FakeFileInfo : FakeFileSystemInfo, IFileInfo
    {
        public override string Name => Path.GetFileName(DisplayPath);

        public override bool Exists => Metadata.Exists && !Metadata.Attributes.HasFlag(FileAttributes.Directory);

        public long Length
        {
            get
            {
                AssertIsExistingFile();
                return Metadata.FileSize;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                Metadata.AssertNoError();
                return Metadata.Attributes.HasFlag(FileAttributes.ReadOnly);
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

        internal FakeFileInfo([NotNull] VolumeContainer container, [NotNull] FakeFileSystem owner, [NotNull] AbsolutePath path,
            [CanBeNull] string displayPath)
            : base(container, owner, path, displayPath)
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

            AbsolutePath destinationPath = Owner.ToAbsolutePathInLock(destFileName);
            ChangePath(destinationPath, destFileName);
        }

#if !NETSTANDARD1_3
        public void Encrypt()
        {
            Owner.File.Encrypt(FullName);
        }

        public void Decrypt()
        {
            Owner.File.Decrypt(FullName);
        }

        public IFileInfo Replace(string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors = false)
        {
            Owner.File.Replace(FullName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
            return Owner.ConstructFileInfo(destinationFileName);
        }
#endif

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
            Metadata.AssertNoError();

            if (Attributes.HasFlag(FileAttributes.Directory))
            {
                throw ErrorFactory.System.FileNotFound(FullName);
            }
        }
    }
}
