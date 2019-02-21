using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Interfaces
{
    [PublicAPI]
    public static class FileInfoExtensions
    {
        [NotNull]
        public static IFileStream OpenRead([NotNull] this IFileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            return fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        [NotNull]
        public static IFileStream OpenWrite([NotNull] this IFileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            return fileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write);
        }

        [NotNull]
        public static StreamReader OpenText([NotNull] this IFileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            IFileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            return new StreamReader(stream.AsStream());
        }

        [NotNull]
        public static StreamWriter CreateText([NotNull] this IFileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            IFileStream stream = fileInfo.Open(FileMode.Create, FileAccess.Write);
            return new StreamWriter(stream.AsStream());
        }

        [NotNull]
        public static StreamWriter AppendText([NotNull] this IFileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            IFileStream stream = fileInfo.Open(FileMode.Append, FileAccess.Write);
            return new StreamWriter(stream.AsStream());
        }
    }
}
