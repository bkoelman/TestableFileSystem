using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class DirectoryEnumerateEntriesArguments
    {
        [NotNull]
        public string Path { get; }

        [NotNull]
        public string SearchPattern { get; }

        public SearchOption SearchOption { get; }
        public EnumerationFilter Filter { get; }

        public DirectoryEnumerateEntriesArguments([NotNull] string path, [NotNull] string searchPattern,
            SearchOption searchOption, EnumerationFilter filter)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(searchPattern, nameof(searchPattern));

            Path = path;
            SearchPattern = searchPattern;
            SearchOption = searchOption;
            Filter = filter;
        }
    }
}
