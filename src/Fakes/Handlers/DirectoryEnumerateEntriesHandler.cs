using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly string incomingPath;

        [NotNull]
        private readonly AbsolutePath absolutePath;

        [NotNull]
        private readonly string searchPattern;

        private readonly SearchOption searchOption;
        private readonly EnumerationFilter filter;

        public DirectoryEnumerateEntriesHandler([NotNull] FakeFileSystem fileSystem, [NotNull] DirectoryEntry root,
            [NotNull] string incomingPath, [NotNull] string searchPattern, SearchOption searchOption, EnumerationFilter filter)
        {
            Guard.NotNull(fileSystem, nameof(fileSystem));
            Guard.NotNull(root, nameof(root));
            Guard.NotNull(incomingPath, nameof(incomingPath));
            Guard.NotNull(searchPattern, nameof(searchPattern));

            absolutePath = fileSystem.ToAbsolutePath(incomingPath);

            this.root = root;
            this.incomingPath = incomingPath;
            this.searchPattern = searchPattern;
            this.searchOption = searchOption;
            this.filter = filter;
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<string> Handle()
        {
            DirectoryEntry directory = ResolveDirectory();

            PathPattern pattern = PathPattern.Create(searchPattern);

            IEnumerable<string> absoluteNames = EnumerateEntriesInDirectory(directory, pattern, absolutePath);

            return ToRelativeNames(absoluteNames);
        }

        [NotNull]
        private DirectoryEntry ResolveDirectory()
        {
            var resolver = new DirectoryResolver(root)
            {
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.DirectoryNameIsInvalid()
            };

            return resolver.ResolveDirectory(absolutePath, absolutePath.GetText());
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<string> EnumerateEntriesInDirectory([NotNull] DirectoryEntry directory, [NotNull] PathPattern pattern,
            [NotNull] AbsolutePath directoryPath)
        {
            PathPattern subPattern = pattern.SubPattern;

            if (subPattern == null)
            {
                foreach (BaseEntry entry in directory.GetEntries(filter).Where(x => pattern.IsMatch(x.Name)).OrderBy(x => x.Name))
                {
                    string basePath = directoryPath.GetText();
                    yield return Path.Combine(basePath, entry.Name);
                }

                if (searchOption == SearchOption.AllDirectories)
                {
                    foreach (DirectoryEntry subdirectory in directory.Directories.Values.OrderBy(x => x.Name))
                    {
                        AbsolutePath subdirectoryPath = directoryPath.Append(subdirectory.Name);
                        foreach (string nextPath in EnumerateEntriesInDirectory(subdirectory, pattern, subdirectoryPath))
                        {
                            yield return nextPath;
                        }
                    }
                }
            }
            else
            {
                foreach (DirectoryEntry subdirectory in directory.Directories.Values)
                {
                    if (pattern.IsMatch(subdirectory.Name))
                    {
                        AbsolutePath subdirectoryPath = directoryPath.Append(subdirectory.Name);
                        foreach (string nextPath in EnumerateEntriesInDirectory(subdirectory, subPattern, subdirectoryPath))
                        {
                            yield return nextPath;
                        }
                    }
                }
            }
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<string> ToRelativeNames([NotNull] [ItemNotNull] IEnumerable<string> absoluteNames)
        {
            string basePath = incomingPath.TrimEnd();
            int absolutePathLength = absolutePath.GetText().Length;

            foreach (string absoluteName in absoluteNames)
            {
                string relativeName = absoluteName.Substring(absolutePathLength);
                yield return basePath + relativeName;
            }
        }
    }
}
