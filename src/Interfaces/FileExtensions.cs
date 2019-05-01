using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Provides extension methods on <see cref="IFile" /> objects to open files and access their contents.
    /// </summary>
    [PublicAPI]
    public static class FileExtensions
    {
        /// <inheritdoc cref="File.OpenRead" />
        [NotNull]
        public static IFileStream OpenRead([NotNull] this IFile file, [NotNull] string path)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            return file.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <inheritdoc cref="File.OpenWrite" />
        [NotNull]
        public static IFileStream OpenWrite([NotNull] this IFile file, [NotNull] string path)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            return file.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
        }

        /// <inheritdoc cref="File.OpenText" />
        [NotNull]
        public static StreamReader OpenText([NotNull] this IFile file, [NotNull] string path)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            IFileStream stream = file.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return CreateReader(stream);
        }

        /// <inheritdoc cref="File.CreateText" />
        [NotNull]
        public static StreamWriter CreateText([NotNull] this IFile file, [NotNull] string path)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            IFileStream stream = file.Open(path, FileMode.Create, FileAccess.Write);
            return CreateWriter(stream);
        }

        /// <inheritdoc cref="File.AppendText" />
        [NotNull]
        public static StreamWriter AppendText([NotNull] this IFile file, [NotNull] string path)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            IFileStream stream = file.Open(path, FileMode.Append, FileAccess.Write);
            return CreateWriter(stream);
        }

        /// <inheritdoc cref="File.ReadAllBytes" />
        [NotNull]
        public static byte[] ReadAllBytes([NotNull] this IFile file, [NotNull] string path)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            using (IFileStream stream = OpenRead(file, path))
            {
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <inheritdoc cref="File.ReadAllLines(string,Encoding)" />
        [NotNull]
        [ItemNotNull]
        public static string[] ReadAllLines([NotNull] this IFile file, [NotNull] string path,
            [CanBeNull] Encoding encoding = null)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            return ReadLines(file, path, encoding).ToArray();
        }

        /// <inheritdoc cref="File.ReadLines(string,Encoding)" />
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<string> ReadLines([NotNull] this IFile file, [NotNull] string path,
            [CanBeNull] Encoding encoding = null)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            using (IFileStream stream = file.OpenRead(path))
            {
                using (StreamReader reader = CreateReader(stream, encoding))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        yield return line;
                    }
                }
            }
        }

        /// <inheritdoc cref="File.ReadAllText(string,Encoding)" />
        [NotNull]
        public static string ReadAllText([NotNull] this IFile file, [NotNull] string path, [CanBeNull] Encoding encoding = null)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            using (IFileStream stream = file.OpenRead(path))
            {
                using (StreamReader reader = CreateReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <inheritdoc cref="File.WriteAllBytes" />
        public static void WriteAllBytes([NotNull] this IFile file, [NotNull] string path, [NotNull] byte[] bytes)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(bytes, nameof(bytes));

            using (IFileStream stream = file.Create(path))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <inheritdoc cref="File.WriteAllLines(string,IEnumerable{string},Encoding)" />
        public static void WriteAllLines([NotNull] this IFile file, [NotNull] string path,
            [NotNull] [ItemNotNull] IEnumerable<string> contents, [CanBeNull] Encoding encoding = null)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(contents, nameof(contents));

            using (IFileStream stream = file.Create(path))
            {
                using (StreamWriter writer = CreateWriter(stream, encoding))
                {
                    foreach (string line in contents)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }

        /// <inheritdoc cref="File.WriteAllText(string,string,Encoding)" />
        public static void WriteAllText([NotNull] this IFile file, [NotNull] string path, [NotNull] string contents,
            [CanBeNull] Encoding encoding = null)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(contents, nameof(contents));

            using (IFileStream stream = file.Create(path))
            {
                using (StreamWriter writer = CreateWriter(stream, encoding))
                {
                    writer.Write(contents);
                }
            }
        }

        /// <inheritdoc cref="File.AppendAllLines(string,IEnumerable{string},Encoding)" />
        public static void AppendAllLines([NotNull] this IFile file, [NotNull] string path,
            [NotNull] [ItemNotNull] IEnumerable<string> contents, [CanBeNull] Encoding encoding = null)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            using (IFileStream stream = file.Open(path, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter writer = CreateWriter(stream, encoding))
                {
                    foreach (string line in contents)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }

        /// <inheritdoc cref="File.AppendAllText(string,string,Encoding)" />
        public static void AppendAllText([NotNull] this IFile file, [NotNull] string path, [CanBeNull] string contents,
            [CanBeNull] Encoding encoding = null)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            using (IFileStream stream = file.Open(path, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter writer = CreateWriter(stream, encoding))
                {
                    writer.Write(contents);
                }
            }
        }

        [NotNull]
        private static StreamReader CreateReader([NotNull] IFileStream stream, [CanBeNull] Encoding encoding = null)
        {
            return encoding == null ? new StreamReader(stream.AsStream()) : new StreamReader(stream.AsStream(), encoding);
        }

        [NotNull]
        private static StreamWriter CreateWriter([NotNull] IFileStream stream, [CanBeNull] Encoding encoding = null)
        {
            return encoding == null ? new StreamWriter(stream.AsStream()) : new StreamWriter(stream.AsStream(), encoding);
        }
    }
}
