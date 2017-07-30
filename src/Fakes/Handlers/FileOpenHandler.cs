using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileOpenHandler : FakeOperationHandler<FileOpenArguments, IFileStream>
    {
        public FileOpenHandler([NotNull] FakeFileSystem fileSystem, [NotNull] DirectoryEntry root)
            : base(fileSystem, root)
        {
        }

        public override IFileStream Handle(FileOpenArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            FileAccess fileAccess = DetectFileAccess(arguments);

            AbsolutePath absolutePath = FileSystem.ToAbsolutePath(arguments.Path);

            var resolver = new FileResolver(Root);
            (DirectoryEntry containingDirectory, FileEntry existingFileOrNull, string fileName) =
                resolver.TryResolveFile(absolutePath);

            if (existingFileOrNull != null)
            {
                if (arguments.Mode == FileMode.CreateNew)
                {
                    throw ErrorFactory.CannotCreateBecauseFileAlreadyExists(absolutePath.GetText());
                }

                return existingFileOrNull.Open(arguments.Mode, fileAccess);
            }

            if (arguments.Mode == FileMode.Open || arguments.Mode == FileMode.Truncate)
            {
                throw ErrorFactory.FileNotFound(absolutePath.GetText());
            }

            FileEntry newFile = containingDirectory.GetOrCreateFile(fileName);
            return newFile.Open(arguments.Mode, fileAccess);
        }

        private static FileAccess DetectFileAccess([NotNull] FileOpenArguments arguments)
        {
            return arguments.Access ?? (arguments.Mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite);
        }
    }
}
