using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryMoveHandler : FakeOperationHandler<EntryMoveArguments, object>
    {
        [NotNull]
        private readonly CurrentDirectoryManager currentDirectoryManager;

        public DirectoryMoveHandler([NotNull] DirectoryEntry root, [NotNull] FileSystemChangeTracker changeTracker,
            [NotNull] CurrentDirectoryManager currentDirectoryManager)
            : base(root, changeTracker)
        {
            Guard.NotNull(currentDirectoryManager, nameof(currentDirectoryManager));

            this.currentDirectoryManager = currentDirectoryManager;
        }

        public override object Handle(EntryMoveArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));
            AssertMovingToDifferentDirectoryOnSameVolume(arguments);
            AssertSourceIsNotVolumeRoot(arguments.SourcePath);

            BaseEntry sourceFileOrDirectory = ResolveSourceFileOrDirectory(arguments.SourcePath);

            if (sourceFileOrDirectory is DirectoryEntry sourceDirectory)
            {
                AssertNoConflictWithCurrentDirectory(sourceDirectory);
                AssertDirectoryContainsNoOpenFiles(sourceDirectory, arguments.SourcePath);

                DirectoryEntry destinationDirectory = ResolveDestinationDirectory(arguments.DestinationPath);
                AssertDestinationIsNotDescendantOfSource(destinationDirectory, sourceDirectory);

                string newDirectoryName = arguments.DestinationPath.Components.Last();
                MoveDirectory(sourceDirectory, destinationDirectory, newDirectoryName);
            }
            else if (sourceFileOrDirectory is FileEntry sourceFile)
            {
                AssertHasExclusiveAccess(sourceFile);

                DirectoryEntry destinationDirectory = ResolveDestinationDirectory(arguments.DestinationPath);

                string newFileName = arguments.DestinationPath.Components.Last();
                MoveFile(sourceFile, destinationDirectory, newFileName);
            }

            return Missing.Value;
        }

        private void AssertMovingToDifferentDirectoryOnSameVolume([NotNull] EntryMoveArguments arguments)
        {
            if (string.Equals(arguments.SourcePath.GetText(), arguments.DestinationPath.GetText(),
                StringComparison.OrdinalIgnoreCase))
            {
                throw ErrorFactory.System.DestinationMustBeDifferentFromSource();
            }

            string sourceRoot = GetPathRoot(arguments.SourcePath);
            string destinationRoot = GetPathRoot(arguments.DestinationPath);

            if (!string.Equals(sourceRoot, destinationRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw ErrorFactory.System.RootsMustBeIdentical();
            }
        }

        [NotNull]
        private static string GetPathRoot([NotNull] AbsolutePath path)
        {
            return path.GetAncestorPath(0).GetText();
        }

        private static void AssertSourceIsNotVolumeRoot([NotNull] AbsolutePath path)
        {
            if (path.IsVolumeRoot)
            {
                throw ErrorFactory.System.AccessDenied(path.GetText());
            }
        }

        [NotNull]
        private BaseEntry ResolveSourceFileOrDirectory([NotNull] AbsolutePath path)
        {
            var resolver = new DirectoryResolver(Root)
            {
                ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound(),
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound()
            };
            DirectoryEntry parentDirectory = resolver.ResolveDirectory(path.TryGetParentPath(), path.GetText());

            string fileOrDirectoryName = path.Components.Last();

            if (parentDirectory.Directories.ContainsKey(fileOrDirectoryName))
            {
                return parentDirectory.Directories[fileOrDirectoryName];
            }

            if (parentDirectory.Files.ContainsKey(fileOrDirectoryName))
            {
                return parentDirectory.Files[fileOrDirectoryName];
            }

            throw ErrorFactory.System.DirectoryNotFound(path.GetText());
        }

        [AssertionMethod]
        private void AssertNoConflictWithCurrentDirectory([NotNull] DirectoryEntry directory)
        {
            if (currentDirectoryManager.IsAtOrAboveCurrentDirectory(directory))
            {
                throw ErrorFactory.System.FileIsInUse();
            }
        }

        [AssertionMethod]
        private static void AssertDirectoryContainsNoOpenFiles([NotNull] DirectoryEntry directory, [NotNull] AbsolutePath path)
        {
            AbsolutePath openFilePath = directory.TryGetPathOfFirstOpenFile(path);
            if (openFilePath != null)
            {
                throw ErrorFactory.System.AccessDenied(path.GetText());
            }
        }

        [NotNull]
        private DirectoryEntry ResolveDestinationDirectory([NotNull] AbsolutePath path)
        {
            AbsolutePath parentPath = AssertDestinationIsNotVolumeRoot(path);

            var resolver = new DirectoryResolver(Root)
            {
                ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound(),
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.ParameterIsIncorrect(),
                ErrorDirectoryNotFound = _ => ErrorFactory.System.DirectoryNotFound()
            };
            DirectoryEntry parentDirectory = resolver.ResolveDirectory(parentPath);

            string directoryName = path.Components.Last();
            AssertDestinationDoesNotExist(directoryName, parentDirectory);

            return parentDirectory;
        }

        [NotNull]
        private static AbsolutePath AssertDestinationIsNotVolumeRoot([NotNull] AbsolutePath path)
        {
            AbsolutePath parentPath = path.TryGetParentPath();
            if (parentPath == null)
            {
                throw ErrorFactory.System.FileOrDirectoryOrVolumeIsIncorrect();
            }

            return parentPath;
        }

        private static void AssertDestinationDoesNotExist([NotNull] string directoryName,
            [NotNull] DirectoryEntry parentDirectory)
        {
            if (parentDirectory.Directories.ContainsKey(directoryName) || parentDirectory.Files.ContainsKey(directoryName))
            {
                throw ErrorFactory.System.CannotCreateFileBecauseFileAlreadyExists();
            }
        }

        private void AssertDestinationIsNotDescendantOfSource([NotNull] DirectoryEntry destinationDirectory,
            [NotNull] DirectoryEntry sourceDirectory)
        {
            DirectoryEntry current = destinationDirectory;

            do
            {
                if (current == sourceDirectory)
                {
                    throw ErrorFactory.System.FileIsInUse();
                }

                current = current.Parent;
            }
            while (current != null);
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.System.FileIsInUse();
            }
        }

        private static void MoveDirectory([NotNull] DirectoryEntry sourceDirectory, [NotNull] DirectoryEntry destinationDirectory,
            [NotNull] string newDirectoryName)
        {
            sourceDirectory.Parent?.DeleteDirectory(sourceDirectory.Name);
            destinationDirectory.MoveDirectoryToHere(sourceDirectory, newDirectoryName);
        }

        private static void MoveFile([NotNull] FileEntry sourceFile, [NotNull] DirectoryEntry destinationDirectory,
            [NotNull] string newFileName)
        {
            sourceFile.Parent.DeleteFile(sourceFile.Name);
            destinationDirectory.MoveFileToHere(sourceFile, newFileName);
        }
    }
}
