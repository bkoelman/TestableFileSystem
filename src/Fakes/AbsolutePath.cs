using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class AbsolutePath
    {
        private readonly bool isExtended;

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<string> Components { get; }

        public bool IsOnLocalDrive => IsDriveLetter(Components.First());

        public bool IsVolumeRoot => Components.Count == 1;

        public AbsolutePath([NotNull] string path)
        {
            Guard.NotNull(path, nameof(path));

            var parser = new Parser(path);

            Components = parser.GetComponents();
            isExtended = parser.IsExtended;
        }

        private AbsolutePath([NotNull] [ItemNotNull] IReadOnlyList<string> components, bool isExtended)
        {
            Components = components;
            this.isExtended = isExtended;
        }

        [CanBeNull]
        public AbsolutePath GetParentPath()
        {
            if (Components.Count == 1)
            {
                return null;
            }

            List<string> newComponents = Components.ToList();
            newComponents.RemoveAt(newComponents.Count - 1);

            return new AbsolutePath(newComponents, isExtended);
        }

        [NotNull]
        public string GetRootName()
        {
            var textBuilder = new StringBuilder();

            if (IsOnLocalDrive)
            {
                WriteDriveLetter(textBuilder);
            }
            else
            {
                WriteFileShare(textBuilder);
            }

            return textBuilder.ToString();
        }

        private void WriteDriveLetter([NotNull] StringBuilder textBuilder)
        {
            if (isExtended)
            {
                textBuilder.Append(@"\\?\");
            }

            textBuilder.Append(Components[0]);
            textBuilder.Append(Path.DirectorySeparatorChar);
        }

        private void WriteFileShare([NotNull] StringBuilder textBuilder)
        {
            if (isExtended)
            {
                textBuilder.Append(@"\\?\UNC\");
                textBuilder.Append(Components[0].Substring(2));
            }
            else
            {
                textBuilder.Append(Components[0]);
            }
        }

        [NotNull]
        public string GetText()
        {
            var builder = new StringBuilder();
            builder.Append(GetRootName());

            if (!IsOnLocalDrive && Components.Count > 1)
            {
                builder.Append(Path.DirectorySeparatorChar);
            }

            string[] components = Components.Skip(1).ToArray();
            builder.Append(string.Join(Path.DirectorySeparatorChar.ToString(), components));

            return builder.ToString();
        }

        public override string ToString()
        {
            return GetText();
        }

        private sealed class Parser
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

            [NotNull]
            private readonly string path;

            public bool IsExtended { get; }

            public Parser([NotNull] string path)
            {
                this.path = NormalizePath(path);
                IsExtended = HasPrefixForExtendedLength(path);
            }

            [NotNull]
            private static string NormalizePath([NotNull] string path)
            {
                Guard.NotNullNorWhiteSpace(path, nameof(path));

                string trimmed = path.TrimEnd();
                string withoutSeparator = WithoutTrailingSeparator(trimmed);
                string withoutPrefix = WithoutPrefixForExtendedLength(withoutSeparator);

                Guard.NotNullNorWhiteSpace(withoutPrefix, nameof(path));

                return withoutPrefix;
            }

            [NotNull]
            private static string WithoutTrailingSeparator([NotNull] string path)
            {
                return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
                    path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                        ? path.Substring(0, path.Length - 1)
                        : path;
            }

            [NotNull]
            private static string WithoutPrefixForExtendedLength([NotNull] string path)
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

            private static bool HasPrefixForExtendedLength([CanBeNull] string path)
            {
                return path?.StartsWith(@"\\?\", StringComparison.Ordinal) == true ||
                    path?.StartsWith(@"\\?\UNC\", StringComparison.Ordinal) == true;
            }

            [NotNull]
            [ItemNotNull]
            public IReadOnlyList<string> GetComponents()
            {
                AssertIsNotReservedComponentName(path);

                List<string> components = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToList();

                bool isOnNetworkShare = AdjustComponentsForNetworkShare(components, path);
                bool isOnDisk = IsDriveLetter(components.First());

                if (!isOnNetworkShare && !isOnDisk)
                {
                    throw ErrorFactory.PathFormatIsNotSupported();
                }

                AdjustComponentsForSelfOrParentIndicators(components);

                return components.AsReadOnly();
            }

            private static bool AdjustComponentsForNetworkShare([NotNull] [ItemNotNull] List<string> components,
                [NotNull] string path)
            {
                if (path.StartsWith(@"\\", StringComparison.Ordinal))
                {
                    if (components.Count < 4)
                    {
                        throw ErrorFactory.UncPathIsInvalid();
                    }

                    AssertDirectoryNameOrFileNameIsValid(components[2]);
                    AssertDirectoryNameOrFileNameIsValid(components[3]);

                    components[3] = TwoDirectorySeparators + components[2] + Path.DirectorySeparatorChar + components[3];
                    components.RemoveRange(0, 3);

                    return true;
                }

                return false;
            }

            [AssertionMethod]
            private static void AssertDirectoryNameOrFileNameIsValid([NotNull] string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw ErrorFactory.IllegalCharactersInPath();
                }

                foreach (char ch in name)
                {
                    if (FileNameCharsInvalid.Contains(ch))
                    {
                        throw ErrorFactory.IllegalCharactersInPath();
                    }
                }

                AssertIsNotReservedComponentName(name);
            }

            [AssertionMethod]
            private static void AssertIsNotReservedComponentName([NotNull] string name)
            {
                if (ReservedComponentNames.Contains(name))
                {
                    throw new NotSupportedException("Reserved names are not supported.");
                }
            }

            private void AdjustComponentsForSelfOrParentIndicators([NotNull] [ItemNotNull] List<string> components)
            {
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
                        AssertDirectoryNameOrFileNameIsValid(component);
                    }
                }
            }
        }

        private static bool IsDriveLetter([NotNull] string name)
        {
            if (name.Length == 2 && name[1] == Path.VolumeSeparatorChar)
            {
                char driveLetter = char.ToUpperInvariant(name[0]);
                return driveLetter >= 'A' && driveLetter <= 'Z';
            }

            return false;
        }
    }
}
