using System.IO;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Wrappers;

namespace TestableFileSystem.Demo
{
    internal class Program
    {
        private static void Main()
        {
            IFileSystem fileSystem = FileSystemWrapper.Default;

            DirectoryInfo info = new DirectoryInfo(@"e:\FileSystemTests\subdir\other\deeper");
            info.Attributes = FileAttributes.ReadOnly;

            fileSystem.Directory.Delete(@"e:\FileSystemTests\subdir", true);
        }
    }
}
