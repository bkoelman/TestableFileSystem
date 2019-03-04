using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    public interface IPath
    {
        [NotNull]
        string GetFullPath([NotNull] string path);

        [NotNull]
        string GetTempPath();

        [NotNull]
        string GetTempFileName();
    }
}
