using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryCreateHandler : FakeOperationHandler<DirectoryCreateArguments, DirectoryEntry>
    {
        public DirectoryCreateHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override DirectoryEntry Handle(DirectoryCreateArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            AssertNetworkShareOrDriveExists(arguments.Path);

            var navigator = new PathNavigator(arguments.Path);

            if (arguments.Path.IsVolumeRoot && Root.TryGetExistingDirectory(navigator) == null)
            {
                throw ErrorFactory.DirectoryNotFound(arguments.Path.GetText());
            }

            return Root.CreateDirectories(navigator);
        }

        private void AssertNetworkShareOrDriveExists([NotNull] AbsolutePath absolutePath)
        {
            if (!Root.Directories.ContainsKey(absolutePath.Components[0]))
            {
                if (absolutePath.IsOnLocalDrive)
                {
                    throw ErrorFactory.DirectoryNotFound(absolutePath.GetText());
                }

                if (absolutePath.IsVolumeRoot)
                {
                    throw ErrorFactory.DirectoryNotFound(absolutePath.GetText());
                }

                throw ErrorFactory.NetworkPathNotFound();
            }
        }
    }
}
