using System.IO;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    internal sealed class PathWrapper : IPath
    {
        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public string GetTempPath()
        {
            return Path.GetTempPath();
        }

        public string GetTempFileName()
        {
            return Path.GetTempFileName();
        }
    }
}
