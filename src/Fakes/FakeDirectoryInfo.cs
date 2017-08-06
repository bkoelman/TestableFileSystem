using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeDirectoryInfo : FakeFileSystemInfo, IDirectoryInfo
    {
        // TODO: Review this naive implementation against MSDN and the actual filesystem.

        public override string Name
        {
            get
            {
                string name = Path.GetFileName(FullName);
                return string.IsNullOrEmpty(name) ? FullName : name;
            }
        }

        public IDirectoryInfo Parent => Owner.Directory.GetParent(FullName);

        public IDirectoryInfo Root
        {
            get
            {
                string root = Owner.Directory.GetDirectoryRoot(FullName);
                return Owner.ConstructDirectoryInfo(root);
            }
        }

        internal FakeDirectoryInfo([NotNull] FakeFileSystem owner, [NotNull] AbsolutePath path)
            : base(owner, path)
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
            string absolutePath = Path.Combine(FullName, path);
            return Owner.Directory.CreateDirectory(absolutePath);
        }

        public void Delete(bool recursive)
        {
            Owner.Directory.Delete(FullName, recursive);
        }

        public void MoveTo(string destDirName)
        {
            Owner.Directory.Move(FullName, destDirName);
        }
    }
}
