using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryEnumerateEntriesHandler : FakeOperationHandler<DirectoryEnumerateEntriesArguments, string[]>
    {
        public DirectoryEnumerateEntriesHandler([NotNull] DirectoryEntry root, [NotNull] FileSystemChangeTracker changeTracker)
            : base(root, changeTracker)
        {
        }

        [ItemNotNull]
        public override string[] Handle(DirectoryEnumerateEntriesArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            DirectoryEntry directory = ResolveDirectory(arguments.Path);

            PathPattern pattern = PathPattern.Create(arguments.SearchPattern);

            IEnumerable<string> absoluteNames =
                EnumerateEntriesInDirectory(directory, pattern, arguments.Path, arguments.SearchOption, arguments.Filter);

            return ToRelativeNames(absoluteNames, arguments.Path, arguments.IncomingPath).ToArray();
        }

        [NotNull]
        private DirectoryEntry ResolveDirectory([NotNull] AbsolutePath absolutePath)
        {
            var resolver = new DirectoryResolver(Root)
            {
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNameIsInvalid()
            };

            return resolver.ResolveDirectory(absolutePath);
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<string> EnumerateEntriesInDirectory([NotNull] DirectoryEntry directory, [NotNull] PathPattern pattern,
            [NotNull] AbsolutePath directoryPath, SearchOption searchOption, EnumerationFilter filter)
        {
            PathPattern subPattern = pattern.SubPattern;

            if (subPattern == null)
            {
                foreach (BaseEntry entry in directory.EnumerateEntries(filter).Where(x => pattern.IsMatch(x.Name))
                    .OrderBy(x => x.Name))
                {
                    string basePath = directoryPath.GetText();
                    yield return Path.Combine(basePath, entry.Name);
                }

                if (searchOption == SearchOption.AllDirectories)
                {
                    foreach (DirectoryEntry subdirectory in directory.Directories.Values.OrderBy(x => x.Name))
                    {
                        AbsolutePath subdirectoryPath = directoryPath.Append(subdirectory.Name);
                        foreach (string nextPath in EnumerateEntriesInDirectory(subdirectory, pattern, subdirectoryPath,
                            searchOption, filter))
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
                        foreach (string nextPath in EnumerateEntriesInDirectory(subdirectory, subPattern, subdirectoryPath,
                            searchOption, filter))
                        {
                            yield return nextPath;
                        }
                    }
                }
            }
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<string> ToRelativeNames([NotNull] [ItemNotNull] IEnumerable<string> absoluteNames,
            [NotNull] AbsolutePath absolutePath, [NotNull] string incomingPath)
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
