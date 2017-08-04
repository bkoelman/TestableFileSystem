using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryMoveHandler : FakeOperationHandler<DirectoryOrFileMoveArguments, object>
    {
        [NotNull]
        private readonly CurrentDirectoryManager currentDirectoryManager;

        public DirectoryMoveHandler([NotNull] DirectoryEntry root, [NotNull] CurrentDirectoryManager currentDirectoryManager)
            : base(root)
        {
            Guard.NotNull(currentDirectoryManager, nameof(currentDirectoryManager));

            this.currentDirectoryManager = currentDirectoryManager;
        }

        public override object Handle(DirectoryOrFileMoveArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));
            AssertMovingToDifferentDirectoryOnSameVolume(arguments);
            AssertSourceIsNotVolumeRoot(arguments.SourcePath);

            DirectoryEntry sourceDirectory = ResolveSourceDirectory(arguments.SourcePath);
            AssertNoConflictWithCurrentDirectory(sourceDirectory);
            AssertDirectoryContainsNoOpenFiles(sourceDirectory, arguments.SourcePath);

            DirectoryEntry destinationDirectory = ResolveDestinationDirectory(arguments.DestinationPath);
            AssertDestinationIsNotDescendantOfSource(destinationDirectory, sourceDirectory);

            string newDirectoryName = arguments.DestinationPath.Components.Last();
            MoveDirectory(sourceDirectory, destinationDirectory, newDirectoryName);

            return Missing.Value;
        }

        private void AssertMovingToDifferentDirectoryOnSameVolume([NotNull] DirectoryOrFileMoveArguments arguments)
        {
            if (string.Equals(arguments.SourcePath.GetText(), arguments.DestinationPath.GetText(),
                StringComparison.OrdinalIgnoreCase))
            {
                throw ErrorFactory.System.DestinationMustBeDifferentFromSource();
            }

            if (!string.Equals(arguments.SourcePath.GetRootName(), arguments.DestinationPath.GetRootName(),
                StringComparison.OrdinalIgnoreCase))
            {
                throw ErrorFactory.System.RootsMustBeIdentical();
            }
        }

        private static void AssertSourceIsNotVolumeRoot([NotNull] AbsolutePath path)
        {
            if (path.IsVolumeRoot)
            {
                throw ErrorFactory.System.AccessDenied(path.GetText());
            }
        }

        [NotNull]
        private DirectoryEntry ResolveSourceDirectory([NotNull] AbsolutePath path)
        {
            var resolver =
                new DirectoryResolver(Root) { ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound() };
            return resolver.ResolveDirectory(path);
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

        private static void MoveDirectory([NotNull] DirectoryEntry sourceDirectory, [NotNull] DirectoryEntry destinationDirectory,
            [NotNull] string newDirectoryName)
        {
            sourceDirectory.Parent?.DeleteDirectory(sourceDirectory.Name);
            destinationDirectory.MoveDirectoryToHere(sourceDirectory, newDirectoryName);
        }
    }
}
