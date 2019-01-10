using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class FileSetAttributesArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public FileAttributes Attributes { get; }
        public FileAccessKinds AccessKinds { get; }

        public FileSetAttributesArguments([NotNull] AbsolutePath path, FileAttributes attributes, FileAccessKinds accessKinds = FileAccessKinds.Attributes)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            Attributes = attributes;
            AccessKinds = accessKinds;
        }
    }
}
