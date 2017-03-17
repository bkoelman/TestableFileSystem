using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    public interface IFileInfo : IFileSystemInfo
    {
        long Length { get; }

        bool IsReadOnly { get; set; }

        [NotNull]
        string DirectoryName { get; }

        [NotNull]
        IDirectoryInfo Directory { get; }

        [NotNull]
        IFileStream Create();

        [NotNull]
        IFileStream Open(FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None);

        [NotNull]
        IFileInfo CopyTo([NotNull] string destFileName, bool overwrite = false);

        void MoveTo([NotNull] string destFileName);

        [NotNull]
        string ToString();
    }
}
