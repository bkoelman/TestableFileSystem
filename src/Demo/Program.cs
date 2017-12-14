using System;
using System.IO;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Wrappers;

namespace TestableFileSystem.Demo
{
    internal static class Program
    {
        private const string TargetPath = @"C:\";

        private static void Main()
        {
            IFileSystem fileSystem = CreateFakeFileSystem();
            //IFileSystem fileSystem = CreateWrapperFileSystem();

            Console.WriteLine($"Files found in '{TargetPath}':");
            foreach (string path in fileSystem.Directory.GetFiles(TargetPath, "*.txt", SearchOption.AllDirectories))
            {
                Console.WriteLine($"File '{path}' contains:");

                string contents = fileSystem.File.ReadAllText(path);
                Console.WriteLine(contents);
            }

            Console.WriteLine("Press any key to close.");
            Console.ReadKey();
        }

        private static IFileSystem CreateFakeFileSystem()
        {
            return new FakeFileSystemBuilder().IncludingTextFile(@"c:\subdir\demo.txt", "Hello File System").Build();
        }

        private static IFileSystem CreateWrapperFileSystem()
        {
            return FileSystemWrapper.Default;
        }
    }
}
