using System;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Provides methods for the creation, copying, deletion, moving, and opening of a single file, and aids in the creation of
    /// <see cref="IFileStream" /> objects.
    /// </summary>
    [PublicAPI]
    public interface IFile
    {
        /// <inheritdoc cref="File.Exists" />
        bool Exists([CanBeNull] string path);

        /// <inheritdoc cref="File.Create(string,int,FileOptions)" />
        [NotNull]
        IFileStream Create([NotNull] string path, int bufferSize = 4096, FileOptions options = FileOptions.None);

        /// <inheritdoc cref="File.Open(string,FileMode,FileAccess,FileShare)" />
        [NotNull]
        IFileStream Open([NotNull] string path, FileMode mode, [CanBeNull] FileAccess? access = null,
            FileShare share = FileShare.None);

        /// <inheritdoc cref="File.Copy(string,string,bool)" />
        void Copy([NotNull] string sourceFileName, [NotNull] string destFileName, bool overwrite = false);

        /// <inheritdoc cref="File.Move" />
        void Move([NotNull] string sourceFileName, [NotNull] string destFileName);

        /// <inheritdoc cref="File.Delete" />
        void Delete([NotNull] string path);

        /// <inheritdoc cref="File.GetAttributes" />
        FileAttributes GetAttributes([NotNull] string path);

        /// <inheritdoc cref="File.SetAttributes" />
        void SetAttributes([NotNull] string path, FileAttributes fileAttributes);

        /// <inheritdoc cref="File.GetCreationTime" />
        DateTime GetCreationTime([NotNull] string path);

        /// <inheritdoc cref="File.GetCreationTimeUtc" />
        DateTime GetCreationTimeUtc([NotNull] string path);

        /// <inheritdoc cref="File.SetCreationTime" />
        void SetCreationTime([NotNull] string path, DateTime creationTime);

        /// <inheritdoc cref="File.SetCreationTimeUtc" />
        void SetCreationTimeUtc([NotNull] string path, DateTime creationTimeUtc);

        /// <inheritdoc cref="File.GetLastAccessTime" />
        DateTime GetLastAccessTime([NotNull] string path);

        /// <inheritdoc cref="File.GetLastAccessTimeUtc" />
        DateTime GetLastAccessTimeUtc([NotNull] string path);

        /// <inheritdoc cref="File.SetLastAccessTime" />
        void SetLastAccessTime([NotNull] string path, DateTime lastAccessTime);

        /// <inheritdoc cref="File.SetLastAccessTimeUtc" />
        void SetLastAccessTimeUtc([NotNull] string path, DateTime lastAccessTimeUtc);

        /// <inheritdoc cref="File.GetLastWriteTime" />
        DateTime GetLastWriteTime([NotNull] string path);

        /// <inheritdoc cref="File.GetLastWriteTimeUtc" />
        DateTime GetLastWriteTimeUtc([NotNull] string path);

        /// <inheritdoc cref="File.SetLastWriteTime" />
        void SetLastWriteTime([NotNull] string path, DateTime lastWriteTime);

        /// <inheritdoc cref="File.SetLastWriteTimeUtc" />
        void SetLastWriteTimeUtc([NotNull] string path, DateTime lastWriteTimeUtc);

#if !NETSTANDARD1_3
        /// <inheritdoc cref="File.Encrypt" />
        void Encrypt([NotNull] string path);

        /// <inheritdoc cref="File.Decrypt" />
        void Decrypt([NotNull] string path);

        /// <inheritdoc cref="File.Replace(string,string,string,bool)" />
        void Replace([NotNull] string sourceFileName, [NotNull] string destinationFileName,
            [CanBeNull] string destinationBackupFileName, bool ignoreMetadataErrors = false);
#endif
    }
}
