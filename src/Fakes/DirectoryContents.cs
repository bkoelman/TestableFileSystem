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

        [NotNull]
        [ItemNotNull]
        public IEnumerable<BaseEntry> GetEntries(EnumerationFilter filter)
        {
            switch (filter)
            {
                case EnumerationFilter.Files:
                    return entries.Values.OfType<FileEntry>();
                case EnumerationFilter.Directories:
                    return entries.Values.OfType<DirectoryEntry>();
                case EnumerationFilter.All:
                    return entries.Values;
                default:
                    throw new NotSupportedException($"Unsupported filter '{filter}'.");
            }
        }

        [NotNull]
        public T Add<T>([NotNull] T entry) where T : BaseEntry
        {
            Guard.NotNull(entry, nameof(entry));
            AssertEntryDoesNotExist(entry.Name);

            entries[entry.Name] = entry;
            return entry;
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

        public void RemoveDirectory([NotNull] string directoryName)
        {
            Guard.NotNull(directoryName, nameof(directoryName));
            AssertDirectoryExists(directoryName);

            entries.Remove(directoryName);
        }

        [AssertionMethod]
        private void AssertDirectoryExists([NotNull] string directoryName)
        {
            Guard.NotNull(directoryName, nameof(directoryName));

            if (!entries.ContainsKey(directoryName))
            {
                throw ErrorFactory.Internal.UnknownError($"Expected to find an existing directory named '{directoryName}'.");
            }
        }

        public void RemoveFile([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));
            AssertFileExists(fileName);

            entries.Remove(fileName);
        }

        [AssertionMethod]
        private void AssertFileExists([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            if (!entries.ContainsKey(fileName))
            {
                throw ErrorFactory.Internal.UnknownError($"Expected to find an existing file named '{fileName}'.");
            }
        }

        public override string ToString()
        {
            return
                $"{entries.Values.OfType<FileEntry>().Count()} files, {entries.Values.OfType<DirectoryEntry>().Count()} directories";
        }
    }
}
