﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryEnumerateEntriesHandler : FakeOperationHandler<DirectoryEnumerateEntriesArguments, string[]>
    {
        public DirectoryEnumerateEntriesHandler([NotNull] FakeFileSystem fileSystem, [NotNull] DirectoryEntry root)
            : base(fileSystem, root)
        {
        }

        [ItemNotNull]
        public override string[] Handle(DirectoryEnumerateEntriesArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            AbsolutePath absolutePath = FileSystem.ToAbsolutePath(arguments.Path);
            DirectoryEntry directory = ResolveDirectory(absolutePath);

            PathPattern pattern = PathPattern.Create(arguments.SearchPattern);

            IEnumerable<string> absoluteNames =
                EnumerateEntriesInDirectory(directory, pattern, absolutePath, arguments.SearchOption, arguments.Filter);

            return ToRelativeNames(absoluteNames, absolutePath, arguments.Path).ToArray();
        }

        [NotNull]
        private DirectoryEntry ResolveDirectory([NotNull] AbsolutePath absolutePath)
        {
            var resolver = new DirectoryResolver(Root)
            {
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.DirectoryNameIsInvalid()
            };

            return resolver.ResolveDirectory(absolutePath, absolutePath.GetText());
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<string> EnumerateEntriesInDirectory([NotNull] DirectoryEntry directory, [NotNull] PathPattern pattern,
            [NotNull] AbsolutePath directoryPath, SearchOption searchOption, EnumerationFilter filter)
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