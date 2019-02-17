using System;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class PathGetTempFileNameHandler : FakeOperationHandler<PathGetTempFileNameArguments, AbsolutePath>
    {
        private const int MaxNumber = 0xFFFF;

        [NotNull]
        private readonly Random randomNumberGenerator;

        public PathGetTempFileNameHandler([NotNull] DirectoryEntry root, [NotNull] Random randomNumberGenerator)
            : base(root)
        {
            Guard.NotNull(randomNumberGenerator, nameof(randomNumberGenerator));
            this.randomNumberGenerator = randomNumberGenerator;
        }

        public override AbsolutePath Handle(PathGetTempFileNameArguments arguments)
        {
            var resolver = new DirectoryResolver(Root)
            {
                ErrorDirectoryNotFound = _ => ErrorFactory.System.DirectoryNameIsInvalid(),
                ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNameIsInvalid(),
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNameIsInvalid(),
                ErrorNetworkShareNotFound = _ => ErrorFactory.System.DirectoryNameIsInvalid()
            };
            DirectoryEntry directory = resolver.ResolveDirectory(arguments.TempDirectory);

            int startIndex = randomNumberGenerator.Next(0, MaxNumber + 1);

            int index = startIndex;
            do
            {
                string fileName = "tmp" + index.ToString("X") + ".tmp";

                if (!directory.ContainsFile(fileName))
                {
                    FileEntry file = directory.CreateFile(fileName);
                    return file.PathFormatter.GetPath();
                }

                index = (index + 1) % MaxNumber;
            }
            while (index != startIndex);

            throw ErrorFactory.System.FileExists();
        }
    }
}
