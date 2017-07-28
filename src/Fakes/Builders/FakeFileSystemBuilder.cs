using System.IO;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Builders
{
    public sealed class FakeFileSystemBuilder : ITestDataBuilder<FakeFileSystem>
    {
        // TODO: Consider allowing to set attributes like Compressed/Encrypted/... from here.

        private bool includeDriveC = true;

        [NotNull]
        private readonly DirectoryTreeBuilder builder = new DirectoryTreeBuilder();

        public FakeFileSystem Build()
        {
            if (includeDriveC)
            {
                builder.IncludingDirectory("C:");
            }

            DirectoryEntry root = builder.Build();
            return new FakeFileSystem(root);
        }

        [NotNull]
        public FakeFileSystemBuilder WithoutDefaultDriveC()
        {
            includeDriveC = false;
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingEmptyFile([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));

            builder.IncludingEmptyFile(path, attributes);
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingTextFile([NotNull] string path, [NotNull] string contents,
            [CanBeNull] Encoding encoding = null, [CanBeNull] FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNullNorEmpty(contents, nameof(contents));

            builder.IncludingTextFile(path, contents, encoding, attributes);
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingBinaryFile([NotNull] string path, [NotNull] byte[] contents,
            [CanBeNull] FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNullNorEmpty(contents, nameof(contents));

            builder.IncludingBinaryFile(path, contents, attributes);
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingDirectory([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));

            builder.IncludingDirectory(path, attributes);
            return this;
        }
    }
}
