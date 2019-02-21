using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class DirectoryContents
    {
        [NotNull]
        private readonly IDictionary<string, BaseEntry> entries =
            new Dictionary<string, BaseEntry>(StringComparer.OrdinalIgnoreCase);

        public bool IsEmpty => !entries.Any();

        [CanBeNull]
        [ItemNotNull]
        private IReadOnlyCollection<FileEntry> fileSetCached;

        [CanBeNull]
        [ItemNotNull]
        private IReadOnlyCollection<DirectoryEntry> directorySetCached;

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<FileEntry> Files
        {
            get
            {
                if (fileSetCached != null)
                {
                    return fileSetCached;
                }

                return fileSetCached = GetEntries(EnumerationFilter.Files).Cast<FileEntry>().OrderBy(x => x.Name).ToArray();
            }
        }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<DirectoryEntry> Directories
        {
            get
            {
                if (directorySetCached != null)
                {
                    return directorySetCached;
                }

                return directorySetCached = GetEntries(EnumerationFilter.Directories).Cast<DirectoryEntry>().OrderBy(x => x.Name)
                    .ToArray();
            }
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<BaseEntry> GetEntries(EnumerationFilter filter)
        {
            switch (filter)
            {
                case EnumerationFilter.Files:
                {
                    return entries.Values.OfType<FileEntry>();
                }
                case EnumerationFilter.Directories:
                {
                    return entries.Values.OfType<DirectoryEntry>();
                }
                case EnumerationFilter.All:
                {
                    return entries.Values;
                }
                default:
                {
                    throw ErrorFactory.Internal.EnumValueUnsupported(filter);
                }
            }
        }

        public bool ContainsFile([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            return entries.ContainsKey(fileName) && entries[fileName] is FileEntry;
        }

        [NotNull]
        public FileEntry GetFile([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));
            AssertFileExists(fileName);

            return (FileEntry)entries[fileName];
        }

        public bool ContainsDirectory([NotNull] string directoryName)
        {
            Guard.NotNull(directoryName, nameof(directoryName));

            return entries.ContainsKey(directoryName) && entries[directoryName] is DirectoryEntry;
        }

        [NotNull]
        public DirectoryEntry GetDirectory([NotNull] string directoryName)
        {
            Guard.NotNull(directoryName, nameof(directoryName));
            AssertDirectoryExists(directoryName);

            return (DirectoryEntry)entries[directoryName];
        }

        public void Add<T>([NotNull] T entry)
            where T : BaseEntry
        {
            Guard.NotNull(entry, nameof(entry));
            AssertEntryDoesNotExist(entry.Name);

            entries[entry.Name] = entry;

            InvalidateCache();
        }

        [AssertionMethod]
        private void AssertEntryDoesNotExist([NotNull] string name)
        {
            if (entries.ContainsKey(name))
            {
                throw entries[name] is DirectoryEntry
                    ? ErrorFactory.Internal.UnknownError($"Expected not to find an existing directory named '{name}'.")
                    : ErrorFactory.Internal.UnknownError($"Expected not to find an existing file named '{name}'.");
            }
        }

        [NotNull]
        public DirectoryEntry RemoveDirectory([NotNull] string directoryName)
        {
            Guard.NotNull(directoryName, nameof(directoryName));

            DirectoryEntry directoryToRemove = GetDirectory(directoryName);
            entries.Remove(directoryName);

            InvalidateCache();

            return directoryToRemove;
        }

        [AssertionMethod]
        private void AssertDirectoryExists([NotNull] string directoryName)
        {
            if (!ContainsDirectory(directoryName))
            {
                throw ErrorFactory.Internal.UnknownError($"Expected to find an existing directory named '{directoryName}'.");
            }
        }

        [NotNull]
        public FileEntry RemoveFile([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            FileEntry fileToRemove = GetFile(fileName);
            entries.Remove(fileName);

            InvalidateCache();

            return fileToRemove;
        }

        [AssertionMethod]
        private void AssertFileExists([NotNull] string fileName)
        {
            if (!ContainsFile(fileName))
            {
                throw ErrorFactory.Internal.UnknownError($"Expected to find an existing file named '{fileName}'.");
            }
        }

        private void InvalidateCache()
        {
            fileSetCached = null;
            directorySetCached = null;
        }

        public override string ToString()
        {
            return
                $"{entries.Values.OfType<FileEntry>().Count()} files, {entries.Values.OfType<DirectoryEntry>().Count()} directories";
        }
    }
}
