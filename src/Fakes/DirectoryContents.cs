using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class DirectoryContents
    {
        [NotNull]
        private readonly DirectoryEntry owner;

        [NotNull]
        private readonly IDictionary<string, BaseEntry> entries =
            new Dictionary<string, BaseEntry>(StringComparer.OrdinalIgnoreCase);

        public bool IsEmpty => !entries.Any();

        public DirectoryContents([NotNull] DirectoryEntry owner)
        {
            Guard.NotNull(owner, nameof(owner));
            this.owner = owner;
        }

        [NotNull]
        public FileEntry GetEntryAsFile([NotNull] string name)
        {
            FileEntry file = TryGetEntryAsFile(name);
            if (file == null)
            {
                throw new Exception("TODO: File not found.");
            }

            return file;
        }

        [CanBeNull]
        public FileEntry TryGetEntryAsFile([NotNull] string name, bool throwIfExistsAsDirectory = true)
        {
            Guard.NotNull(name, nameof(name));

            if (entries.ContainsKey(name))
            {
                var file = entries[name] as FileEntry;
                if (file != null)
                {
                    return file;
                }

                if (throwIfExistsAsDirectory)
                {
                    string path = Path.Combine(owner.GetAbsolutePath(), name);
                    throw ErrorFactory.UnauthorizedAccess(path);
                }
            }

            return null;
        }

        [NotNull]
        public DirectoryEntry GetEntryAsDirectory([NotNull] string name)
        {
            DirectoryEntry directory = TryGetEntryAsDirectory(name);
            if (directory == null)
            {
                throw new Exception("TODO: Directory not found.");
            }

            return directory;
        }

        [CanBeNull]
        public DirectoryEntry TryGetEntryAsDirectory([NotNull] string name, bool throwIfExistsAsFile = true)
        {
            Guard.NotNull(name, nameof(name));

            if (entries.ContainsKey(name))
            {
                var directory = entries[name] as DirectoryEntry;
                if (directory != null)
                {
                    return directory;
                }

                if (throwIfExistsAsFile)
                {
                    throw new Exception("TODO: File instead of Directory.");
                }
            }

            return null;
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<DirectoryEntry> GetDirectoryEntries()
        {
            foreach (DirectoryEntry entry in entries.Values.OfType<DirectoryEntry>())
            {
                yield return entry;
            }
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FileEntry> GetFileEntries()
        {
            foreach (FileEntry entry in entries.Values.OfType<FileEntry>())
            {
                yield return entry;
            }
        }

        public void Add([NotNull] BaseEntry entry)
        {
            Guard.NotNull(entry, nameof(entry));

            entries[entry.Name] = entry;
        }

        public void Remove([NotNull] string name)
        {
            Guard.NotNull(name, nameof(name));

            entries.Remove(name);
        }

        public override string ToString()
        {
            return $"{entries.OfType<FileEntry>().Count()} files, {entries.OfType<DirectoryEntry>().Count()} directories";
        }
    }
}
