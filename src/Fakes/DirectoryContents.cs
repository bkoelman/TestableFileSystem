using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class DirectoryContents
    {
        [NotNull]
        private readonly IDictionary<string, BaseEntry> entries =
            new Dictionary<string, BaseEntry>(StringComparer.OrdinalIgnoreCase);

        public bool IsEmpty => !entries.Any();

        [CanBeNull]
        private IReadOnlyDictionary<string, FileEntry> fileMapCached;

        [CanBeNull]
        private IReadOnlyDictionary<string, DirectoryEntry> directoryMapCached;

        [NotNull]
        public IReadOnlyDictionary<string, FileEntry> Files
        {
            get
            {
                if (fileMapCached != null)
                {
                    return fileMapCached;
                }

                return fileMapCached = GetEntries(EnumerationFilter.Files).Cast<FileEntry>()
                    .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            }
        }

        [NotNull]
        public IReadOnlyDictionary<string, DirectoryEntry> Directories
        {
            get
            {
                if (directoryMapCached != null)
                {
                    return directoryMapCached;
                }

                return directoryMapCached = GetEntries(EnumerationFilter.Directories).Cast<DirectoryEntry>()
                    .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
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
            Guard.NotNull(name, nameof(name));

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
            AssertDirectoryExists(directoryName);

            DirectoryEntry directoryEntry = Directories[directoryName];
            entries.Remove(directoryName);

            InvalidateCache();

            return directoryEntry;
        }

        [AssertionMethod]
        private void AssertDirectoryExists([NotNull] string directoryName)
        {
            Guard.NotNull(directoryName, nameof(directoryName));

            if (!Directories.ContainsKey(directoryName))
            {
                throw ErrorFactory.Internal.UnknownError($"Expected to find an existing directory named '{directoryName}'.");
            }
        }

        [NotNull]
        public FileEntry RemoveFile([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));
            AssertFileExists(fileName);

            FileEntry fileToRemove = Files[fileName];

            entries.Remove(fileName);

            InvalidateCache();

            return fileToRemove;
        }

        [AssertionMethod]
        private void AssertFileExists([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            if (!Files.ContainsKey(fileName))
            {
                throw ErrorFactory.Internal.UnknownError($"Expected to find an existing file named '{fileName}'.");
            }
        }

        private void InvalidateCache()
        {
            fileMapCached = null;
            directoryMapCached = null;
        }

        public override string ToString()
        {
            return
                $"{entries.Values.OfType<FileEntry>().Count()} files, {entries.Values.OfType<DirectoryEntry>().Count()} directories";
        }
    }
}
