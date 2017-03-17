using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    public interface IFileSystem
    {
        [NotNull]
        IFile File { get; }

        [NotNull]
        IDirectory Directory { get; }
    }
}
