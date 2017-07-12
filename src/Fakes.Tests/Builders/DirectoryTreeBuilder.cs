using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

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
            IncludeFile(path, entry => { }, attributes);
            return this;
        }

        [NotNull]
        public DirectoryTreeBuilder IncludingTextFile([NotNull] string path, [NotNull] string contents,
            [CanBeNull] FileAttributes? attributes = null)
        {
            IncludeFile(path, entry => WriteStringToFile(entry, contents), attributes);
            return this;
        }

        private static void WriteStringToFile([NotNull] IFileStream stream, [NotNull] string text)
        {
            using (var writer = new StreamWriter(stream.AsStream()))
            {
                writer.Write(text);
            }
        }

        [NotNull]
        public DirectoryTreeBuilder IncludingBinaryFile([NotNull] string path, [NotNull] byte[] contents,
            [CanBeNull] FileAttributes? attributes = null)
        {
            IncludeFile(path, entry => WriteBytesToFile(entry, contents), attributes);
            return this;
        }

        private static void WriteBytesToFile([NotNull] IFileStream stream, [NotNull] byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        private void IncludeFile([NotNull] string path, [NotNull] Action<IFileStream> writeContentsToStream,
            [CanBeNull] FileAttributes? attributes)
        {
            var absolutePath = new AbsolutePath(path);
            FileEntry file = root.GetOrCreateFile(absolutePath, true);

            using (IFileStream stream = file.Open(FileMode.Open, FileAccess.Write))
            {
                writeContentsToStream(stream);
            }

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
