using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileGetAttributesHandler : FakeOperationHandler<FileGetAttributesArguments, FileAttributes>
    {
        public FileGetAttributesHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override FileAttributes Handle(FileGetAttributesArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            AssertVolumeRootExists(arguments.Path);

            BaseEntry entry = GetExistingEntry(arguments.Path);
            return entry.Attributes;
        }

        private void AssertVolumeRootExists([NotNull] AbsolutePath absolutePath)
        {
            if (!Root.Directories.ContainsKey(absolutePath.Components[0]))
            {
                if (absolutePath.IsOnLocalDrive)
                {
                    throw ErrorFactory.DirectoryNotFound(absolutePath.GetText());
                }

                throw ErrorFactory.NetworkPathNotFound();
            }
        }

        [NotNull]
        private BaseEntry GetExistingEntry([NotNull] AbsolutePath absolutePath)
        {
            AssertParentIsDirectoryOrMissing(absolutePath);

            var navigator = new PathNavigator(absolutePath);
            BaseEntry entry = Root.TryGetExistingFile(navigator);

            if (entry == null)
            {
                entry = Root.TryGetExistingDirectory(navigator);
                if (entry == null)
                {
                    AbsolutePath parentPath = absolutePath.TryGetParentPath();

                    DirectoryEntry parentDirectory = parentPath != null
                        ? Root.TryGetExistingDirectory(new PathNavigator(parentPath))
                        : null;

                    if (parentDirectory == null)
                    {
                        throw ErrorFactory.DirectoryNotFound(absolutePath.GetText());
                    }

                    throw ErrorFactory.FileNotFound(absolutePath.GetText());
                }
            }
            return entry;
        }

        private void AssertParentIsDirectoryOrMissing([NotNull] AbsolutePath path)
        {
            AbsolutePath parentPath = path.TryGetParentPath();
            if (parentPath == null)
            {
                return;
            }

            var navigator = new PathNavigator(parentPath);
            DirectoryEntry directory = Root.TryGetExistingDirectory(navigator);
            if (directory != null)
            {
                return;
            }

            throw ErrorFactory.DirectoryNotFound(path.GetText());
        }
    }
}
