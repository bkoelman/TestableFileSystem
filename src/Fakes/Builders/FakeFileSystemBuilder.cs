using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Builders
{
    public sealed class FakeFileSystemBuilder : ITestDataBuilder<FakeFileSystem>
    {
        // TODO: Consider allowing to set attributes like Compressed/Encrypted/... from this builder.

        private bool includeDriveC = true;

        [NotNull]
        private readonly DirectoryEntry root = DirectoryEntry.CreateRoot();

        [NotNull]
        private WaitIndicator copyWaitIndicator = WaitIndicator.None;

        public FakeFileSystem Build()
        {
            if (includeDriveC)
            {
                IncludingDirectory("C:");
            }

            return new FakeFileSystem(root, copyWaitIndicator);
        }

        [NotNull]
        public FakeFileSystemBuilder WithoutDefaultDriveC()
        {
            includeDriveC = false;
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder WithCopyWaitIndicator([NotNull] WaitIndicator waitIndicator)
        {
            Guard.NotNull(waitIndicator, nameof(waitIndicator));

            copyWaitIndicator = waitIndicator;
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingDirectory([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));

            var absolutePath = new AbsolutePath(path);
            var navigator = new PathNavigator(absolutePath);

            DirectoryEntry directory = root.CreateDirectories(navigator);

            if (attributes != null)
            {
                directory.Attributes = attributes.Value;
            }

            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingEmptyFile([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));

            IncludeFile(path, entry => { }, attributes);
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingTextFile([NotNull] string path, [NotNull] string contents,
            [CanBeNull] Encoding encoding = null, [CanBeNull] FileAttributes? attributes = null)
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

        [NotNull]
        public FakeFileSystemBuilder IncludingBinaryFile([NotNull] string path, [NotNull] byte[] contents,
            [CanBeNull] FileAttributes? attributes = null)
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
    }
}
