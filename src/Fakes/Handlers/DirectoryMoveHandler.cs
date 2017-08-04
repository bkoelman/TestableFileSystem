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
        public DirectoryMoveHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override object Handle(DirectoryOrFileMoveArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));
            AssertMovingOnSameVolume(arguments);

            DirectoryEntry sourceDirectory = ResolveSourceDirectory(arguments.SourcePath);

            DirectoryEntry destinationDirectory = ResolveDestinationDirectory(arguments.DestinationPath);

            string newDirectoryName = arguments.DestinationPath.Components.Last();
            MoveDirectory(sourceDirectory, destinationDirectory, newDirectoryName);

            return Missing.Value;
        }

        private void AssertMovingOnSameVolume([NotNull] DirectoryOrFileMoveArguments arguments)
        {
            if (!string.Equals(arguments.SourcePath.GetRootName(), arguments.DestinationPath.GetRootName(),
                StringComparison.OrdinalIgnoreCase))
            {
                throw ErrorFactory.System.RootsMustBeIdentical();
            }
        }

        [NotNull]
        private DirectoryEntry ResolveSourceDirectory([NotNull] AbsolutePath path)
        {
            AssertSourceIsNotVolumeRoot(path);

            var resolver = new DirectoryResolver(Root);
            return resolver.ResolveDirectory(path);
        }

        private static void AssertSourceIsNotVolumeRoot([NotNull] AbsolutePath path)
        {
            if (path.IsVolumeRoot)
            {
                throw ErrorFactory.System.AccessDenied(path.GetText());
            }
        }

        [NotNull]
        private DirectoryEntry ResolveDestinationDirectory([NotNull] AbsolutePath path)
        {
            AbsolutePath parentPath = AssertDestinationIsNotVolumeRoot(path);

            var resolver = new DirectoryResolver(Root);
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

        private static void MoveDirectory([NotNull] DirectoryEntry sourceDirectory, [NotNull] DirectoryEntry destinationDirectory,
            [NotNull] string newDirectoryName)
        {
            sourceDirectory.Parent?.DeleteDirectory(sourceDirectory.Name);
            destinationDirectory.MoveDirectoryToHere(sourceDirectory, newDirectoryName);
        }
    }
}
