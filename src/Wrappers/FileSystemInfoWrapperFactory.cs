using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    internal static class FileSystemInfoWrapperFactory
    {
        [CanBeNull]
        public static IFileSystemInfo CreateWrapper([CanBeNull] FileSystemInfo source)
        {
            if (source == null)
            {
                return null;
            }

            if (source is DirectoryInfo directoryInfo)
            {
                return new DirectoryInfoWrapper(directoryInfo);
            }

            if (source is FileInfo fileInfo)
            {
                return new FileInfoWrapper(fileInfo);
            }

            throw new NotSupportedException($"Type {source.GetType()} is not supported.");
        }
    }
}
