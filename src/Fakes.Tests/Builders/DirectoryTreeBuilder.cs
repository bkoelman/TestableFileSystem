using JetBrains.Annotations;

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
        public DirectoryTreeBuilder IncludingFile([NotNull] string path)
        {
            AbsolutePath absolutePath = new AbsolutePath(path);
            root.GetOrCreateFile(absolutePath, true);
            return this;
        }

        [NotNull]
        public DirectoryTreeBuilder IncludingDirectory([NotNull] string path)
        {
            AbsolutePath absolutePath = new AbsolutePath(path);
            root.CreateDirectory(absolutePath);
            return this;
        }
    }
}
