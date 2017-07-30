using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryEnumerateEntriesHandler
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        private readonly string path;

        [NotNull]
        private readonly AbsolutePath absolutePath;

        [NotNull]
        private readonly string searchPattern;

        private readonly SearchOption searchOption;
        private readonly EnumerationFilter filter;

        public DirectoryEnumerateEntriesHandler([NotNull] FakeFileSystem fileSystem, [NotNull] DirectoryEntry root,
            [NotNull] string path, [NotNull] string searchPattern, SearchOption searchOption, EnumerationFilter filter)
        {
            Guard.NotNull(fileSystem, nameof(fileSystem));
            Guard.NotNull(root, nameof(root));
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(searchPattern, nameof(searchPattern));

            absolutePath = fileSystem.ToAbsolutePath(path);

            this.root = root;
            this.path = path;
            this.searchPattern = searchPattern;
            this.searchOption = searchOption;
            this.filter = filter;
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<string> Handle()
        {
            var resolver = new DirectoryResolver(root)
            {
                ErrorLastDirectoryFoundAsFile = incomingPath => ErrorFactory.DirectoryNameIsInvalid()
            };

            DirectoryEntry directory = resolver.ResolveDirectory(absolutePath, absolutePath.GetText());

            IEnumerable<string> absoluteNames =
                root.EnumerateEntries(directory, searchPattern, searchOption, filter, absolutePath);
            return ToRelativeNames(absoluteNames);
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<string> ToRelativeNames([NotNull] [ItemNotNull] IEnumerable<string> absoluteNames)
        {
            string basePath = path.TrimEnd();
            int absolutePathLength = absolutePath.GetText().Length;

            foreach (string absoluteName in absoluteNames)
            {
                string relativeName = absoluteName.Substring(absolutePathLength);
                yield return basePath + relativeName;
            }
        }
    }
}
