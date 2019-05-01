using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class FakeDirectoryInfo : FakeFileSystemInfo, IDirectoryInfo
    {
        public override string Name => AbsolutePath.IsVolumeRoot ? FullName : AbsolutePath.Components.Last();

        public override bool Exists => Metadata.Exists && Metadata.Attributes.HasFlag(FileAttributes.Directory);

        public IDirectoryInfo Parent
        {
            get
            {
                AbsolutePath parentPath = AbsolutePath.TryGetParentPath();
                if (parentPath == null)
                {
                    return null;
                }

                string displayPath = parentPath.Components.Last();
                if (parentPath.IsVolumeRoot && !parentPath.IsOnLocalDrive)
                {
                    // Emulate the bug where a network share is incorrectly broken into parts ("\\server\share" should be a single component, not two).
                    int lastSeparatorIndex = parentPath.VolumeName.LastIndexOf(Path.DirectorySeparatorChar);
                    displayPath = parentPath.VolumeName.Substring(lastSeparatorIndex + 1);
                }

                return Owner.ConstructDirectoryInfo(parentPath, displayPath);
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

        internal FakeDirectoryInfo([NotNull] VolumeContainer container, [NotNull] FakeFileSystem owner,
            [NotNull] AbsolutePath path, [CanBeNull] string displayPath)
            : base(container, owner, path, displayPath)
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
            Guard.NotNull(path, nameof(path));

            AssertPathIsNotEmpty(path);
            AssertPathIsRelative(path);

            AbsolutePath completePath =
                path.Trim().Length == 0 ? AbsolutePath : RelativePathConverter.Combine(AbsolutePath, path);

            if (!PathStartsWith(completePath, AbsolutePath))
            {
                throw ErrorFactory.System.DirectoryIsNotASubdirectory(path, AbsolutePath.GetText());
            }

            return Owner.Directory.CreateDirectory(completePath.GetText());
        }

        private static void AssertPathIsNotEmpty([NotNull] string path)
        {
            if (path.Length == 0)
            {
                throw ErrorFactory.System.PathCannotBeEmptyOrWhitespace(nameof(path));
            }
        }

        private static void AssertPathIsRelative([NotNull] string path)
        {
            if (PathFacts.DirectorySeparatorChars.Contains(path[0]) || path.Contains(Path.VolumeSeparatorChar))
            {
                throw ErrorFactory.System.PathFragmentMustNotBeDriveOrUncName(nameof(path));
            }
        }

        private bool PathStartsWith([NotNull] AbsolutePath longPath, [NotNull] AbsolutePath shortPath)
        {
            if (longPath.Components.Count < shortPath.Components.Count)
            {
                return false;
            }

            for (int index = 0; index < shortPath.Components.Count; index++)
            {
                if (!string.Equals(shortPath.Components[index], longPath.Components[index], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public void Delete(bool recursive)
        {
            Owner.Directory.Delete(FullName, recursive);
        }

        public void MoveTo(string destDirName)
        {
            Owner.Directory.Move(FullName, destDirName);

            AbsolutePath destinationPath = Owner.ToAbsolutePathInLock(destDirName);
            ChangePath(destinationPath, destDirName);
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
                {
                    Owner.Directory.SetCreationTimeUtc(FullName, value);
                    break;
                }
                case FileTimeKind.LastAccessTime:
                {
                    Owner.Directory.SetLastAccessTimeUtc(FullName, value);
                    break;
                }
                case FileTimeKind.LastWriteTime:
                {
                    Owner.Directory.SetLastWriteTimeUtc(FullName, value);
                    break;
                }
                default:
                {
                    throw ErrorFactory.Internal.EnumValueUnsupported(kind);
                }
            }
        }
    }
}
