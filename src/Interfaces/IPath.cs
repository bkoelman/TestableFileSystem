using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    public interface IPath
    {
        [NotNull]
        string GetTempPath();

        [NotNull]
        string GetTempFileName();
    }
}
