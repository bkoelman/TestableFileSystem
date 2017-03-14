using System.IO;
using FluentAssertions;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Tests.Utilities
{
    internal static class FileEntryExtensions
    {
        public static void WriteToFile([NotNull] this FileEntry entry, [NotNull] byte[] buffer)
        {
            using (var stream = entry.Open(FileMode.Open, FileAccess.Write))
            {
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        public static void WriteToFile([NotNull] this FileEntry entry, [NotNull] string text)
        {
            using (var stream = entry.Open(FileMode.Open, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream.InnerStream))
                {
                    writer.Write(text);
                }
            }
        }

        [NotNull]
        public static byte[] GetFileContentsAsByteArray([NotNull] this FileEntry entry)
        {
            using (var stream = entry.Open(FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[stream.Length];

                int count = stream.Read(buffer, 0, buffer.Length);

                count.Should().Be((int) stream.Length);

                return buffer;
            }
        }

        [NotNull]
        public static string GetFileContentsAsString([NotNull] this FileEntry entry)
        {
            using (var stream = entry.Open(FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream.InnerStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
