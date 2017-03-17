using System;
using System.Collections.Generic;
using System.IO;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    public sealed class DirectoryWrapper : IDirectory
    {
        public IDirectoryInfo GetParent(string path)
        {
            return Utilities.WrapOrNull(Directory.GetParent(path), x => new DirectoryInfoWrapper(x));
        }

        public string GetDirectoryRoot(string path)
        {
            return Directory.GetDirectoryRoot(path);
        }

        public string[] GetFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return Directory.GetFiles(path, searchPattern, searchOption);
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return Directory.EnumerateFiles(path, searchPattern, searchOption);
        }

        public string[] GetDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return Directory.GetDirectories(path, searchPattern, searchOption);
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return Directory.EnumerateDirectories(path, searchPattern, searchOption);
        }

        public string[] GetFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return Directory.GetFileSystemEntries(path, searchPattern, searchOption);
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public IDirectoryInfo CreateDirectory(string path)
        {
            return new DirectoryInfoWrapper(Directory.CreateDirectory(path));
        }

        public void Delete(string path, bool recursive = false)
        {
            Directory.Delete(path, recursive);
        }

        public void Move(string sourceDirName, string destDirName)
        {
            Directory.Move(sourceDirName, destDirName);
        }

        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public void SetCurrentDirectory(string path)
        {
            Directory.SetCurrentDirectory(path);
        }

        public DateTime GetCreationTime(string path)
        {
            return Directory.GetCreationTime(path);
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            return Directory.GetCreationTimeUtc(path);
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            Directory.SetCreationTime(path, creationTime);
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            Directory.SetCreationTimeUtc(path, creationTimeUtc);
        }

        public DateTime GetLastAccessTime(string path)
        {
            return Directory.GetLastAccessTime(path);
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            return Directory.GetLastAccessTimeUtc(path);
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            Directory.SetLastAccessTime(path, lastAccessTime);
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            Directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
        }

        public DateTime GetLastWriteTime(string path)
        {
            return Directory.GetLastWriteTime(path);
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            return Directory.GetLastWriteTimeUtc(path);
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            Directory.SetLastWriteTime(path, lastWriteTime);
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
        }
    }
}
