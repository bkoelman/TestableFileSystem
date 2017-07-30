using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileMoveHandler : FakeOperationHandler<FileMoveArguments, object>
    {
        // TODO: Implement timings - https://support.microsoft.com/en-us/help/299648/description-of-ntfs-date-and-time-stamps-for-files-and-folders

        public FileMoveHandler([NotNull] FakeFileSystem fileSystem, [NotNull] DirectoryEntry root)
            : base(fileSystem, root)
        {
        }

        public override object Handle(FileMoveArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            AbsolutePath sourcePath = FileSystem.ToAbsolutePath(arguments.SourceFileName);
            AbsolutePath destinationPath = FileSystem.ToAbsolutePath(arguments.DestinationFileName);

            FileEntry sourceFile = ResolveSourceFile(sourcePath);
            AssertHasExclusiveAccess(sourceFile);

            DirectoryEntry destinationDirectory = ResolveDestinationDirectory(sourcePath, destinationPath, sourceFile);

            string newFileName = destinationPath.Components.Last();
            MoveFile(sourceFile, destinationDirectory, newFileName);

            return Missing.Value;
        }

        [NotNull]
        private FileEntry ResolveSourceFile([NotNull] AbsolutePath sourcePath)
        {
            var sourceResolver = new FileResolver(Root)
            {
                ErrorFileFoundAsDirectory = incomingPath => ErrorFactory.FileNotFound(incomingPath),
                ErrorDirectoryFoundAsFile = incomingPath => ErrorFactory.FileNotFound(incomingPath),
                ErrorLastDirectoryFoundAsFile = incomingPath => ErrorFactory.FileNotFound(incomingPath),
                ErrorDirectoryNotFound = incomingPath => ErrorFactory.FileNotFound(incomingPath),
                ErrorPathIsVolumeRoot = incomingPath => ErrorFactory.FileNotFound(incomingPath),
                ErrorNetworkShareNotFound = incomingPath => ErrorFactory.FileNotFound(incomingPath)
            };

            return sourceResolver.ResolveExistingFile(sourcePath);
        }

        [NotNull]
        private DirectoryEntry ResolveDestinationDirectory([NotNull] AbsolutePath sourcePath,
            [NotNull] AbsolutePath destinationPath, [NotNull] FileEntry sourceFile)
        {
            var destinationResolver = new FileResolver(Root)
            {
                ErrorFileFoundAsDirectory = _ => ErrorFactory.CannotCreateFileBecauseFileAlreadyExists(),
                ErrorDirectoryFoundAsFile = _ => ErrorFactory.DirectoryNotFound(),
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.ParameterIsIncorrect(),
                ErrorDirectoryNotFound = _ => ErrorFactory.DirectoryNotFound(),
                ErrorPathIsVolumeRoot = _ => ErrorFactory.FileOrDirectoryOrVolumeIsIncorrect(),
                ErrorFileExists = _ => ErrorFactory.CannotCreateFileBecauseFileAlreadyExists()
            };

            bool isFileCasingChangeOnly = IsFileCasingChangeOnly(sourcePath, destinationPath);

            return isFileCasingChangeOnly
                ? sourceFile.Parent
                : destinationResolver.ResolveContainingDirectoryForMissingFile(destinationPath);
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.FileIsInUse();
            }
        }

        private bool IsFileCasingChangeOnly([NotNull] AbsolutePath sourcePath, [NotNull] AbsolutePath destinationPath)
        {
            return sourcePath.Components.SequenceEqual(destinationPath.Components, StringComparer.OrdinalIgnoreCase);
        }

        private static void MoveFile([NotNull] FileEntry sourceFile, [NotNull] DirectoryEntry destinationDirectory,
            [NotNull] string newFileName)
        {
            sourceFile.Parent.Detach(sourceFile);
            sourceFile.MoveTo(newFileName, destinationDirectory);
            destinationDirectory.Attach(sourceFile);
        }
    }
}
