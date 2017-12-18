using System;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Handlers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Builders
{
    public sealed class FakeFileSystemBuilder : ITestDataBuilder<FakeFileSystem>
    {
        private bool includeDriveC = true;

        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        private WaitIndicator copyWaitIndicator = WaitIndicator.None;

        public FakeFileSystemBuilder()
            : this(new SystemClock())
        {
        }

        public FakeFileSystemBuilder([NotNull] SystemClock systemClock)
        {
            Guard.NotNull(systemClock, nameof(systemClock));

            root = DirectoryEntry.CreateRoot(systemClock);
        }

        public FakeFileSystem Build()
        {
            if (includeDriveC)
            {
                IncludingDirectory("C:");
            }

            copyWaitIndicator.Reset();
            return new FakeFileSystem(root, copyWaitIndicator);
        }

        [NotNull]
        public FakeFileSystemBuilder WithoutDefaultDriveC()
        {
            includeDriveC = false;
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingDirectory([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));

            var absolutePath = new AbsolutePath(path);
            DirectoryEntry directory = CreateDirectories(absolutePath);

            if (attributes != null)
            {
                directory.Attributes = attributes.Value;
            }

            return this;
        }

        [NotNull]
        private DirectoryEntry CreateDirectories([NotNull] AbsolutePath absolutePath)
        {
            var arguments = new DirectoryCreateArguments(absolutePath, true);
            var handler = new DirectoryCreateHandler(root, new FileSystemChangeTracker());

            return handler.Handle(arguments);
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

            DirectoryEntry directory = CreateParentDirectories(absolutePath);

            string fileName = absolutePath.Components.Last();
            AssertIsNotDirectory(fileName, directory, absolutePath);

            FileEntry file = directory.Files.ContainsKey(fileName) ? directory.Files[fileName] : directory.CreateFile(fileName);

            using (IFileStream stream = file.Open(FileMode.Truncate, FileAccess.Write, absolutePath))
            {
                writeContentsToStream(stream);
            }

            if (attributes != null)
            {
                file.Attributes = attributes.Value;
            }
        }

        [NotNull]
        private DirectoryEntry CreateParentDirectories([NotNull] AbsolutePath absolutePath)
        {
            AbsolutePath parentPath = absolutePath.TryGetParentPath();
            if (parentPath == null)
            {
                throw ErrorFactory.System.DirectoryNotFound(absolutePath.GetText());
            }

            return CreateDirectories(parentPath);
        }

        [AssertionMethod]
        private static void AssertIsNotDirectory([NotNull] string fileName, [NotNull] DirectoryEntry directory,
            [NotNull] AbsolutePath absolutePath)
        {
            if (directory.Directories.ContainsKey(fileName))
            {
                throw ErrorFactory.System.CannotCreateBecauseFileOrDirectoryAlreadyExists(absolutePath.GetText());
            }
        }

        [NotNull]
        public FakeFileSystemBuilder WithCopyWaitIndicator([NotNull] WaitIndicator waitIndicator)
        {
            Guard.NotNull(waitIndicator, nameof(waitIndicator));

            copyWaitIndicator = waitIndicator;
            return this;
        }
    }
}
