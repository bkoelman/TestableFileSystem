using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    public sealed class DirectoryInfoWrapper : FileSystemInfoWrapper, IDirectoryInfo
    {
        [NotNull]
        private readonly DirectoryInfo source;

        public IDirectoryInfo Parent => Utilities.WrapOrNull(source.Parent, x => new DirectoryInfoWrapper(x));

        public IDirectoryInfo Root => new DirectoryInfoWrapper(source.Root);

        public DirectoryInfoWrapper([NotNull] DirectoryInfo source)
            : base(source)
        {
            Guard.NotNull(source, nameof(source));
            this.source = source;
        }

        public IFileInfo[] GetFiles(string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            FileInfo[] files = source.GetFiles(searchPattern, searchOption);
            return files.Select(x => (IFileInfo)new FileInfoWrapper(x)).ToArray();
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            FileInfo[] files = source.GetFiles(searchPattern, searchOption);
            return files.Select(x => (IFileInfo)new FileInfoWrapper(x));
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            DirectoryInfo[] directories = source.GetDirectories(searchPattern, searchOption);
            return directories.Select(x => (IDirectoryInfo)new DirectoryInfoWrapper(x)).ToArray();
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            IEnumerable<DirectoryInfo> directories = source.EnumerateDirectories(searchPattern, searchOption);
            return directories.Select(x => (IDirectoryInfo)new DirectoryInfoWrapper(x));
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            FileSystemInfo[] entries = source.GetFileSystemInfos(searchPattern, searchOption);
            return entries.Select(FileSystemInfoWrapperFactory.CreateWrapper).ToArray();
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            FileSystemInfo[] entries = source.GetFileSystemInfos(searchPattern, searchOption);
            return entries.Select(FileSystemInfoWrapperFactory.CreateWrapper);
        }

        public void Create()
        {
            source.Create();
        }

        public IDirectoryInfo CreateSubdirectory(string path)
        {
            return new DirectoryInfoWrapper(source.CreateSubdirectory(path));
        }

        public void Delete(bool recursive)
        {
            source.Delete(recursive);
        }

        public void MoveTo(string destDirName)
        {
            source.MoveTo(destDirName);
        }

        public override string ToString()
        {
            return source.ToString();
        }
    }
}
