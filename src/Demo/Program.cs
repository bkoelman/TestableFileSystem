using TestableFileSystem.Interfaces;
using TestableFileSystem.Wrappers;

namespace TestableFileSystem.Demo
{
    internal static class Program
    {
        private static void Main()
        {
            IFileSystem fileSystem = FileSystemWrapper.Default;
        }
    }
}
