using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryMoveHandler : FakeOperationHandler<EntryMoveArguments, Missing>
    {
        [NotNull]
        private readonly CurrentDirectoryManager currentDirectoryManager;

        [NotNull]
        private readonly FakeFileSystemChangeTracker changeTracker;

        public bool IsFileMoveRequired { get; private set; }

        public DirectoryMoveHandler([NotNull] DirectoryEntry root, [NotNull] CurrentDirectoryManager currentDirectoryManager,
            [NotNull] FakeFileSystemChangeTracker changeTracker)
            : base(root)
        {
            Guard.NotNull(currentDirectoryManager, nameof(currentDirectoryManager));
            Guard.NotNull(changeTracker, nameof(changeTracker));

            this.currentDirectoryManager = currentDirectoryManager;
            this.changeTracker = changeTracker;
        }

        public override Missing Handle(EntryMoveArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));
            AssertMovingToDifferentDirectoryOnSameVolume(arguments);
            AssertSourceIsNotVolumeRoot(arguments.SourcePath);

            BaseEntry sourceFileOrDirectory = ResolveSourceFileOrDirectory(arguments.SourcePath);
            IsFileMoveRequired = sourceFileOrDirectory is FileEntry;

            if (sourceFileOrDirectory is DirectoryEntry sourceDirectory)
            {
                AssertNoConflictWithCurrentDirectory(sourceDirectory);
                AssertDirectoryContainsNoOpenFiles(sourceDirectory, arguments.SourcePath);

                DirectoryEntry destinationDirectory = ResolveDestinationDirectory(arguments.DestinationPath);
                AssertDestinationIsNotDescendantOfSource(destinationDirectory, sourceDirectory);

                string destinationDirectoryName = arguments.DestinationPath.Components.Last();
                MoveDirectory(sourceDirectory, destinationDirectory, destinationDirectoryName, arguments.SourcePath.Formatter);
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

            if (!arguments.SourcePath.IsOnSameVolume(arguments.DestinationPath))
            {
                throw ErrorFactory.System.VolumesMustBeIdentical();
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
        private BaseEntry ResolveSourceFileOrDirectory([NotNull] AbsolutePath path)
        {
            var resolver = new DirectoryResolver(Root)
            {
                ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound(),
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound()
            };

            // ReSharper disable once AssignNullToNotNullAttribute
            DirectoryEntry parentDirectory = resolver.ResolveDirectory(path.TryGetParentPath(), path.GetText());

            string fileOrDirectoryName = path.Components.Last();

            if (parentDirectory.ContainsDirectory(fileOrDirectoryName))
            {
                return parentDirectory.GetDirectory(fileOrDirectoryName);
            }

            if (parentDirectory.ContainsFile(fileOrDirectoryName))
            {
                return parentDirectory.GetFile(fileOrDirectoryName);
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

        [AssertionMethod]
        private static void AssertDestinationDoesNotExist([NotNull] string directoryName,
            [NotNull] DirectoryEntry parentDirectory)
        {
            if (parentDirectory.ContainsDirectory(directoryName) || parentDirectory.ContainsFile(directoryName))
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

        private void MoveDirectory([NotNull] DirectoryEntry sourceDirectory, [NotNull] DirectoryEntry destinationDirectory,
            [NotNull] string destinationDirectoryName, [NotNull] IPathFormatter sourcePathFormatter)
        {
            if (sourceDirectory.Parent == destinationDirectory)
            {
                sourceDirectory.Parent.RenameDirectory(sourceDirectory.Name, destinationDirectoryName, sourcePathFormatter);
            }
            else
            {
                changeTracker.NotifyContentsAccessed(destinationDirectory.PathFormatter, FileAccessKinds.Read);

                sourceDirectory.Parent?.DeleteDirectory(sourceDirectory.Name);
                destinationDirectory.MoveDirectoryToHere(sourceDirectory, destinationDirectoryName);
            }
        }
    }
}
