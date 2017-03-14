using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Tests.Builders
{
    internal sealed class MemoryFileSystemBuilder : ITestDataBuilder<IFileSystem>
    {
        [NotNull]
        private readonly DirectoryTreeBuilder builder = new DirectoryTreeBuilder();

        public IFileSystem Build()
        {
            DirectoryEntry root = builder.Build();
            return new MemoryFileSystem(root);
        }

        [NotNull]
        public MemoryFileSystemBuilder IncludingFile([NotNull] string path)
        {
            builder.IncludingFile(path);
            return this;
        }

        [NotNull]
        public MemoryFileSystemBuilder IncludingDirectory([NotNull] string path)
        {
            builder.IncludingDirectory(path);
            return this;
        }
    }
}
