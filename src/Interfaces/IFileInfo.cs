using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Provides properties and methods for the creation, copying, deletion, moving, and opening of files, and aids in the creation of
    /// <see cref="IFileStream" /> objects.
    /// </summary>
    [PublicAPI]
    public interface IFileInfo : IFileSystemInfo
    {
        /// <inheritdoc cref="FileInfo.Length" />
        long Length { get; }

        /// <inheritdoc cref="FileInfo.IsReadOnly" />
        bool IsReadOnly { get; set; }

        /// <inheritdoc cref="FileInfo.DirectoryName" />
        [CanBeNull]
        string DirectoryName { get; }

        /// <inheritdoc cref="FileInfo.Directory" />
        [CanBeNull]
        IDirectoryInfo Directory { get; }

        /// <inheritdoc cref="FileInfo.Create" />
        [NotNull]
        IFileStream Create();

        /// <inheritdoc cref="FileInfo.Open(FileMode,FileAccess,FileShare)" />
        [NotNull]
        IFileStream Open(FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None);

        /// <inheritdoc cref="FileInfo.CopyTo(string,bool)" />
        [NotNull]
        IFileInfo CopyTo([NotNull] string destFileName, bool overwrite = false);

        /// <inheritdoc cref="FileInfo.MoveTo" />
        void MoveTo([NotNull] string destFileName);

#if !NETSTANDARD1_3
        /// <inheritdoc cref="FileInfo.Encrypt" />
        void Encrypt();

        /// <inheritdoc cref="FileInfo.Decrypt" />
        void Decrypt();

        /// <inheritdoc cref="FileInfo.Replace(string,string,bool)" />
        [NotNull]
        IFileInfo Replace([NotNull] string destinationFileName, [CanBeNull] string destinationBackupFileName,
            bool ignoreMetadataErrors = false);
#endif

        /// <inheritdoc cref="FileInfo.ToString" />
        [NotNull]
        string ToString();
    }
}
