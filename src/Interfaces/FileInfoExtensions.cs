using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Provides extension methods on <see cref="IFileInfo" /> objects to open files.
    /// </summary>
    [PublicAPI]
    public static class FileInfoExtensions
    {
        /// <inheritdoc cref="FileInfo.OpenRead" />
        [NotNull]
        public static IFileStream OpenRead([NotNull] this IFileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            return fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <inheritdoc cref="FileInfo.OpenWrite" />
        [NotNull]
        public static IFileStream OpenWrite([NotNull] this IFileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            return fileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write);
        }

        /// <inheritdoc cref="FileInfo.OpenText" />
        [NotNull]
        public static StreamReader OpenText([NotNull] this IFileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            IFileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            return new StreamReader(stream.AsStream());
        }

        /// <inheritdoc cref="FileInfo.CreateText" />
        [NotNull]
        public static StreamWriter CreateText([NotNull] this IFileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            IFileStream stream = fileInfo.Open(FileMode.Create, FileAccess.Write);
            return new StreamWriter(stream.AsStream());
        }

        /// <inheritdoc cref="FileInfo.AppendText" />
        [NotNull]
        public static StreamWriter AppendText([NotNull] this IFileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            IFileStream stream = fileInfo.Open(FileMode.Append, FileAccess.Write);
            return new StreamWriter(stream.AsStream());
        }
    }
}
