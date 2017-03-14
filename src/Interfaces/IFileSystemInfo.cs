using JetBrains.Annotations;
using System;
using System.IO;

namespace TestableFileSystem.Interfaces
{
    public interface IFileSystemInfo
    {
        [NotNull]
        string Name { get; }
        [NotNull]
        string Extension { get; }
        [NotNull]
        string FullName { get; }

        FileAttributes Attributes { get; set; }

        DateTime CreationTime { get; set; }
        DateTime CreationTimeUtc { get; set; }
        DateTime LastAccessTime { get; set; }
        DateTime LastAccessTimeUtc { get; set; }
        DateTime LastWriteTime { get; set; }
        DateTime LastWriteTimeUtc { get; set; }

        bool Exists { get; }

        void Refresh();
        void Delete();
    }
}