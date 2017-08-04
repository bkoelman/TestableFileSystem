using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

// TODO: Remove this after converting all specs.
[assembly: InternalsVisibleTo("TestableFileSystem.Fakes.Tests")]

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileSystem : IFileSystem
    {
        // TODO: Add specs for paths like "\folder\file.txt" (current drive)
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx

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

        [NotNull]
        internal WaitIndicator CopyWaitIndicator { get; }

        internal FakeFileSystem([NotNull] DirectoryEntry root, [NotNull] WaitIndicator copyWaitIndicator)
        {
            Guard.NotNull(root, nameof(root));

            CurrentDirectory = new CurrentDirectoryManager(root);
            Directory = new DirectoryOperationLocker<FakeDirectory>(this, new FakeDirectory(root, this));
            File = new FileOperationLocker<FakeFile>(this, new FakeFile(root, this));
            CopyWaitIndicator = copyWaitIndicator;
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
                throw ErrorFactory.System.PathIsNotLegal(nameof(path));
            }

            string basePath = CurrentDirectory.GetValue().GetText();

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
