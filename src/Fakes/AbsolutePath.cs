using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class AbsolutePath
    {
        [NotNull]
        private static readonly char[] FileNameCharsInvalid = Path.GetInvalidFileNameChars();

        [NotNull]
        private static readonly string TwoDirectorySeparators = new string(Path.DirectorySeparatorChar, 2);

        private readonly int offset;

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<string> Components { get; }

        [NotNull]
        public string Name => Components[offset];

        public bool IsAtEnd => offset == Components.Count - 1;

        public bool IsLocalDrive => StartsWithDriveLetter(Components);

        public AbsolutePath([NotNull] string path)
            : this(ToComponents(path), 0)
        {
        }

        [NotNull]
        [ItemNotNull]
        private static IReadOnlyList<string> ToComponents([NotNull] string path)
        {
            path = WithoutTrailingSeparator(path);
            path = WithoutExtendedLengthPrefix(path);

            Guard.NotNullNorWhiteSpace(path, nameof(path));

            List<string> components = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToList();

            if (!StartsWithDriveLetter(components) && !IsNetworkShare(components))
            {
                throw ErrorFactory.PathFormatIsNotSupported();
            }

            for (int index = 1; index < components.Count; index++)
            {
                string component = components[index];

                if (component == ".")
                {
                    components.RemoveAt(index);
                    index--;
                }
                else if (component == "..")
                {
                    if (index > 1)
                    {
                        components.RemoveRange(index - 1, 2);
                        index -= 2;
                    }
                    else
                    {
                        // Silently ignore moving to above root.
                        components.RemoveAt(index);
                        index--;
                    }
                }
                else
                {
                    AssertNetworkShareOrDirectoryOrFileNameIsValid(component);
                }
            }

            return components.AsReadOnly();
        }

        [CanBeNull]
        private static string WithoutTrailingSeparator([CanBeNull] string path)
        {
            return path?.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) == true
                ? path.Substring(0, path.Length - 1)
                : path;
        }

        [CanBeNull]
        private static string WithoutExtendedLengthPrefix([CanBeNull] string path)
        {
            if (path != null)
            {
                AssertIsFileSystemNamespaceValid(path);

                if (path.StartsWith(@"\\?\UNC\", StringComparison.Ordinal))
                {
                    return '\\' + path.Substring(7);
                }

                if (path.StartsWith(@"\\?\", StringComparison.Ordinal))
                {
                    return path.Substring(4);
                }
            }

            return path;
        }

        [AssertionMethod]
        private static void AssertIsFileSystemNamespaceValid([NotNull] string path)
        {
            if (path.StartsWith(@"\\?\GLOBALROOT", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(@"\\.\", StringComparison.Ordinal))
            {
                throw new NotSupportedException("Only Win32 File Namespaces are supported.");
            }
        }

        private static bool StartsWithDriveLetter([NotNull] [ItemNotNull] IReadOnlyList<string> components)
        {
            if (components[0].Length == 2 && components[0][1] == Path.VolumeSeparatorChar)
            {
                char driveLetter = char.ToUpperInvariant(components[0][0]);
                return driveLetter >= 'A' && driveLetter <= 'Z';
            }

            return false;
        }

        private static bool IsNetworkShare([NotNull] [ItemNotNull] List<string> components)
        {
            if (components.Count > 2 && components[0] == string.Empty && components[1] == string.Empty)
            {
                string networkShare = components[2];

                AssertNetworkShareOrDirectoryOrFileNameIsValid(networkShare);

                components.RemoveRange(0, 2);
                components[0] = TwoDirectorySeparators + networkShare;
                return true;
            }

            return false;
        }

        [AssertionMethod]
        private static void AssertNetworkShareOrDirectoryOrFileNameIsValid([CanBeNull] string component)
        {
            if (string.IsNullOrWhiteSpace(component))
            {
                throw ErrorFactory.IllegalCharactersInPath();
            }

            foreach (char ch in component)
            {
                if (FileNameCharsInvalid.Contains(ch))
                {
                    throw ErrorFactory.IllegalCharactersInPath();
                }
            }
        }

        private AbsolutePath([NotNull] [ItemNotNull] IReadOnlyList<string> components, int offset)
        {
            Components = components;
            this.offset = offset;
        }

        [NotNull]
        public AbsolutePath MoveDown()
        {
            if (IsAtEnd)
            {
                throw new InvalidOperationException();
            }

            return new AbsolutePath(Components, offset + 1);
        }

        [NotNull]
        public string GetText()
        {
            return Components.Count == 1 && IsLocalDrive
                ? Components[0] + Path.DirectorySeparatorChar
                : string.Join(Path.DirectorySeparatorChar.ToString(), Components);
        }

        public override string ToString()
        {
            var textBuilder = new StringBuilder();

            for (int index = 0; index < Components.Count; index++)
            {
                if (textBuilder.Length > 0)
                {
                    textBuilder.Append(Path.DirectorySeparatorChar);
                }

                if (index == offset)
                {
                    textBuilder.Append('[');
                }

                textBuilder.Append(Components[index]);

                if (index == offset)
                {
                    textBuilder.Append(']');
                }
            }

            return textBuilder.ToString();
        }
    }
}
