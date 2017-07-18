using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Builders
{
    // TODO: Consider inlining this type, after moving specs that depend on it.
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
            [CanBeNull] Encoding encoding = null, [CanBeNull] FileAttributes? attributes = null)
        {
            IncludeFile(path, entry => WriteStringToFile(entry, contents, encoding), attributes);
            return this;
        }

        private static void WriteStringToFile([NotNull] IFileStream fileStream, [NotNull] string text,
            [CanBeNull] Encoding encoding)
        {
            Stream stream = fileStream.AsStream();
            using (StreamWriter writer = encoding == null ? new StreamWriter(stream) : new StreamWriter(stream, encoding))
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
            var navigator = new PathNavigator(absolutePath);

            AssertDoesNotExistAsDirectory(absolutePath);
            RemoveExistingFile(absolutePath);

            FileEntry file = root.GetOrCreateFile(navigator, true);

            using (IFileStream stream = file.Open(FileMode.Open, FileAccess.Write))
            {
                writeContentsToStream(stream);
            }

            if (attributes != null)
            {
                file.Attributes = attributes.Value;
            }
        }

        private void AssertDoesNotExistAsDirectory([NotNull] AbsolutePath path)
        {
            var navigator = new PathNavigator(path);
            DirectoryEntry directory = root.TryGetExistingDirectory(navigator);
            if (directory != null)
            {
                throw ErrorFactory.CannotCreateBecauseFileOrDirectoryAlreadyExists(path.GetText());
            }
        }

        private void RemoveExistingFile([NotNull] AbsolutePath absolutePath)
        {
            var navigator = new PathNavigator(absolutePath);
            FileEntry file = root.TryGetExistingFile(navigator);
            if (file != null)
            {
                root.DeleteFile(navigator);
            }
        }

        [NotNull]
        public DirectoryTreeBuilder IncludingDirectory([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            var absolutePath = new AbsolutePath(path);
            var navigator = new PathNavigator(absolutePath);

            DirectoryEntry directory = root.CreateDirectories(navigator);

            if (attributes != null)
            {
                directory.Attributes = attributes.Value;
            }

            return this;
        }
    }
}
