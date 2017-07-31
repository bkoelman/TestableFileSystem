using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileSetAttributesArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public FileAttributes Attributes { get; }

        public FileSetAttributesArguments([NotNull] AbsolutePath path, FileAttributes attributes)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            Attributes = attributes;
        }
    }
}
