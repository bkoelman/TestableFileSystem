using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    public static class FileSystemInfoWrapperFactory
    {
        [CanBeNull]
        public static IFileSystemInfo CreateWrapper([CanBeNull] FileSystemInfo source)
        {
            if (source == null)
            {
                return null;
            }

            var directoryInfo = source as DirectoryInfo;
            if (directoryInfo != null)
            {
                return new DirectoryInfoWrapper(directoryInfo);
            }

            var fileInfo = source as FileInfo;
            if (fileInfo != null)
            {
                return new FileInfoWrapper(fileInfo);
            }

            throw new NotSupportedException($"Type {source.GetType()} is not supported.");
        }
    }
}
