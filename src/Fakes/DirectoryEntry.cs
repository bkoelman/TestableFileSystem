using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class DirectoryEntry : BaseEntry
    {
        private const FileAttributes DirectoryAttributesToDiscard =
            FileAttributes.Device | FileAttributes.Normal | FileAttributes.SparseFile | FileAttributes.Compressed |
            FileAttributes.Encrypted | FileAttributes.IntegrityStream;

        private const FileAttributes MinimumDriveAttributes =
            FileAttributes.Directory | FileAttributes.System | FileAttributes.Hidden;

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
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DateTime CreationTimeUtc
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DateTime LastWriteTime
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DateTime LastWriteTimeUtc
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DateTime LastAccessTime
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DateTime LastAccessTimeUtc
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        private DirectoryEntry([NotNull] string name, [CanBeNull] DirectoryEntry parent)
            : base(name)
        {
            Parent = parent;
            Attributes = IsDriveLetter(name) ? MinimumDriveAttributes : FileAttributes.Directory;
            contents = new DirectoryContents(this);
        }

        [NotNull]
        public static DirectoryEntry CreateRoot() => new DirectoryEntry("My Computer", null);

        [NotNull]
        [ItemNotNull]
        public ICollection<DirectoryEntry> FilterDrives()
        {
            return contents.GetDirectoryEntries().Where(x => x.Name.IndexOf(Path.VolumeSeparatorChar) != -1).ToArray();
        }

        [NotNull]
        public string GetAbsolutePath()
        {
            // TODO: Get rid of this method, as it is unable to account for extended paths or preserve original casing.

            if (Parent == null)
            {
                throw new InvalidOperationException();
            }

            if (Parent.Parent == null)
            {
                return IsDriveLetter(Name) ? Name + Path.DirectorySeparatorChar : Name;
            }

            return Path.Combine(Parent.GetAbsolutePath(), Name);
        }

        [NotNull]
        public FileEntry GetOrCreateFile([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            FileEntry file = contents.TryGetEntryAsFile(fileName);
            if (file == null)
            {
                contents.Add(new FileEntry(fileName, this));
            }

            return contents.GetEntryAsFile(fileName);
        }

        [NotNull]
        public FileEntry GetOrCreateFile([NotNull] PathNavigator pathNavigator, bool createSubdirectories)
        {
            Guard.NotNull(pathNavigator, nameof(pathNavigator));

            if (pathNavigator.IsAtEnd)
            {
                FileEntry file = contents.TryGetEntryAsFile(pathNavigator.Name);
                if (file == null)
                {
                    contents.Add(new FileEntry(pathNavigator.Name, this));
                }

                return contents.GetEntryAsFile(pathNavigator.Name);
            }

            DirectoryEntry subdirectory = contents.TryGetEntryAsDirectory(pathNavigator.Name);
            if (subdirectory == null && !createSubdirectories)
            {
                throw ErrorFactory.DirectoryNotFound(pathNavigator.Path.GetText());
            }

            subdirectory = GetOrCreateSingleDirectory(pathNavigator.Name);
            return subdirectory.GetOrCreateFile(pathNavigator.MoveDown(), createSubdirectories);
        }

        [CanBeNull]
        public FileEntry TryGetExistingFile([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            return contents.TryGetEntryAsFile(fileName, false);
        }

        [CanBeNull]
        public FileEntry TryGetExistingFile([NotNull] PathNavigator pathNavigator)
        {
            Guard.NotNull(pathNavigator, nameof(pathNavigator));

            if (pathNavigator.IsAtEnd)
            {
                return contents.TryGetEntryAsFile(pathNavigator.Name, false);
            }

            DirectoryEntry subdirectory = contents.TryGetEntryAsDirectory(pathNavigator.Name, false);
            return subdirectory?.TryGetExistingFile(pathNavigator.MoveDown());
        }

        public void MoveFile([NotNull] FileEntry sourceFile, [NotNull] PathNavigator destinationPathNavigator)
        {
            Guard.NotNull(sourceFile, nameof(sourceFile));
            Guard.NotNull(destinationPathNavigator, nameof(destinationPathNavigator));

            if (destinationPathNavigator.IsAtEnd)
            {
                if (Parent == null)
                {
                    throw ErrorFactory.FileOrDirectoryNameIsIncorrect();
                }

                DirectoryEntry directory = contents.TryGetEntryAsDirectory(destinationPathNavigator.Name, false);
                if (directory != null)
                {
                    throw ErrorFactory.CannotCreateFileBecauseFileAlreadyExists();
                }

                FileEntry file = contents.TryGetEntryAsFile(destinationPathNavigator.Name, false);
                if (file != null && file != sourceFile)
                {
                    throw ErrorFactory.CannotCreateFileBecauseFileAlreadyExists();
                }

                sourceFile.Parent.contents.Remove(sourceFile.Name);
                sourceFile.MoveTo(destinationPathNavigator.Name, this);
                contents.Add(sourceFile);
                return;
            }

            DirectoryEntry subdirectory = contents.TryGetEntryAsDirectory(destinationPathNavigator.Name);
            if (subdirectory == null)
            {
                throw ErrorFactory.DirectoryNotFound();
            }

            subdirectory.MoveFile(sourceFile, destinationPathNavigator.MoveDown());
        }

        internal void DeleteFile([NotNull] FileEntry fileEntry)
        {
            Guard.NotNull(fileEntry, nameof(fileEntry));

            contents.Remove(fileEntry.Name);
        }

        public void DeleteFile([NotNull] PathNavigator pathNavigator)
        {
            Guard.NotNull(pathNavigator, nameof(pathNavigator));

            if (pathNavigator.IsAtEnd)
            {
                FileEntry file = contents.TryGetEntryAsFile(pathNavigator.Name);
                if (file != null)
                {
                    AssertIsNotReadOnly(file, true);

                    // Block deletion when file is in use.
                    using (file.Open(FileMode.Open, FileAccess.ReadWrite))
                    {
                        contents.Remove(pathNavigator.Name);
                    }
                }
            }
            else
            {
                DirectoryEntry subdirectory = contents.TryGetEntryAsDirectory(pathNavigator.Name);
                if (subdirectory == null)
                {
                    throw ErrorFactory.DirectoryNotFound(pathNavigator.Path.GetText());
                }

                subdirectory.DeleteFile(pathNavigator.MoveDown());
            }
        }

        [NotNull]
        public DirectoryEntry CreateDirectories([NotNull] PathNavigator pathNavigator)
        {
            Guard.NotNull(pathNavigator, nameof(pathNavigator));

            DirectoryEntry directory = GetOrCreateSingleDirectory(pathNavigator.Name);

            return pathNavigator.IsAtEnd ? directory : directory.CreateDirectories(pathNavigator.MoveDown());
        }

        [NotNull]
        private DirectoryEntry GetOrCreateSingleDirectory([NotNull] string name)
        {
            if (Parent == null)
            {
                AssertIsDriveLetterOrNetworkShare(name);

                if (IsDriveLetter(name))
                {
                    name = name.ToUpperInvariant();
                }
            }
            else
            {
                AssertIsDirectoryName(name);
            }

            FileEntry file = contents.TryGetEntryAsFile(name, false);
            if (file != null)
            {
                string pathUpToHere = Path.Combine(GetAbsolutePath(), name);
                throw ErrorFactory.CannotCreateBecauseFileOrDirectoryAlreadyExists(pathUpToHere);
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
            // TODO: Get rid of duplication in Drive/UNC handling (see AbsolutePath).

            if (IsDriveLetter(name))
            {
                return;
            }

            if (name.StartsWith(PathFacts.TwoDirectorySeparators, StringComparison.Ordinal))
            {
                return;
            }

            throw new InvalidOperationException("Internal error: Drive letter or network share must be created at this level.");
        }

        private static bool IsDriveLetter([NotNull] string name)
        {
            if (name.Length == 2 && name[1] == Path.VolumeSeparatorChar)
            {
                char driveLetter = char.ToUpperInvariant(name[0]);
                if (driveLetter >= 'A' && driveLetter <= 'Z')
                {
                    return true;
                }
            }

            return false;
        }

        [AssertionMethod]
        private void AssertIsDirectoryName([NotNull] string name)
        {
            if (name.Contains(Path.VolumeSeparatorChar) ||
                name.StartsWith(PathFacts.TwoDirectorySeparators, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Drive letter or network share cannot be created at this level.");
            }
        }

        [NotNull]
        public DirectoryEntry GetExistingDirectory([NotNull] PathNavigator pathNavigator)
        {
            DirectoryEntry directory = TryGetExistingDirectory(pathNavigator);

            if (directory == null)
            {
                FileEntry file = TryGetExistingFile(pathNavigator);
                if (file != null)
                {
                    throw ErrorFactory.DirectoryNameIsInvalid();
                }

                throw ErrorFactory.DirectoryNotFound(pathNavigator.Path.GetText());
            }

            return directory;
        }

        [CanBeNull]
        public DirectoryEntry TryGetExistingDirectory([NotNull] PathNavigator pathNavigator)
        {
            Guard.NotNull(pathNavigator, nameof(pathNavigator));

            DirectoryEntry directory = contents.TryGetEntryAsDirectory(pathNavigator.Name, false);

            if (directory == null)
            {
                return null;
            }

            return pathNavigator.IsAtEnd ? directory : directory.TryGetExistingDirectory(pathNavigator.MoveDown());
        }

        public void DeleteDirectory([NotNull] PathNavigator pathNavigator, bool isRecursive)
        {
            Guard.NotNull(pathNavigator, nameof(pathNavigator));

            if (pathNavigator.IsAtEnd)
            {
                DirectoryEntry directory = contents.TryGetEntryAsDirectory(pathNavigator.Name);
                if (directory != null)
                {
                    AssertNotDeletingDriveOrNetworkShare(directory, pathNavigator.Path, isRecursive);
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

                    contents.Remove(pathNavigator.Name);
                }
            }
            else
            {
                DirectoryEntry subdirectory = contents.TryGetEntryAsDirectory(pathNavigator.Name);
                if (subdirectory == null)
                {
                    throw ErrorFactory.DirectoryNotFound(pathNavigator.Path.GetText());
                }

                subdirectory.DeleteDirectory(pathNavigator.MoveDown(), isRecursive);
            }
        }

        [AssertionMethod]
        private static void AssertNotDeletingDriveOrNetworkShare([NotNull] DirectoryEntry directoryToDelete,
            [NotNull] AbsolutePath path, bool isRecursive)
        {
            if (directoryToDelete.Parent?.Parent == null)
            {
                if (!path.IsOnLocalDrive)
                {
                    throw ErrorFactory.FileIsInUse(path.GetText());
                }

                if (isRecursive)
                {
                    throw ErrorFactory.FileNotFound(path.GetText());
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
        public IEnumerable<string> EnumerateEntries([NotNull] PathNavigator pathNavigator, [NotNull] string searchPattern,
            [CanBeNull] SearchOption? searchOption, EnumerationFilter filter)
        {
            Guard.NotNull(pathNavigator, nameof(pathNavigator));
            Guard.NotNull(searchPattern, nameof(searchPattern));

            DirectoryEntry directory = contents.TryGetEntryAsDirectory(pathNavigator.Name, false);
            if (directory == null)
            {
                throw ErrorFactory.DirectoryNotFound(pathNavigator.Path.GetText());
            }

            if (pathNavigator.IsAtEnd)
            {
                PathPattern pattern = PathPattern.Create(searchPattern);
                return EnumerateEntriesInDirectory(directory, pattern, searchOption, pathNavigator.Path, filter);
            }

            return directory.EnumerateEntries(pathNavigator.MoveDown(), searchPattern, searchOption, filter);
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<string> EnumerateEntriesInDirectory([NotNull] DirectoryEntry directory, [NotNull] PathPattern pattern,
            [CanBeNull] SearchOption? searchOption, [NotNull] AbsolutePath directoryPath, EnumerationFilter filter)
        {
            PathPattern subPattern = pattern.SubPattern;

            if (subPattern == null)
            {
                string basePath = directoryPath.GetText();

                foreach (BaseEntry entry in directory.contents.GetEntries(filter).Where(x => pattern.IsMatch(x.Name))
                    .OrderBy(x => x.Name))
                {
                    yield return Path.Combine(basePath, entry.Name);
                }

                if (searchOption == SearchOption.AllDirectories)
                {
                    foreach (DirectoryEntry subdirectory in directory.contents.GetDirectoryEntries().OrderBy(x => x.Name))
                    {
                        AbsolutePath subdirectoryPath = directoryPath.Append(subdirectory.Name);
                        foreach (string path in EnumerateEntriesInDirectory(subdirectory, pattern, searchOption, subdirectoryPath,
                            filter))
                        {
                            yield return path;
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
                        AbsolutePath subdirectoryPath = directoryPath.Append(subdirectory.Name);
                        foreach (string path in EnumerateEntriesInDirectory(subdirectory, subPattern, searchOption,
                            subdirectoryPath, filter))
                        {
                            yield return path;
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

            FileAttributes minimumAttributes = Parent?.Parent == null ? MinimumDriveAttributes : FileAttributes.Directory;
            return (attributes & ~DirectoryAttributesToDiscard) | minimumAttributes;
        }
    }
}
