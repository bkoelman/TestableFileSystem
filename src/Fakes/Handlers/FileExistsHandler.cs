using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileExistsHandler : FakeOperationHandler<FileExistsArguments, bool>
    {
        public FileExistsHandler([NotNull] FakeFileSystem fileSystem, [NotNull] DirectoryEntry root)
            : base(fileSystem, root)
        {
        }

        public override bool Handle(FileExistsArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            if (string.IsNullOrWhiteSpace(arguments.Path))
            {
                return false;
            }

            try
            {
                AbsolutePath absolutePath = FileSystem.ToAbsolutePath(arguments.Path);

                var resolver = new FileResolver(Root);
                (DirectoryEntry _, FileEntry existingFileOrNull, string _) = resolver.TryResolveFile(absolutePath);

                return existingFileOrNull != null;
            }
            catch (Exception ex) when (ShouldSuppress(ex))
            {
                return false;
            }
        }

        private static bool ShouldSuppress([NotNull] Exception ex)
        {
            return ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException ||
                ex is NotSupportedException;
        }
    }
}
