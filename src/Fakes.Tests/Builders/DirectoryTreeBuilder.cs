using System.IO;
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
        public DirectoryTreeBuilder IncludingFile([NotNull] string path, [CanBeNull] string contents = null,
            [CanBeNull] FileAttributes? attributes = null)
        {
            var absolutePath = new AbsolutePath(path);
            FileEntry file = root.GetOrCreateFile(absolutePath, true);

            if (contents != null)
            {
                file.WriteToFile(contents);
            }

            if (attributes != null)
            {
                file.Attributes = attributes.Value;
            }

            return this;
        }

        [NotNull]
        public DirectoryTreeBuilder IncludingDirectory([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            var absolutePath = new AbsolutePath(path);
            DirectoryEntry directory = root.CreateDirectory(absolutePath);

            if (attributes != null)
            {
                directory.Attributes = attributes.Value;
            }

            return this;
        }
    }
}
