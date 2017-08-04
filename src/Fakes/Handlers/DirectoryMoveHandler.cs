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

            DirectoryEntry sourceDirectory = ResolveSourceDirectory(arguments.SourcePath);

            DirectoryEntry destinationDirectory = ResolveDestinationDirectory(arguments.DestinationPath);

            string newDirectoryName = arguments.DestinationPath.Components.Last();
            MoveDirectory(sourceDirectory, destinationDirectory, newDirectoryName);

            return Missing.Value;
        }

        [NotNull]
        private DirectoryEntry ResolveSourceDirectory([NotNull] AbsolutePath path)
        {
            var resolver = new DirectoryResolver(Root);
            return resolver.ResolveDirectory(path);
        }

        [NotNull]
        private DirectoryEntry ResolveDestinationDirectory([NotNull] AbsolutePath path)
        {
            AbsolutePath parentPath = path.TryGetParentPath();
            if (parentPath == null)
            {
                throw new Exception("TODO: Parent of destination not found. Trying to move to volume root?");
            }

            var resolver = new DirectoryResolver(Root);
            DirectoryEntry parentDirectory = resolver.ResolveDirectory(parentPath);

            if (parentDirectory.Directories.ContainsKey(path.Components.Last()))
            {
                throw ErrorFactory.System.CannotCreateFileBecauseFileAlreadyExists();
            }

            if (parentDirectory.Files.ContainsKey(path.Components.Last()))
            {
                throw new Exception("TODO: Directory already exists as file.");
            }

            return parentDirectory;
        }

        private static void MoveDirectory([NotNull] DirectoryEntry sourceDirectory, [NotNull] DirectoryEntry destinationDirectory,
            [NotNull] string newDirectoryName)
        {
            if (sourceDirectory.Parent == null)
            {
                throw new Exception("TODO: Parent of source not found. Trying to move from volume root?");
            }

            sourceDirectory.Parent.DeleteDirectory(sourceDirectory.Name);
            destinationDirectory.MoveDirectoryToHere(sourceDirectory, newDirectoryName);
        }
    }
}
