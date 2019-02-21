﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Interfaces
{
    [PublicAPI]
    public static class FileExtensions
    {
        [NotNull]
        public static IFileStream OpenRead([NotNull] this IFile file, [NotNull] string path)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            return file.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        [NotNull]
        public static IFileStream OpenWrite([NotNull] this IFile file, [NotNull] string path)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            return file.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
        }

        [NotNull]
        public static StreamReader OpenText([NotNull] this IFile file, [NotNull] string path)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            IFileStream stream = file.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return CreateReader(stream);
        }

        [NotNull]
        public static StreamWriter CreateText([NotNull] this IFile file, [NotNull] string path)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            IFileStream stream = file.Open(path, FileMode.Create, FileAccess.Write);
            return CreateWriter(stream);
        }

        [NotNull]
        public static StreamWriter AppendText([NotNull] this IFile file, [NotNull] string path)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            IFileStream stream = file.Open(path, FileMode.Append, FileAccess.Write);
            return CreateWriter(stream);
        }

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

        [NotNull]
        [ItemNotNull]
        public static string[] ReadAllLines([NotNull] this IFile file, [NotNull] string path,
            [CanBeNull] Encoding encoding = null)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(path, nameof(path));

            return ReadLines(file, path, encoding).ToArray();
        }

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
