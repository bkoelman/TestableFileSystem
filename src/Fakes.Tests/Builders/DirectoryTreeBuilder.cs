using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Tests.Utilities;

namespace TestableFileSystem.Fakes.Tests.Builders
{
    internal sealed class DirectoryTreeBuilder : ITestDataBuilder<DirectoryEntry>
    {
        [NotNull]
        private readonly DirectoryEntry root = DirectoryEntry.CreateRoot();

        public DirectoryEntry Build()
        {
            return root;
        }

        [NotNull]
        public DirectoryTreeBuilder IncludingEmptyFile([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            InnerIncludingFile(path, entry => { }, attributes);
            return this;
        }

        [NotNull]
        public DirectoryTreeBuilder IncludingTextFile([NotNull] string path, [NotNull] string contents,
            [CanBeNull] FileAttributes? attributes = null)
        {
            InnerIncludingFile(path, entry => entry.WriteToFile(contents), attributes);
            return this;
        }

        [NotNull]
        public DirectoryTreeBuilder IncludingBinaryFile([NotNull] string path, [NotNull] byte[] contents,
            [CanBeNull] FileAttributes? attributes = null)
        {
            InnerIncludingFile(path, entry => entry.WriteToFile(contents), attributes);
            return this;
        }

        private void InnerIncludingFile([NotNull] string path, [NotNull] Action<FileEntry> writeContentsToFile,
            [CanBeNull] FileAttributes? attributes)
        {
            var absolutePath = new AbsolutePath(path);
            FileEntry file = root.GetOrCreateFile(absolutePath, true);

            writeContentsToFile(file);

            if (attributes != null)
            {
                file.Attributes = attributes.Value;
            }
        }

        [NotNull]
        public DirectoryTreeBuilder IncludingDirectory([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            var absolutePath = new AbsolutePath(path);
            DirectoryEntry directory = root.CreateDirectory(absolutePath);

            if (attributes != null)
            {
                directory.Attributes = attributes.Value;
            }

            return this;
        }
    }
}
