using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class DirectoryEntry
        : BaseEntry
    {
        [NotNull]
        private static readonly string TwoDirectorySeparators = new string(Path.DirectorySeparatorChar, 2);

        private const FileAttributes DirectoryAttributesToDiscard = FileAttributes.Device | FileAttributes.Normal |
            FileAttributes.SparseFile | FileAttributes.Compressed | FileAttributes.Encrypted | FileAttributes.IntegrityStream;

        [CanBeNull]
        public DirectoryEntry Parent { get; }

        [NotNull]
        public IDictionary<string, FileEntry> Files { get; } =
            new Dictionary<string, FileEntry>(StringComparer.OrdinalIgnoreCase);

        [NotNull]
        public IDictionary<string, DirectoryEntry> Directories { get; } =
            new Dictionary<string, DirectoryEntry>(StringComparer.OrdinalIgnoreCase);

        public bool IsEmpty => !Files.Any() && !Directories.Any();

        public override DateTime CreationTime
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override DateTime CreationTimeUtc
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override DateTime LastWriteTime
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override DateTime LastWriteTimeUtc
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override DateTime LastAccessTime
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override DateTime LastAccessTimeUtc
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        private DirectoryEntry([NotNull] string name, [CanBeNull] DirectoryEntry parent)
            :base(name)
        {
            Parent = parent;
            Attributes = FileAttributes.Directory;
        }

        [NotNull]
        public static DirectoryEntry CreateRoot() => new DirectoryEntry("My Computer", null);

        [NotNull]
        public string GetAbsolutePath()
        {
            if (Parent == null)
            {
                throw new InvalidOperationException();
            }

            if (Parent.Parent == null)
            {
                return Name + Path.DirectorySeparatorChar;
            }

            return Path.Combine(Parent.GetAbsolutePath(), Name);
        }

        [NotNull]
        public FileEntry GetOrCreateFile([NotNull] AbsolutePath path, bool createSubdirectories)
        {
            Guard.NotNull(path, nameof(path));

            if (path.IsAtEnd)
            {
                if (!Files.ContainsKey(path.Name))
                {
                    Files[path.Name] = new FileEntry(path.Name, this);
                }

                return Files[path.Name];
            }

            if (!createSubdirectories)
            {
                AssertContainsDirectory(path);
            }

            DirectoryEntry directory = GetOrCreateDirectory(path.Name);
            return directory.GetOrCreateFile(path.MoveDown(), createSubdirectories);
        }

        [CanBeNull]
        public FileEntry TryGetExistingFile([NotNull] AbsolutePath path, bool throwOnMissingDirectory = true)
        {
            Guard.NotNull(path, nameof(path));

            if (path.IsAtEnd)
            {
                if (!Files.ContainsKey(path.Name))
                {
                    return null;
                }

                return Files[path.Name];
            }

            if (!AssertContainsDirectory(path, throwOnMissingDirectory))
            {
                return null;
            }

            return Directories[path.Name].TryGetExistingFile(path.MoveDown(), throwOnMissingDirectory);
        }

        public void MoveFile([NotNull] FileEntry sourceFile, [NotNull] AbsolutePath destinationPath)
        {
            Guard.NotNull(sourceFile, nameof(sourceFile));
            Guard.NotNull(destinationPath, nameof(destinationPath));

            if (destinationPath.IsAtEnd)
            {
                if (Parent == null)
                {
                    throw ErrorFactory.CannotMoveBecauseTargetIsInvalid();
                }

                if (Directories.ContainsKey(destinationPath.Name))
                {
                    throw ErrorFactory.CannotMoveBecauseFileAlreadyExists();
                }

                if (Files.ContainsKey(destinationPath.Name) && Files[destinationPath.Name] != sourceFile)
                {
                    throw ErrorFactory.CannotMoveBecauseFileAlreadyExists();
                }

                sourceFile.Parent.Files.Remove(sourceFile.Name);
                Files[destinationPath.Name] = sourceFile;

                sourceFile.MoveTo(destinationPath.Name, this);
                return;
            }

            if (!Directories.ContainsKey(destinationPath.Name))
            {
                throw ErrorFactory.DirectoryNotFound();
            }

            Directories[destinationPath.Name].MoveFile(sourceFile, destinationPath.MoveDown());
        }

        public void DeleteFile([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            if (path.IsAtEnd)
            {
                if (Files.ContainsKey(path.Name))
                {
                    FileEntry file = Files[path.Name];

                    // Block deletion when file is in use.
                    using (file.Open(FileMode.Open, FileAccess.ReadWrite))
                    {
                        Files.Remove(path.Name);
                    }
                }
            }
            else
            {
                AssertContainsDirectory(path);
                Directories[path.Name].DeleteFile(path.MoveDown());
            }
        }

        [NotNull]
        public DirectoryEntry CreateDirectory([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            DirectoryEntry directory = GetOrCreateDirectory(path.Name);

            return path.IsAtEnd ? directory : directory.CreateDirectory(path.MoveDown());
        }

        [NotNull]
        private DirectoryEntry GetOrCreateDirectory([NotNull] string name)
        {
            if (Parent == null)
            {
                AssertIsDriveLetterOrNetworkShare(name);
            }
            else
            {
                AssertIsDirectoryName(name);
            }

            if (!Directories.ContainsKey(name))
            {
                Directories[name] = new DirectoryEntry(name, this);
            }

            return Directories[name];
        }

        [AssertionMethod]
        private static void AssertIsDriveLetterOrNetworkShare([NotNull] string name)
        {
            if (name.Length == 2 && name[1] == Path.VolumeSeparatorChar)
            {
                char driveLetter = char.ToUpperInvariant(name[0]);
                if (driveLetter >= 'A' && driveLetter <= 'Z')
                {
                    return;
                }
            }

            if (name.StartsWith(TwoDirectorySeparators, StringComparison.Ordinal))
            {
                return;
            }

            throw new InvalidOperationException("Drive letter or network share must be created at this level.");
        }

        [AssertionMethod]
        private void AssertIsDirectoryName([NotNull] string name)
        {
            if (name.Contains(Path.VolumeSeparatorChar) || name.StartsWith(TwoDirectorySeparators, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Drive letter or network share cannot be created at this level.");
            }
        }

        [NotNull]
        public DirectoryEntry GetExistingDirectory([NotNull] AbsolutePath path)
        {
            DirectoryEntry directory = TryGetExistingDirectory(path);

            if (directory == null)
            {
                throw ErrorFactory.DirectoryNotFound(path.GetText());
            }

            return directory;
        }

        [CanBeNull]
        public DirectoryEntry TryGetExistingDirectory([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            if (!Directories.ContainsKey(path.Name))
            {
                return null;
            }

            DirectoryEntry directory = Directories[path.Name];

            return path.IsAtEnd ? directory : directory.TryGetExistingDirectory(path.MoveDown());
        }

        public void DeleteDirectory([NotNull] AbsolutePath path, bool isRecursive)
        {
            Guard.NotNull(path, nameof(path));

            if (path.IsAtEnd)
            {
                if (Directories.ContainsKey(path.Name))
                {
                    DirectoryEntry directory = Directories[path.Name];

                    // Block deletion when directory contains file that is in use.
                    FileEntry openFile = directory.TryGetFirstOpenFile();
                    if (openFile != null)
                    {
                        throw ErrorFactory.FileIsInUse(openFile.GetAbsolutePath());
                    }

                    if (!isRecursive && !directory.IsEmpty)
                    {
                        throw ErrorFactory.DirectoryIsNotEmpty();
                    }

                    Directories.Remove(path.Name);
                }
            }
            else
            {
                AssertContainsDirectory(path);
                Directories[path.Name].DeleteDirectory(path.MoveDown(), isRecursive);
            }
        }

        [CanBeNull]
        private FileEntry TryGetFirstOpenFile()
        {
            FileEntry file = Files.Values.FirstOrDefault(x => x.IsOpen());
            if (file == null)
            {
                foreach (DirectoryEntry directory in Directories.Values)
                {
                    file = directory.TryGetFirstOpenFile();
                    if (file != null)
                    {
                        break;
                    }
                }
            }

            return file;
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<string> EnumerateDirectories([NotNull] AbsolutePath path, [CanBeNull] string searchPattern)
        {
            Guard.NotNull(path, nameof(path));

            AssertContainsDirectory(path);
            DirectoryEntry directory = Directories[path.Name];

            if (path.IsAtEnd)
            {
                PathPattern pattern = searchPattern == null ? PathPattern.MatchAny : PathPattern.Create(searchPattern);
                return EnumerateDirectoriesInDirectory(directory, pattern);
            }

            return directory.EnumerateDirectories(path.MoveDown(), searchPattern);
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<string> EnumerateDirectoriesInDirectory([NotNull] DirectoryEntry directory,
            [NotNull] PathPattern pattern)
        {
            PathPattern subPattern = pattern.SubPattern;

            if (subPattern == null)
            {
                string directoryPath = directory.GetAbsolutePath();
                foreach (string directoryName in directory.Directories.Keys.Where(pattern.IsMatch))
                {
                    yield return Path.Combine(directoryPath, directoryName);
                }
            }
            else
            {
                foreach (DirectoryEntry subdirectory in directory.Directories.Values)
                {
                    if (pattern.IsMatch(subdirectory.Name))
                    {
                        foreach (string directoryPath in EnumerateDirectoriesInDirectory(subdirectory, subPattern))
                        {
                            yield return directoryPath;
                        }
                    }
                }
            }
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<string> EnumerateFiles([NotNull] AbsolutePath path, [CanBeNull] string searchPattern,
            [CanBeNull] SearchOption? searchOption)
        {
            Guard.NotNull(path, nameof(path));

            AssertContainsDirectory(path);
            DirectoryEntry directory = Directories[path.Name];

            if (path.IsAtEnd)
            {
                PathPattern pattern = searchPattern == null ? PathPattern.MatchAny : PathPattern.Create(searchPattern);
                return EnumerateFilesInDirectory(directory, pattern, searchOption);
            }

            return Directories[path.Name].EnumerateFiles(path.MoveDown(), searchPattern, searchOption);
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<string> EnumerateFilesInDirectory([NotNull] DirectoryEntry directory, [NotNull] PathPattern pattern,
            [CanBeNull] SearchOption? searchOption)
        {
            PathPattern subPattern = pattern.SubPattern;

            if (subPattern == null)
            {
                string path = directory.GetAbsolutePath();

                foreach (string fileName in directory.Files.Keys.Where(pattern.IsMatch))
                {
                    yield return Path.Combine(path, fileName);
                }

                if (searchOption == SearchOption.AllDirectories)
                {
                    foreach (DirectoryEntry subdirectory in directory.Directories.Values)
                    {
                        foreach (string filePath in EnumerateFilesInDirectory(subdirectory, pattern, searchOption))
                        {
                            yield return filePath;
                        }
                    }
                }
            }
            else
            {
                foreach (DirectoryEntry subdirectory in directory.Directories.Values)
                {
                    if (pattern.IsMatch(subdirectory.Name))
                    {
                        foreach (string filePath in EnumerateFilesInDirectory(subdirectory, subPattern, searchOption))
                        {
                            yield return filePath;
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"Directory: {Name} ({Files.Count} files, {Directories.Count} directories)";
        }

        [AssertionMethod]
        private bool AssertContainsDirectory([NotNull] AbsolutePath path, bool throwOnMissingDirectory = true)
        {
            if (!Directories.ContainsKey(path.Name))
            {
                if (throwOnMissingDirectory)
                {
                    throw ErrorFactory.DirectoryNotFound(path.GetText());
                }
                return false;
            }
            return true;
        }

        protected override void AssertNameIsValid(string name)
        {
            // Only reachable through an AbsolutePath instance, which already performs validation.
            // TODO: Add validation when implementing Rename, Copy etc.
        }

        protected override FileAttributes FilterAttributes(FileAttributes attributes)
        {
            if ((attributes & FileAttributes.Temporary) != 0)
            {
                throw new ArgumentException("Invalid File or Directory attributes value.", nameof(attributes));
            }

            return (attributes & ~DirectoryAttributesToDiscard) | FileAttributes.Directory;
        }
    }
}
