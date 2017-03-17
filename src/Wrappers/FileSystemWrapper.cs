using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    public sealed class FileSystemWrapper : IFileSystem
    {
        [NotNull]
        public static readonly IFileSystem Default = new FileSystemWrapper();

        public IFile File => new FileWrapper();

        public IDirectory Directory => new DirectoryWrapper();

        private FileSystemWrapper()
        {
        }
    }
}
