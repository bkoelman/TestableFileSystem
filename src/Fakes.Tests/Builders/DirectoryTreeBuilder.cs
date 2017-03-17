using JetBrains.Annotations;
using TestableFileSystem.Fakes.Tests.Utilities;

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
        public DirectoryTreeBuilder IncludingFile([NotNull] string path, [CanBeNull] string contents = null)
        {
            var absolutePath = new AbsolutePath(path);
            FileEntry file = root.GetOrCreateFile(absolutePath, true);

            if (contents != null)
            {
                file.WriteToFile(contents);
            }

            return this;
        }

        [NotNull]
        public DirectoryTreeBuilder IncludingDirectory([NotNull] string path)
        {
            var absolutePath = new AbsolutePath(path);
            root.CreateDirectory(absolutePath);
            return this;
        }
    }
}
