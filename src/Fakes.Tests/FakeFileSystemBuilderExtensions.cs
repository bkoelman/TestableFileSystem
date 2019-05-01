using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;

namespace TestableFileSystem.Fakes.Tests
{
    internal static class FakeFileSystemBuilderExtensions
    {
        [NotNull]
        public static FakeFileSystemBuilder WithEmptyFilesInTempDirectory([NotNull] this FakeFileSystemBuilder builder,
            [NotNull] string tempDirectory,
            int indexToExclude = -1)
        {
            builder.WithTempDirectory(tempDirectory);

            for (int index = 0; index <= 0xFFFF; index++)
            {
                if (index != indexToExclude)
                {
                    string path = Path.Combine(tempDirectory, "tmp" + index.ToString("X") + ".tmp");
                    builder.IncludingEmptyFile(path);
                }
            }

            return builder;
        }
    }
}
