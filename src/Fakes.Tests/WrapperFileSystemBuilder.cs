using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Tests
{
    internal sealed class WrapperFileSystemBuilder : IFileSystemBuilder
    {
        [NotNull]
        private readonly IFileSystem fileSystem;

        [NotNull]
        private readonly FileSystemBuilderFactory owner;

        public WrapperFileSystemBuilder([NotNull] IFileSystem fileSystem, [NotNull] FileSystemBuilderFactory owner)
        {
            Guard.NotNull(fileSystem, nameof(fileSystem));
            Guard.NotNull(owner, nameof(owner));

            this.fileSystem = fileSystem;
            this.owner = owner;
        }

        public IFileSystem Build()
        {
            return fileSystem;
        }

        public IFileSystemBuilder IncludingDirectory(string path, FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));

            CreateDirectory(path);

            if (attributes != null)
            {
                fileSystem.File.SetAttributes(path, attributes.Value);
            }

            return this;
        }

        public IFileSystemBuilder IncludingEmptyFile(string path, FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));

            IncludeFile(path, entry => { }, attributes);
            return this;
        }

        public IFileSystemBuilder IncludingTextFile(string path, string contents, Encoding encoding = null,
            FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNullNorEmpty(contents, nameof(contents));

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

        public IFileSystemBuilder IncludingBinaryFile(string path, byte[] contents, FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNullNorEmpty(contents, nameof(contents));

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
            CreateParentDirectories(path);

            using (IFileStream stream = fileSystem.File.Create(path))
            {
                writeContentsToStream(stream);
            }

            if (attributes != null)
            {
                fileSystem.File.SetAttributes(path, attributes.Value);
            }
        }

        private void CreateParentDirectories([NotNull] string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (directory != null)
            {
                CreateDirectory(directory);
            }
        }

        private void CreateDirectory([NotNull] string directory)
        {
            if (!directory.StartsWith(@"\\", StringComparison.Ordinal) &&
                !directory.StartsWith(Path.GetTempPath(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Error in test: Detected usage of non-mapped local path '{directory}'.");
            }

            owner.EnsureNetworkShareExists(directory);

            fileSystem.Directory.CreateDirectory(directory);
        }
    }
}
