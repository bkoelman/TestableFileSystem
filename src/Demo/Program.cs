using System;
using System.IO;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Wrappers;

namespace TestableFileSystem.Demo
{
    internal static class Program
    {
        private static readonly string TargetDirectory = Environment.ExpandEnvironmentVariables(@"%TEMP%\TestableFileSystemDemo");

        private static void Main()
        {
            IFileSystem fileSystem = CreateFileSystem();

            fileSystem.Directory.CreateDirectory(TargetDirectory);
            new FileSystemChangeDumper(fileSystem).Start(TargetDirectory);

            DeleteAndCreateFile(fileSystem);
            ShowFilesInTargetDirectory(fileSystem);

            Console.WriteLine("Press any key to close.");
            Console.ReadKey();
        }

        private static void DeleteAndCreateFile(IFileSystem fileSystem)
        {
            string fileName = Path.Combine(TargetDirectory, "newfile.txt");

            fileSystem.File.Delete(fileName);
            fileSystem.File.WriteAllText(fileName, "TestableFileSystem Example File");
        }

        private static void ShowFilesInTargetDirectory(IFileSystem fileSystem)
        {
            Console.WriteLine();
            Console.WriteLine($"Scanning for text files in '{TargetDirectory}'...");
            foreach (string path in fileSystem.Directory.GetFiles(TargetDirectory, "*.txt", SearchOption.AllDirectories))
            {
                Console.WriteLine($"File '{path}' contains:");

                string contents = fileSystem.File.ReadAllText(path);
                Console.WriteLine(contents);
            }
        }

        private static IFileSystem CreateFileSystem()
        {
            //return CreateFakeFileSystem();
            return CreateWrapperFileSystem();
        }

        private static IFileSystem CreateFakeFileSystem()
        {
            return new FakeFileSystemBuilder()
                .IncludingTextFile(Path.Combine(TargetDirectory, @"subdir\demo.txt"), "Hello from Fake File System").Build();
        }

        private static IFileSystem CreateWrapperFileSystem()
        {
            return FileSystemWrapper.Default;
        }
    }
}
