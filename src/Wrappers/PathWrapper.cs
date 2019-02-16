using System.IO;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    internal sealed class PathWrapper : IPath
    {
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
