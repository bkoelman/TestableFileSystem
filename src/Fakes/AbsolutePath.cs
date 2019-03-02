﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class AbsolutePath
    {
        // https://blogs.msdn.microsoft.com/jeremykuhne/2016/04/21/path-format-overview/
        // https://blogs.msdn.microsoft.com/jeremykuhne/2016/04/21/path-normalization/

        [NotNull]
        private static readonly string TwoDirectorySeparators = new string(Path.DirectorySeparatorChar, 2);

        private readonly bool isExtended;

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<string> Components { get; }

        public bool IsOnLocalDrive => IsDriveLetter(Components.First());

        public bool IsVolumeRoot => Components.Count == 1;

        [NotNull]
        public string TrailingWhiteSpace { get; }

        [NotNull]
        public IPathFormatter Formatter { get; }

        public AbsolutePath([NotNull] string path)
        {
            Guard.NotNull(path, nameof(path));

            var parser = new Parser(path);

            Components = parser.GetComponents();
            isExtended = parser.IsExtended;
            TrailingWhiteSpace = parser.TrailingWhiteSpace;

            Formatter = new AbsolutePathFormatter(this);
        }

        private AbsolutePath([NotNull] [ItemNotNull] IReadOnlyList<string> components, bool isExtended)
        {
            Components = components;
            this.isExtended = isExtended;
            TrailingWhiteSpace = string.Empty;

            Formatter = new AbsolutePathFormatter(this);
        }

        [CanBeNull]
        public AbsolutePath TryGetParentPath()
        {
            return IsVolumeRoot ? null : GetAncestorPath(Components.Count - 2);
        }

        [NotNull]
        public AbsolutePath GetAncestorPath(int depth)
        {
            Guard.InRangeInclusive(depth, nameof(depth), 0, Components.Count - 1);

            if (depth == Components.Count - 1)
            {
                return this;
            }

            List<string> newComponents = Components.Take(depth + 1).ToList();
            return new AbsolutePath(newComponents, isExtended);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<AbsolutePathComponent> EnumerateComponents()
        {
            for (int offset = 0; offset < Components.Count; offset++)
            {
                yield return new AbsolutePathComponent(this, offset);
            }
        }

        [NotNull]
        public string GetText()
        {
            var builder = new StringBuilder();
            WriteVolumeRoot(builder);

            if (!IsOnLocalDrive && Components.Count > 1)
            {
                builder.Append(Path.DirectorySeparatorChar);
            }

            string[] components = Components.Skip(1).ToArray();
            builder.Append(string.Join(Path.DirectorySeparatorChar.ToString(), components));

            return builder.ToString();
        }

        private void WriteVolumeRoot([NotNull] StringBuilder builder)
        {
            if (IsOnLocalDrive)
            {
                WriteDriveLetter(builder);
            }
            else
            {
                WriteFileShare(builder);
            }
        }

        private void WriteDriveLetter([NotNull] StringBuilder builder)
        {
            if (isExtended)
            {
                builder.Append(@"\\?\");
            }

            builder.Append(Components[0]);
            builder.Append(Path.DirectorySeparatorChar);
        }

        private void WriteFileShare([NotNull] StringBuilder builder)
        {
            if (isExtended)
            {
                builder.Append(@"\\?\UNC\");
                builder.Append(Components[0].Substring(2));
            }
            else
            {
                builder.Append(Components[0]);
            }
        }

        public override string ToString()
        {
            return GetText();
        }

        internal static bool IsDriveLetter([NotNull] string name)
        {
            if (name.Length == 2 && name[1] == Path.VolumeSeparatorChar)
            {
                char driveLetter = char.ToUpperInvariant(name[0]);
                return driveLetter >= 'A' && driveLetter <= 'Z';
            }

            return false;
        }

        [NotNull]
        public AbsolutePath Append([NotNull] string name)
        {
            Guard.NotNullNorWhiteSpace(name, nameof(name));

            if (name == ".")
            {
                return this;
            }

            if (name == "..")
            {
                // Silently ignore moving to above root.
                return TryGetParentPath() ?? this;
            }

            Parser.AssertDirectoryNameOrFileNameIsValid(name);

            List<string> newComponents = Components.ToList();
            newComponents.Add(name);

            return new AbsolutePath(newComponents, isExtended);
        }

        [NotNull]
        public string MakeRelativeTo([NotNull] AbsolutePath basePath)
        {
            Guard.NotNull(basePath, nameof(basePath));
            AssertBasePathIsNotLongerThanSelf(basePath);

            var relativeComponents = new List<string>();

            for (int index = 0; index < basePath.Components.Count; index++)
            {
                string baseComponent = basePath.Components[index];
                string thisComponent = Components[index];

                AssertIsSameComponent(thisComponent, baseComponent, basePath);
            }

            for (int index = basePath.Components.Count; index < Components.Count; index++)
            {
                string thisComponent = Components[index];
                relativeComponents.Add(thisComponent);
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(), relativeComponents);
        }

        private void AssertBasePathIsNotLongerThanSelf([NotNull] AbsolutePath basePath)
        {
            if (basePath.Components.Count > Components.Count)
            {
                throw ErrorFactory.Internal.UnknownError($"Cannot rebase path '{GetText()}' to '{basePath.GetText()}'.");
            }
        }

        [AssertionMethod]
        private void AssertIsSameComponent([NotNull] string thisComponent, [NotNull] string baseComponent,
            [NotNull] AbsolutePath basePath)
        {
            if (!string.Equals(thisComponent, baseComponent, StringComparison.OrdinalIgnoreCase))
            {
                throw ErrorFactory.Internal.UnknownError($"Cannot rebase path '{GetText()}' to '{basePath.GetText()}'.");
            }
        }

        public bool IsDescendantOf([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            if (path.Components.Count > Components.Count)
            {
                return false;
            }

            for (int index = 0; index < path.Components.Count; index++)
            {
                string baseComponent = path.Components[index];
                string thisComponent = Components[index];

                if (!string.Equals(thisComponent, baseComponent, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return path.Components.Count < Components.Count;
        }

        public static bool AreEquivalent([CanBeNull] AbsolutePath path1, [CanBeNull] AbsolutePath path2)
        {
            if (ReferenceEquals(path1, path2))
            {
                return true;
            }

            if (ReferenceEquals(path1, null))
            {
                return false;
            }

            return path1.IsEquivalentTo(path2);
        }

        private bool IsEquivalentTo([CanBeNull] AbsolutePath path)
        {
            if (path == null || path.Components.Count != Components.Count)
            {
                return false;
            }

            for (int index = 0; index < path.Components.Count; index++)
            {
                string baseComponent = path.Components[index];
                string thisComponent = Components[index];

                if (!string.Equals(thisComponent, baseComponent, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsOnSameVolume([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            return string.Equals(Components[0], path.Components[0], StringComparison.OrdinalIgnoreCase);
        }

        private sealed class Parser
        {
            [NotNull]
            private static readonly char[] FileNameCharsInvalid = Path.GetInvalidFileNameChars();

            [NotNull]
            [ItemNotNull]
            private static readonly ISet<string> ReservedComponentNames = new HashSet<string>(new[]
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

            [NotNull]
            public string TrailingWhiteSpace { get; }

            public Parser([NotNull] string path)
            {
                this.path = NormalizePath(path);
                IsExtended = HasPrefixForExtendedLength(path);
                TrailingWhiteSpace = path.Substring(path.TrimEnd().Length);
            }

            [NotNull]
            private static string NormalizePath([NotNull] string path)
            {
                Guard.NotNullNorWhiteSpace(path, nameof(path));

                // TODO: If the path doesn't end in a separator, all trailing periods and spaces (character code 32 only) will be removed.
                // If the last segment is simply a single or double period it falls under the relative components rule.
                // This rule leads to the possibly surprising ability to create a directory with a trailing space. You simply need to add a trailing separator to do so.

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

            private static bool HasPrefixForExtendedLength([NotNull] string path)
            {
                return path.StartsWith(@"\\?\", StringComparison.Ordinal) ||
                    path.StartsWith(@"\\?\UNC\", StringComparison.Ordinal);
            }

            [NotNull]
            [ItemNotNull]
            public IReadOnlyList<string> GetComponents()
            {
                AssertIsNotReservedComponentName(path);

                List<string> components = path.Split(PathFacts.DirectorySeparatorChars).ToList();

                bool isOnNetworkShare = AdjustComponentsForNetworkShare(components);
                bool isOnDisk = IsDriveLetter(components.First());

                if (!isOnNetworkShare && !isOnDisk)
                {
                    throw ErrorFactory.System.PathFormatIsNotSupported();
                }

                AdjustComponentsForSelfOrParentIndicators(components);

                return components.AsReadOnly();
            }

            private bool AdjustComponentsForNetworkShare([NotNull] [ItemNotNull] List<string> components)
            {
                if (path.StartsWith(@"\\", StringComparison.Ordinal))
                {
                    if (components.Count < 4)
                    {
                        throw ErrorFactory.System.UncPathIsInvalid();
                    }

                    // TODO: Add support for IPv4 and IPv6 network addresses for hostname.

                    AssertDirectoryNameOrFileNameIsValid(components[2]);
                    AssertDirectoryNameOrFileNameIsValid(components[3]);

                    components[3] = TwoDirectorySeparators + components[2] + Path.DirectorySeparatorChar + components[3];
                    components.RemoveRange(0, 3);

                    return true;
                }

                return false;
            }

            [AssertionMethod]
            public static void AssertDirectoryNameOrFileNameIsValid([NotNull] string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw ErrorFactory.System.IllegalCharactersInPath(nameof(path));
                }

                foreach (char ch in path)
                {
                    if (FileNameCharsInvalid.Contains(ch))
                    {
                        throw ErrorFactory.System.IllegalCharactersInPath(nameof(path));
                    }
                }

                AssertIsNotReservedComponentName(path);
            }

            [AssertionMethod]
            private static void AssertIsNotReservedComponentName([NotNull] string name)
            {
                if (ReservedComponentNames.Contains(name))
                {
                    throw new PlatformNotSupportedException("Reserved names are not supported.");
                }
            }

            private void AdjustComponentsForSelfOrParentIndicators([NotNull] [ItemNotNull] List<string> components)
            {
                // TODO: Runs of slashes are collapsed into a single slash, after the first two slashes if present.
                for (int index = 1; index < components.Count; index++)
                {
                    string component = components[index];

                    // TODO: If a segment ends in a single period, that period will be removed. A segment of a single or double period falls under the relative component rule above.
                    // A segment of three periods (or more) doesn't hit any of these rules and is actually a valid file/directory name.

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

        [DebuggerDisplay("{GetPath().GetText()}")]
        private sealed class AbsolutePathFormatter : IPathFormatter
        {
            [NotNull]
            private readonly AbsolutePath path;

            public AbsolutePathFormatter([NotNull] AbsolutePath path)
            {
                Guard.NotNull(path, nameof(path));
                this.path = path;
            }

            public AbsolutePath GetPath()
            {
                return path;
            }
        }
    }
}
