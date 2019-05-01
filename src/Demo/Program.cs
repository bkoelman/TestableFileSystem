using System;
using System.IO;
using System.Threading;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Wrappers;

namespace TestableFileSystem.Demo
{
    internal static class Program
    {
        private static readonly string TargetDirectory = Environment.ExpandEnvironmentVariables(@"%TEMP%\FileSystemDemo");

        private static void Main()
        {
            IFileSystem fileSystem = CreateFileSystem();

            fileSystem.Directory.CreateDirectory(TargetDirectory);

            using (var dumper = new FileSystemChangeDumper(fileSystem))
            {
                dumper.Start(TargetDirectory, NotifyFilters.FileName | NotifyFilters.LastWrite);

                DeleteAndCreateFile(fileSystem);

                Thread.Sleep(500);

                ShowFilesInTargetDirectory(fileSystem);

                Console.WriteLine("Press any key to close.");
                Console.ReadKey();
            }
        }

        private static void DeleteAndCreateFile(IFileSystem fileSystem)
        {
            string fileName = Path.Combine(TargetDirectory, "newfile.txt");

            fileSystem.File.Delete(fileName);
            fileSystem.File.WriteAllText(fileName, "TestableFileSystem Example Content");
        }

        private static void ShowFilesInTargetDirectory(IFileSystem fileSystem)
        {
            Console.WriteLine();
            Console.WriteLine("Scanning for text files in directory:");
            Console.WriteLine($"  \"{TargetDirectory}\"");

            foreach (string path in fileSystem.Directory.GetFiles(TargetDirectory, "*.txt", SearchOption.AllDirectories))
            {
                Console.WriteLine($"File \"{path}\" contains:");

                string contents = fileSystem.File.ReadAllText(path);
                Console.WriteLine(contents);
            }
        }

        private static IFileSystem CreateFileSystem()
        {
            return CreateFakeFileSystem();
            //return CreateWrapperFileSystem();
        }

        private static IFileSystem CreateFakeFileSystem()
        {
            string filePath = Path.Combine(TargetDirectory, @"subdir\demo.txt");

            return new FakeFileSystemBuilder().IncludingTextFile(filePath, "Hello from Fake File System").Build();
        }

        private static IFileSystem CreateWrapperFileSystem()
        {
            return FileSystemWrapper.Default;
        }
    }
}
