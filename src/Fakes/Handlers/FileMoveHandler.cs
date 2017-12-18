using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileMoveHandler : FakeOperationHandler<EntryMoveArguments, object>
    {
        public FileMoveHandler([NotNull] DirectoryEntry root, [NotNull] FileSystemChangeTracker changeTracker)
            : base(root, changeTracker)
        {
        }

        public override object Handle(EntryMoveArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            FileEntry sourceFile = ResolveSourceFile(arguments.SourcePath);
            AssertHasExclusiveAccess(sourceFile);

            DirectoryEntry destinationDirectory =
                ResolveDestinationDirectory(arguments.SourcePath, arguments.DestinationPath, sourceFile);

            string newFileName = arguments.DestinationPath.Components.Last();
            MoveFile(sourceFile, destinationDirectory, newFileName);

            return Missing.Value;
        }

        [NotNull]
        private FileEntry ResolveSourceFile([NotNull] AbsolutePath sourcePath)
        {
            var sourceResolver = new FileResolver(Root)
            {
                ErrorFileFoundAsDirectory = ErrorFactory.System.FileNotFound,
                ErrorDirectoryFoundAsFile = ErrorFactory.System.FileNotFound,
                ErrorLastDirectoryFoundAsFile = ErrorFactory.System.FileNotFound,
                ErrorDirectoryNotFound = ErrorFactory.System.FileNotFound,
                ErrorPathIsVolumeRoot = ErrorFactory.System.FileNotFound,
                ErrorNetworkShareNotFound = ErrorFactory.System.FileNotFound
            };

            return sourceResolver.ResolveExistingFile(sourcePath);
        }

        [NotNull]
        private DirectoryEntry ResolveDestinationDirectory([NotNull] AbsolutePath sourcePath,
            [NotNull] AbsolutePath destinationPath, [NotNull] FileEntry sourceFile)
        {
            var destinationResolver = new FileResolver(Root)
            {
                ErrorFileFoundAsDirectory = _ => ErrorFactory.System.CannotCreateFileBecauseFileAlreadyExists(),
                ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound(),
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.ParameterIsIncorrect(),
                ErrorDirectoryNotFound = _ => ErrorFactory.System.DirectoryNotFound(),
                ErrorPathIsVolumeRoot = _ => ErrorFactory.System.FileOrDirectoryOrVolumeIsIncorrect(),
                ErrorFileExists = _ => ErrorFactory.System.CannotCreateFileBecauseFileAlreadyExists()
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
                throw ErrorFactory.System.FileIsInUse();
            }
        }

        private bool IsFileCasingChangeOnly([NotNull] AbsolutePath sourcePath, [NotNull] AbsolutePath destinationPath)
        {
            return sourcePath.Components.SequenceEqual(destinationPath.Components, StringComparer.OrdinalIgnoreCase);
        }

        private static void MoveFile([NotNull] FileEntry sourceFile, [NotNull] DirectoryEntry destinationDirectory,
            [NotNull] string newFileName)
        {
            sourceFile.Parent.DeleteFile(sourceFile.Name);
            destinationDirectory.MoveFileToHere(sourceFile, newFileName);
        }
    }
}
