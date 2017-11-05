using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeDirectoryInfo : FakeFileSystemInfo, IDirectoryInfo
    {
        public override string Name => AbsolutePath.IsVolumeRoot ? FullName : AbsolutePath.Components.Last();

        public override bool Exists => Properties.Exists && Properties.Attributes.HasFlag(FileAttributes.Directory);

        public IDirectoryInfo Parent
        {
            get
            {
                AbsolutePath parentPath = AbsolutePath.TryGetParentPath();
                return parentPath == null ? null : Owner.ConstructDirectoryInfo(parentPath);
            }
        }

        public IDirectoryInfo Root
        {
            get
            {
                AbsolutePath rootPath = AbsolutePath.GetAncestorPath(0);
                return rootPath == AbsolutePath ? this : Owner.ConstructDirectoryInfo(rootPath);
            }
        }

        internal FakeDirectoryInfo([NotNull] DirectoryEntry root, [NotNull] FakeFileSystem owner, [NotNull] AbsolutePath path)
            : base(root, owner, path)
        {
        }

        public IFileInfo[] GetFiles(string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFiles(searchPattern, searchOption).ToArray();
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            foreach (string path in Owner.Directory.EnumerateFiles(FullName, searchPattern, searchOption))
            {
                yield return Owner.ConstructFileInfo(path);
            }
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateDirectories(searchPattern, searchOption).ToArray();
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            foreach (string path in Owner.Directory.EnumerateDirectories(FullName, searchPattern, searchOption))
            {
                yield return Owner.ConstructDirectoryInfo(path);
            }
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFileSystemInfos(searchPattern, searchOption).ToArray();
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            foreach (string path in Owner.Directory.EnumerateFileSystemEntries(FullName, searchPattern, searchOption))
            {
                yield return Owner.File.Exists(path)
                    ? (IFileSystemInfo)Owner.ConstructFileInfo(path)
                    : Owner.ConstructDirectoryInfo(path);
            }
        }

        public void Create()
        {
            Owner.Directory.CreateDirectory(FullName);
        }

        public IDirectoryInfo CreateSubdirectory(string path)
        {
            AbsolutePath subPath = RelativePathConverter.Combine(AbsolutePath, path);
            return Owner.Directory.CreateDirectory(subPath.GetText());
        }

        public void Delete(bool recursive)
        {
            Owner.Directory.Delete(FullName, recursive);
        }

        public void MoveTo(string destDirName)
        {
            Owner.Directory.Move(FullName, destDirName);

            AbsolutePath destinationPath = Owner.ToAbsolutePath(destDirName);
            ChangePath(destinationPath);
        }

        public override void Delete()
        {
            Owner.Directory.Delete(FullName);
        }

        internal override void SetTimeUtc(FileTimeKind kind, DateTime value)
        {
            switch (kind)
            {
                case FileTimeKind.CreationTime:
                    Owner.Directory.SetCreationTimeUtc(FullName, value);
                    break;
                case FileTimeKind.LastAccessTime:
                    Owner.Directory.SetLastAccessTimeUtc(FullName, value);
                    break;
                case FileTimeKind.LastWriteTime:
                    Owner.Directory.SetLastWriteTimeUtc(FullName, value);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported kind of file time '{kind}'.");
            }
        }
    }
}
