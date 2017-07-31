using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class DirectoryCreateArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public bool CanCreateVolumeRoot { get; }

        public DirectoryCreateArguments([NotNull] AbsolutePath path, bool canCreateVolumeRoot)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            CanCreateVolumeRoot = canCreateVolumeRoot;
        }
    }
}
