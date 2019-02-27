using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class DirectoryEnumerateEntriesArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        [NotNull]
        public string IncomingPath { get; }

        [NotNull]
        public string SearchPattern { get; }

        public SearchOption SearchOption { get; }
        public EnumerationFilter Filter { get; }

        public DirectoryEnumerateEntriesArguments([NotNull] AbsolutePath path, [NotNull] string incomingPath,
            [NotNull] string searchPattern, SearchOption searchOption, EnumerationFilter filter)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(incomingPath, nameof(incomingPath));
            Guard.NotNull(searchPattern, nameof(searchPattern));

            Path = path;
            IncomingPath = incomingPath;
            SearchPattern = searchPattern;
            SearchOption = searchOption;
            Filter = filter;
        }
    }
}
