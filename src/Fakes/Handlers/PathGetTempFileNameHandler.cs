using System;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class PathGetTempFileNameHandler : FakeOperationHandler<PathGetTempFileNameArguments, AbsolutePath>
    {
        private const int MaxNumber = 0xFFFF;

        [NotNull]
        private readonly Random randomNumberGenerator;

        public PathGetTempFileNameHandler([NotNull] VolumeContainer container, [NotNull] Random randomNumberGenerator)
            : base(container)
        {
            Guard.NotNull(randomNumberGenerator, nameof(randomNumberGenerator));
            this.randomNumberGenerator = randomNumberGenerator;
        }

        public override AbsolutePath Handle(PathGetTempFileNameArguments arguments)
        {
            DirectoryEntry tempDirectory = ResolveDirectory(arguments.TempDirectory);

            int startIndex = randomNumberGenerator.Next(0, MaxNumber + 1);

            string tempFileName = GetFirstAvailableFileName(tempDirectory, startIndex);
            FileEntry file = tempDirectory.CreateFile(tempFileName);
            return file.PathFormatter.GetPath();
        }

        [NotNull]
        private DirectoryEntry ResolveDirectory([NotNull] AbsolutePath tempDirectory)
        {
            var resolver = new DirectoryResolver(Container)
            {
                ErrorDirectoryNotFound = _ => ErrorFactory.System.DirectoryNameIsInvalid(),
                ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNameIsInvalid(),
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNameIsInvalid(),
                ErrorNetworkShareNotFound = _ => ErrorFactory.System.DirectoryNameIsInvalid()
            };

            return resolver.ResolveDirectory(tempDirectory);
        }

        [NotNull]
        private string GetFirstAvailableFileName([NotNull] DirectoryEntry tempDirectory, int startIndex)
        {
            int index = startIndex;
            do
            {
                string fileName = "tmp" + index.ToString("X") + ".tmp";

                if (!tempDirectory.ContainsFile(fileName))
                {
                    return fileName;
                }

                index = (index + 1) % MaxNumber;
            }
            while (index != startIndex);

            throw ErrorFactory.System.FileExists();
        }
    }
}
