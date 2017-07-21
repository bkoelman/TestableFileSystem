using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

[assembly: InternalsVisibleTo("TestableFileSystem.Fakes.Tests")]

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileSystem : IFileSystem
    {
        [NotNull]
        public FileOperationLocker<FakeFile> File { get; }

        IFile IFileSystem.File => File;

        [NotNull]
        public DirectoryOperationLocker<FakeDirectory> Directory { get; }

        IDirectory IFileSystem.Directory => Directory;

        [NotNull]
        internal readonly object TreeLock = new object();

        [NotNull]
        internal CurrentDirectoryManager CurrentDirectory { get; }

        internal FakeFileSystem([NotNull] DirectoryEntry rootEntry)
        {
            Guard.NotNull(rootEntry, nameof(rootEntry));

            CurrentDirectory = new CurrentDirectoryManager(rootEntry);
            Directory = new DirectoryOperationLocker<FakeDirectory>(this, new FakeDirectory(rootEntry, this));
            File = new FileOperationLocker<FakeFile>(this, new FakeFile(rootEntry, this));
        }

        [NotNull]
        public FakeFileInfo ConstructFileInfo([NotNull] string fileName)
        {
            AbsolutePath absolutePath = ToAbsolutePath(fileName);
            return new FakeFileInfo(this, absolutePath.GetText());
        }

        IFileInfo IFileSystem.ConstructFileInfo(string fileName) => ConstructFileInfo(fileName);

        [NotNull]
        public FakeDirectoryInfo ConstructDirectoryInfo([NotNull] string path)
        {
            AbsolutePath absolutePath = ToAbsolutePath(path);
            return new FakeDirectoryInfo(this, absolutePath.GetText());
        }

        IDirectoryInfo IFileSystem.ConstructDirectoryInfo(string path) => ConstructDirectoryInfo(path);

        [NotNull]
        internal AbsolutePath ToAbsolutePath([NotNull] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw ErrorFactory.PathIsNotLegal(nameof(path));
            }

            DirectoryEntry baseDirectory = CurrentDirectory.GetValue();
            string basePath = baseDirectory.GetAbsolutePath();

            path = CompensatePathForRelativeDriveReference(path, basePath);

            string rooted = Path.Combine(basePath, path);
            return new AbsolutePath(rooted);
        }

        [NotNull]
        private static string CompensatePathForRelativeDriveReference([NotNull] string path, [NotNull] string basePath)
        {
            bool hasRelativeDriveReference = path.Length >= 3 && path[1] == ':' && !IsPathSeparator(path[2]);
            if (!hasRelativeDriveReference)
            {
                return path;
            }

            char pathDriveLetter = path[0];
            char baseDriveLetter = basePath[0];

            return pathDriveLetter == baseDriveLetter
                ? path.Substring(2)
                : path.Substring(0, 2) + Path.DirectorySeparatorChar + path.Substring(2);
        }

        private static bool IsPathSeparator(char ch)
        {
            return PathFacts.DirectorySeparatorChars.Contains(ch);
        }

        internal long GetFileSize([NotNull] string path)
        {
            return File.ExecuteOnFile(f => f.GetSize(path));
        }
    }
}
