using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Utilities;
using TestableFileSystem.Wrappers;

namespace TestableFileSystem.Fakes.Tests
{
    internal sealed class FileSystemBuilderFactory : IDisposable
    {
        private readonly bool useFakeFileSystem;

        [NotNull]
        private readonly string rootDirectory =
            Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), "FS", Guid.NewGuid().ToString());

        public FileSystemBuilderFactory(bool useFakeFileSystem)
        {
            this.useFakeFileSystem = useFakeFileSystem;
        }

        [NotNull]
        public IFileSystemBuilder Create()
        {
            return useFakeFileSystem
                ? (IFileSystemBuilder)new FakeFileSystemBuilder()
                : new WrapperFileSystemBuilder(FileSystemWrapper.Default);
        }

        [NotNull]
        public string MapPath([NotNull] string path)
        {
            Guard.NotNull(path, nameof(path));

            if (useFakeFileSystem)
            {
                return path;
            }

            bool isExtended = HasPrefixForExtendedLength(path);
            if (isExtended)
            {
                path = WithoutPrefixForExtendedLength(path);
            }

            if (IsPathOnDrive(path))
            {
                return MapPathOnDrive(path, isExtended);
            }

            if (IsPathOnNetworkShare(path))
            {
                return MapPathOnNetworkShare(path, isExtended);
            }

            throw new NotSupportedException($"The path '{path}' cannot be mapped.");
        }

        private bool HasPrefixForExtendedLength([NotNull] string path)
        {
            return path.StartsWith(@"\\?\", StringComparison.Ordinal) ||
                path.StartsWith(@"\\?\UNC\", StringComparison.Ordinal);
        }

        [NotNull]
        private static string WithoutPrefixForExtendedLength([NotNull] string path)
        {
            if (path.StartsWith(@"\\?\UNC\", StringComparison.Ordinal))
            {
                return '\\' + path.Substring(7);
            }

            if (path.StartsWith(@"\\?\", StringComparison.Ordinal))
            {
                return path.Substring(4);
            }

            return path;
        }

        private bool IsPathOnDrive([NotNull] string path)
        {
            if (path.Length >= 2 && path[1] == Path.VolumeSeparatorChar)
            {
                char driveLetter = char.ToUpperInvariant(path[0]);
                return driveLetter >= 'A' && driveLetter <= 'Z';
            }

            return false;
        }

        [NotNull]
        private string MapPathOnDrive([NotNull] string path, bool isExtended)
        {
            char driveLetter = char.ToUpperInvariant(path[0]);

            string mapped = path.Length > 3
                ? Path.Combine(rootDirectory, "Volume" + driveLetter, path.Substring(3))
                : Path.Combine(rootDirectory, "Volume" + driveLetter);

            if (isExtended)
            {
                mapped = @"\\?\" + mapped;
            }

            return mapped;
        }

        private static bool IsPathOnNetworkShare([NotNull] string path)
        {
            return path.StartsWith("\\", StringComparison.Ordinal);
        }

        [NotNull]
        private string MapPathOnNetworkShare([NotNull] string path, bool isExtended)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (!useFakeFileSystem && Directory.Exists(rootDirectory))
            {
                try
                {
                    Directory.Delete(rootDirectory, true);
                }
                catch (UnauthorizedAccessException)
                {
                    RemoveReadOnlyAttributes();

                    Directory.Delete(rootDirectory, true);
                }
            }
        }

        private void RemoveReadOnlyAttributes()
        {
            foreach (string path in Directory.GetFiles(rootDirectory, "*.*", SearchOption.AllDirectories))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }

            foreach (string path in Directory.GetDirectories(rootDirectory, "*.*", SearchOption.AllDirectories))
            {
                File.SetAttributes(path, FileAttributes.Directory);
            }
        }
    }
}
