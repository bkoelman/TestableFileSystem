using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Builders
{
    public sealed class FakeFileSystemBuilder : ITestDataBuilder<FakeFileSystem>
    {
        [NotNull]
        private readonly DirectoryTreeBuilder builder = new DirectoryTreeBuilder().IncludingDirectory("C:");

        public FakeFileSystem Build()
        {
            DirectoryEntry root = builder.Build();
            return new FakeFileSystem(root);
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingEmptyFile([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            builder.IncludingEmptyFile(path, attributes);
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingTextFile([NotNull] string path, [NotNull] string contents,
            [CanBeNull] FileAttributes? attributes = null)
        {
            builder.IncludingTextFile(path, contents, attributes);
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingBinaryFile([NotNull] string path, [NotNull] byte[] contents,
            [CanBeNull] FileAttributes? attributes = null)
        {
            builder.IncludingBinaryFile(path, contents, attributes);
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingDirectory([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            builder.IncludingDirectory(path, attributes);
            return this;
        }
    }
}
