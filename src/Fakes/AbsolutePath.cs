using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    // TODO: Split into Path and Navigator components.
    internal sealed class AbsolutePath
    {
        [NotNull]
        private static readonly char[] FileNameCharsInvalid = Path.GetInvalidFileNameChars();

        [NotNull]
        private static readonly string TwoDirectorySeparators = new string(Path.DirectorySeparatorChar, 2);

        [NotNull]
        [ItemNotNull]
        private static readonly ISet<string> ReservedComponentNames =
            new HashSet<string>(
                new[]
                {
                    "CON",
                    "PRN",
                    "AUX",
                    "NUL",
                    "COM1",
                    "COM2",
                    "COM3",
                    "COM4",
                    "COM5",
                    "COM6",
                    "COM7",
                    "COM8",
                    "COM9",
                    "LPT1",
                    "LPT2",
                    "LPT3",
                    "LPT4",
                    "LPT5",
                    "LPT6",
                    "LPT7",
                    "LPT8",
                    "LPT9"
                }, StringComparer.OrdinalIgnoreCase);

        private readonly int offset;

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<string> Components { get; }

        private readonly bool isExtended;

        [NotNull]
        public string Name => Components[offset];

        public bool IsAtEnd => offset == Components.Count - 1;

        public bool IsOnLocalDrive => StartsWithDriveLetter(Components);

        public bool IsRoot => IsOnLocalDrive ? Components.Count == 1 : Components.Count == 2;

        public AbsolutePath([NotNull] string path)
            : this(ToComponents(path), 0, HasExtendedLengthPrefix(path))
        {
        }

        [NotNull]
        [ItemNotNull]
        private static IReadOnlyList<string> ToComponents([NotNull] string path)
        {
            path = path.TrimEnd();
            path = WithoutTrailingSeparator(path);
            path = WithoutExtendedLengthPrefix(path);

            Guard.NotNullNorWhiteSpace(path, nameof(path));

            List<string> components = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToList();

            AssertIsNotReservedComponentName(components[0]);

            if (StartsWithNetworkShare(components))
            {
                AssertNetworkShareOrDirectoryOrFileNameIsValid(components[2]);

                if (components.Count < 4)
                {
                    throw ErrorFactory.UncPathIsInvalid();
                }

                AssertNetworkShareOrDirectoryOrFileNameIsValid(components[3]);

                components[2] = TwoDirectorySeparators + components[2];
                components.RemoveRange(0, 2);
            }
            else if (!StartsWithDriveLetter(components))
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

        private static bool HasExtendedLengthPrefix([CanBeNull] string path)
        {
            return path != null && path.StartsWith(@"\\?\", StringComparison.Ordinal);
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
        private static void AssertIsNotReservedComponentName([NotNull] string name)
        {
            if (ReservedComponentNames.Contains(name))
            {
                throw new NotSupportedException("Reserved names are not supported.");
            }
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

        private static bool StartsWithNetworkShare([NotNull] [ItemNotNull] List<string> components)
        {
            return components.Count > 2 && components[0] == string.Empty && components[1] == string.Empty;
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

            AssertIsNotReservedComponentName(component);
        }

        private AbsolutePath([NotNull] [ItemNotNull] IReadOnlyList<string> components, int offset, bool isExtended)
        {
            Components = components;
            this.offset = offset;
            this.isExtended = isExtended;
        }

        [NotNull]
        public string GetRootName()
        {
            return IsOnLocalDrive ? GetRootNameForLocalDrive() : GetRootNameForUncPath();
        }

        [CanBeNull]
        public AbsolutePath GetParentPath()
        {
            if (IsOnLocalDrive && Components.Count == 1)
            {
                return null;
            }

            if (!IsOnLocalDrive && Components.Count == 2)
            {
                return null;
            }

            List<string> newComponents = Components.ToList();
            newComponents.RemoveAt(newComponents.Count - 1);

            return new AbsolutePath(newComponents, 0, isExtended);
        }

        [NotNull]
        private string GetRootNameForLocalDrive()
        {
            var nameBuilder = new StringBuilder();

            if (isExtended)
            {
                nameBuilder.Append(@"\\?\");
            }

            nameBuilder.Append(Components[0]);
            nameBuilder.Append(Path.DirectorySeparatorChar);

            return nameBuilder.ToString();
        }

        [NotNull]
        private string GetRootNameForUncPath()
        {
            var nameBuilder = new StringBuilder();

            if (isExtended)
            {
                nameBuilder.Append(@"\\?\UNC\");
                nameBuilder.Append(Components[0].Substring(2));
            }
            else
            {
                nameBuilder.Append(Components[0]);
            }

            nameBuilder.Append(Path.DirectorySeparatorChar);
            nameBuilder.Append(Components[1]);

            return nameBuilder.ToString();
        }

        [NotNull]
        public AbsolutePath MoveDown()
        {
            if (IsAtEnd)
            {
                throw new InvalidOperationException();
            }

            return new AbsolutePath(Components, offset + 1, isExtended);
        }

        [NotNull]
        public string GetText()
        {
            var builder = new StringBuilder();
            builder.Append(GetRootName());

            int componentStartIndex;
            if (IsOnLocalDrive)
            {
                componentStartIndex = 1;
            }
            else
            {
                if (Components.Count > 2)
                {
                    builder.Append(Path.DirectorySeparatorChar);
                }

                componentStartIndex = 2;
            }

            int componentCount = Components.Count - componentStartIndex;

            string relativePath = string.Join(Path.DirectorySeparatorChar.ToString(), Components.ToArray(), componentStartIndex,
                componentCount);

            builder.Append(relativePath);
            return builder.ToString();
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
