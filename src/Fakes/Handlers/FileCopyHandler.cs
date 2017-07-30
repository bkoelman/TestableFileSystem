using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileCopyHandler : FakeOperationHandler<FileCopyArguments, object>
    {
        public FileCopyHandler([NotNull] FakeFileSystem fileSystem, [NotNull] DirectoryEntry root)
            : base(fileSystem, root)
        {
        }

        public override object Handle(FileCopyArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            AbsolutePath sourcePath = FileSystem.ToAbsolutePath(arguments.SourceFileName);
            AbsolutePath destinationPath = FileSystem.ToAbsolutePath(arguments.DestinationFileName);

            FileEntry sourceFile = ResolveSourceFile(sourcePath);
            AssertHasExclusiveAccess(sourceFile);

            FileEntry destinationFile = ResolveDestinationFile(destinationPath, arguments.Overwrite);

            using (IFileStream sourceStream = sourceFile.Open(FileMode.Open, FileAccess.ReadWrite))
            {
                destinationFile.Attributes = sourceFile.Attributes;

                // TODO: Set timings.

                using (IFileStream destinationStream = destinationFile.Open(FileMode.Truncate, FileAccess.Write))
                {
                    destinationStream.SetLength(sourceStream.Length);

                    // TODO: Move copying of data outside FileSystem lock.

                    sourceStream.CopyTo(destinationStream.AsStream());
                }
            }

            return Missing.Value;
        }

        [NotNull]
        private FileEntry ResolveSourceFile([NotNull] AbsolutePath sourcePath)
        {
            var sourceResolver = new FileResolver(Root)
            {
                ErrorPathIsVolumeRoot = incomingPath => ErrorFactory.DirectoryNotFound(incomingPath)
            };

            return sourceResolver.ResolveExistingFile(sourcePath);
        }

        [NotNull]
        private FileEntry ResolveDestinationFile([NotNull] AbsolutePath destinationPath, bool overwrite)
        {
            var destinationResolver = new FileResolver(Root)
            {
                ErrorFileFoundAsDirectory = incomingPath => ErrorFactory.TargetIsNotFile(incomingPath)
            };

            (DirectoryEntry destinationDirectory, FileEntry destinationFileOrNull, string fileName) =
                destinationResolver.TryResolveFile(destinationPath);

            if (destinationFileOrNull != null)
            {
                AssertCanOverwriteFile(overwrite, destinationPath);
                AssertIsNotReadOnly(destinationFileOrNull, destinationPath);
            }

            return destinationFileOrNull ?? destinationDirectory.GetOrCreateFile(fileName);
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.FileIsInUse();
            }
        }

        [AssertionMethod]
        private void AssertCanOverwriteFile(bool overwrite, [NotNull] AbsolutePath path)
        {
            if (!overwrite)
            {
                throw ErrorFactory.FileAlreadyExists(path.GetText());
            }
        }

        [AssertionMethod]
        private void AssertIsNotReadOnly([NotNull] FileEntry file, [NotNull] AbsolutePath path)
        {
            if (file.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.UnauthorizedAccess(path.GetText());
            }
        }
    }
}
