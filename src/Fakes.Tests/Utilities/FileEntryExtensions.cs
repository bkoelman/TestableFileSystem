using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Tests.Utilities
{
    internal static class FileEntryExtensions
    {
        public static void WriteToFile([NotNull] this FileEntry entry, [NotNull] byte[] buffer)
        {
            using (IFileStream stream = entry.Open(FileMode.Open, FileAccess.Write))
            {
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        public static void WriteToFile([NotNull] this FileEntry entry, [NotNull] string text)
        {
            using (IFileStream stream = entry.Open(FileMode.Open, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream.AsStream()))
                {
                    writer.Write(text);
                }
            }
        }

        [NotNull]
        public static byte[] GetFileContentsAsByteArray([NotNull] this FileEntry entry)
        {
            using (IFileStream stream = entry.Open(FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[stream.Length];

                int count = stream.Read(buffer, 0, buffer.Length);

                count.Should().Be((int)stream.Length);

                return buffer;
            }
        }

        [NotNull]
        public static string GetFileContentsAsString([NotNull] this FileEntry entry)
        {
            using (IFileStream stream = entry.Open(FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream.AsStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
