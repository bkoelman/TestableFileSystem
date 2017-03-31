using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Tests.Builders
{
    public sealed class FakeFileSystemBuilder : ITestDataBuilder<IFileSystem>
    {
        [NotNull]
        private readonly DirectoryTreeBuilder builder = new DirectoryTreeBuilder().IncludingDirectory("C:");

        public IFileSystem Build()
        {
            DirectoryEntry root = builder.Build();
            return new FakeFileSystem(root);
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingFile([NotNull] string path, [CanBeNull] string contents = null,
            [CanBeNull] FileAttributes? attributes = null)
        {
            builder.IncludingFile(path, contents, attributes);
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
