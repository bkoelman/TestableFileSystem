using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    [PublicAPI]
    public interface IFileInfo : IFileSystemInfo
    {
        long Length { get; }

        bool IsReadOnly { get; set; }

        [CanBeNull]
        string DirectoryName { get; }

        [CanBeNull]
        IDirectoryInfo Directory { get; }

        [NotNull]
        IFileStream Create();

        [NotNull]
        IFileStream Open(FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None);

        [NotNull]
        IFileInfo CopyTo([NotNull] string destFileName, bool overwrite = false);

        void MoveTo([NotNull] string destFileName);

#if !NETSTANDARD1_3
        void Encrypt();
        void Decrypt();
#endif

        [NotNull]
        string ToString();
    }
}
