using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    [PublicAPI]
    public interface IDirectoryInfo : IFileSystemInfo
    {
        [CanBeNull]
        IDirectoryInfo Parent { get; }

        [NotNull]
        IDirectoryInfo Root { get; }

        [NotNull]
        [ItemNotNull]
        IFileInfo[] GetFiles([NotNull] string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);

        [NotNull]
        [ItemNotNull]
        IEnumerable<IFileInfo> EnumerateFiles([NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        [NotNull]
        [ItemNotNull]
        IDirectoryInfo[] GetDirectories([NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        [NotNull]
        [ItemNotNull]
        IEnumerable<IDirectoryInfo> EnumerateDirectories([NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        [NotNull]
        [ItemNotNull]
        IFileSystemInfo[] GetFileSystemInfos([NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        [NotNull]
        [ItemNotNull]
        IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos([NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        void Create();

        [NotNull]
        IDirectoryInfo CreateSubdirectory([NotNull] string path);

        void Delete(bool recursive);

        void MoveTo([NotNull] string destDirName);

        [NotNull]
        string ToString();
    }
}
