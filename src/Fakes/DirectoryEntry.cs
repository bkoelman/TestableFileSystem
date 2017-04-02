using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class DirectoryEntry : BaseEntry
    {
        [NotNull]
        private static readonly string TwoDirectorySeparators = new string(Path.DirectorySeparatorChar, 2);

        private const FileAttributes DirectoryAttributesToDiscard = FileAttributes.Device | FileAttributes.Normal |
            FileAttributes.SparseFile | FileAttributes.Compressed | FileAttributes.Encrypted | FileAttributes.IntegrityStream;

        [NotNull]
        private readonly DirectoryContents contents;

        [NotNull]
        public IReadOnlyDictionary<string, FileEntry> Files => contents.GetFileEntries()
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        [NotNull]
        public IReadOnlyDictionary<string, DirectoryEntry> Directories => contents.GetDirectoryEntries()
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        [CanBeNull]
        public DirectoryEntry Parent { get; }

        public override DateTime CreationTime
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override DateTime CreationTimeUtc
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override DateTime LastWriteTime
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override DateTime LastWriteTimeUtc
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override DateTime LastAccessTime
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override DateTime LastAccessTimeUtc
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private DirectoryEntry([NotNull] string name, [CanBeNull] DirectoryEntry parent)
            : base(name)
        {
            Parent = parent;
            Attributes = FileAttributes.Directory;
            contents = new DirectoryContents(this);
        }

        [NotNull]
        public static DirectoryEntry CreateRoot() => new DirectoryEntry("My Computer", null);

        [NotNull]
        [ItemNotNull]
        public ICollection<DirectoryEntry> GetDrives()
        {
            return contents.GetDirectoryEntries().Where(x => x.Name.IndexOf(Path.VolumeSeparatorChar) != -1).ToArray();
        }

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
                FileEntry file = contents.TryGetEntryAsFile(path.Name);
                if (file == null)
                {
                    contents.Add(new FileEntry(path.Name, this));
                }

                return contents.GetEntryAsFile(path.Name);
            }

            DirectoryEntry subdirectory = contents.TryGetEntryAsDirectory(path.Name);
            if (subdirectory == null && !createSubdirectories)
            {
                throw ErrorFactory.DirectoryNotFound(path.GetText());
            }

            subdirectory = GetOrCreateDirectory(path.Name);
            return subdirectory.GetOrCreateFile(path.MoveDown(), createSubdirectories);
        }

        [CanBeNull]
        public FileEntry TryGetExistingFile([NotNull] AbsolutePath path, bool throwOnMissingDirectory = true)
        {
            Guard.NotNull(path, nameof(path));

            if (path.IsAtEnd)
            {
                return contents.TryGetEntryAsFile(path.Name, false);
            }

            DirectoryEntry subdirectory = contents.TryGetEntryAsDirectory(path.Name);
            if (subdirectory == null)
            {
                if (throwOnMissingDirectory)
                {
                    throw ErrorFactory.DirectoryNotFound(path.GetText());
                }
                return null;
            }

            subdirectory = contents.GetEntryAsDirectory(path.Name);
            return subdirectory.TryGetExistingFile(path.MoveDown(), throwOnMissingDirectory);
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

                DirectoryEntry directory = contents.TryGetEntryAsDirectory(destinationPath.Name, false);
                if (directory != null)
                {
                    throw ErrorFactory.CannotMoveBecauseFileAlreadyExists();
                }

                FileEntry file = contents.TryGetEntryAsFile(destinationPath.Name, false);
                if (file != null && file != sourceFile)
                {
                    throw ErrorFactory.CannotMoveBecauseFileAlreadyExists();
                }

                sourceFile.Parent.contents.Remove(sourceFile.Name);
                sourceFile.MoveTo(destinationPath.Name, this);
                contents.Add(sourceFile);
                return;
            }

            DirectoryEntry subdirectory = contents.TryGetEntryAsDirectory(destinationPath.Name);
            if (subdirectory == null)
            {
                throw ErrorFactory.DirectoryNotFound();
            }

            subdirectory.MoveFile(sourceFile, destinationPath.MoveDown());
        }

        public void DeleteFile([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            if (path.IsAtEnd)
            {
                FileEntry file = contents.TryGetEntryAsFile(path.Name);
                if (file != null)
                {
                    AssertIsNotReadOnly(file, true);

                    // Block deletion when file is in use.
                    using (file.Open(FileMode.Open, FileAccess.ReadWrite))
                    {
                        contents.Remove(path.Name);
                    }
                }
            }
            else
            {
                DirectoryEntry subdirectory = contents.TryGetEntryAsDirectory(path.Name);
                if (subdirectory == null)
                {
                    throw ErrorFactory.DirectoryNotFound(path.GetText());
                }

                subdirectory.DeleteFile(path.MoveDown());
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

            DirectoryEntry directory = contents.TryGetEntryAsDirectory(name);
            if (directory == null)
            {
                contents.Add(new DirectoryEntry(name, this));
            }

            return contents.GetEntryAsDirectory(name);
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

            DirectoryEntry directory = contents.TryGetEntryAsDirectory(path.Name, false);
            if (directory == null)
            {
                return null;
            }

            return path.IsAtEnd ? directory : directory.TryGetExistingDirectory(path.MoveDown());
        }

        public void DeleteDirectory([NotNull] AbsolutePath path, bool isRecursive)
        {
            Guard.NotNull(path, nameof(path));

            if (path.IsAtEnd)
            {
                DirectoryEntry directory = contents.TryGetEntryAsDirectory(path.Name);
                if (directory != null)
                {
                    AssertNotDeletingDrive(directory, isRecursive);
                    AssertIsNotReadOnly(directory);

                    // Block deletion when directory contains file that is in use.
                    FileEntry openFile = directory.TryGetFirstOpenFile();
                    if (openFile != null)
                    {
                        throw ErrorFactory.FileIsInUse(openFile.GetAbsolutePath());
                    }

                    if (!isRecursive && !directory.contents.IsEmpty)
                    {
                        throw ErrorFactory.DirectoryIsNotEmpty();
                    }

                    contents.Remove(path.Name);
                }
            }
            else
            {
                DirectoryEntry subdirectory = contents.TryGetEntryAsDirectory(path.Name);
                if (subdirectory == null)
                {
                    throw ErrorFactory.DirectoryNotFound(path.GetText());
                }

                subdirectory.DeleteDirectory(path.MoveDown(), isRecursive);
            }
        }

        [AssertionMethod]
        private static void AssertNotDeletingDrive([NotNull] DirectoryEntry directoryToDelete, bool isRecursive)
        {
            if (directoryToDelete.Parent?.Parent == null)
            {
                if (isRecursive)
                {
                    string path = directoryToDelete.GetAbsolutePath();
                    throw ErrorFactory.FileNotFound(path);
                }

                throw ErrorFactory.DirectoryIsNotEmpty();
            }
        }

        private static void AssertIsNotReadOnly([NotNull] DirectoryEntry directory)
        {
            if ((directory.Attributes & FileAttributes.ReadOnly) != 0)
            {
                throw ErrorFactory.AccessDenied(directory.GetAbsolutePath());
            }

            foreach (DirectoryEntry subdirectory in directory.contents.GetDirectoryEntries())
            {
                AssertIsNotReadOnly(subdirectory);
            }

            foreach (FileEntry file in directory.contents.GetFileEntries())
            {
                AssertIsNotReadOnly(file, false);
            }
        }

        private static void AssertIsNotReadOnly([NotNull] FileEntry file, bool reportAbsolutePath)
        {
            if ((file.Attributes & FileAttributes.ReadOnly) != 0)
            {
                string path = reportAbsolutePath ? file.GetAbsolutePath() : file.Name;
                throw ErrorFactory.UnauthorizedAccess(path);
            }
        }

        [CanBeNull]
        private FileEntry TryGetFirstOpenFile()
        {
            FileEntry file = contents.GetFileEntries().FirstOrDefault(x => x.IsOpen());
            if (file == null)
            {
                foreach (DirectoryEntry directory in contents.GetDirectoryEntries())
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

            DirectoryEntry directory = contents.TryGetEntryAsDirectory(path.Name);
            if (directory == null)
            {
                throw ErrorFactory.DirectoryNotFound(path.GetText());
            }

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
                foreach (DirectoryEntry directoryEntry in directory.contents.GetDirectoryEntries()
                    .Where(x => pattern.IsMatch(x.Name)))
                {
                    yield return Path.Combine(directoryPath, directoryEntry.Name);
                }
            }
            else
            {
                foreach (DirectoryEntry subdirectory in directory.contents.GetDirectoryEntries())
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

            DirectoryEntry directory = contents.TryGetEntryAsDirectory(path.Name);
            if (directory == null)
            {
                throw ErrorFactory.DirectoryNotFound(path.GetText());
            }

            if (path.IsAtEnd)
            {
                PathPattern pattern = searchPattern == null ? PathPattern.MatchAny : PathPattern.Create(searchPattern);
                return EnumerateFilesInDirectory(directory, pattern, searchOption);
            }

            return directory.EnumerateFiles(path.MoveDown(), searchPattern, searchOption);
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

                foreach (FileEntry fileEntry in directory.contents.GetFileEntries().Where(x => pattern.IsMatch(x.Name)))
                {
                    yield return Path.Combine(path, fileEntry.Name);
                }

                if (searchOption == SearchOption.AllDirectories)
                {
                    foreach (DirectoryEntry subdirectory in directory.contents.GetDirectoryEntries())
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
                foreach (DirectoryEntry subdirectory in directory.contents.GetDirectoryEntries())
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
            return $"Directory: {Name} ({contents})";
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
