using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    public interface IPath
    {
        // TODO: Add Path.GetFullPath, which uses the current directory. Also update analyzer.
        // But make exception for Path.GetFullPath(string path, string basePath) from NetCore21, which does not use the current directory.

        [NotNull]
        string GetTempPath();

        [NotNull]
        string GetTempFileName();
    }
}
