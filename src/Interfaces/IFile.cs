using System;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    [PublicAPI]
    public interface IFile
    {
        bool Exists([CanBeNull] string path);

        [NotNull]
        IFileStream Create([NotNull] string path, int bufferSize = 4096, FileOptions options = FileOptions.None);

        [NotNull]
        IFileStream Open([NotNull] string path, FileMode mode, [CanBeNull] FileAccess? access = null,
            FileShare share = FileShare.None);

        void Copy([NotNull] string sourceFileName, [NotNull] string destFileName, bool overwrite = false);

        void Move([NotNull] string sourceFileName, [NotNull] string destFileName);

        void Delete([NotNull] string path);

        FileAttributes GetAttributes([NotNull] string path);
        void SetAttributes([NotNull] string path, FileAttributes fileAttributes);

        DateTime GetCreationTime([NotNull] string path);
        DateTime GetCreationTimeUtc([NotNull] string path);
        void SetCreationTime([NotNull] string path, DateTime creationTime);
        void SetCreationTimeUtc([NotNull] string path, DateTime creationTimeUtc);

        DateTime GetLastAccessTime([NotNull] string path);
        DateTime GetLastAccessTimeUtc([NotNull] string path);
        void SetLastAccessTime([NotNull] string path, DateTime lastAccessTime);
        void SetLastAccessTimeUtc([NotNull] string path, DateTime lastAccessTimeUtc);

        DateTime GetLastWriteTime([NotNull] string path);
        DateTime GetLastWriteTimeUtc([NotNull] string path);
        void SetLastWriteTime([NotNull] string path, DateTime lastWriteTime);
        void SetLastWriteTimeUtc([NotNull] string path, DateTime lastWriteTimeUtc);

#if !NETSTANDARD1_3
        void Encrypt([NotNull] string path);
        void Decrypt([NotNull] string path);

        void Replace([NotNull] string sourceFileName, [NotNull] string destinationFileName,
            [CanBeNull] string destinationBackupFileName, bool ignoreMetadataErrors = false);
#endif
    }
}
