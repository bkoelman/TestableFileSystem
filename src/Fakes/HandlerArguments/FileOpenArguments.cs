using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class FileOpenArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public FileMode Mode { get; }

        [CanBeNull]
        public FileAccess? Access { get; }

        [CanBeNull]
        public FileOptions? CreateOptions { get; }

        public FileOpenArguments([NotNull] AbsolutePath path, FileMode mode, [CanBeNull] FileAccess? access,
            [CanBeNull] FileOptions? createOptions)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            Mode = mode;
            Access = access;
            CreateOptions = createOptions;
        }
    }
}
