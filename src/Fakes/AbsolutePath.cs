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

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<string> Components { get; }

        public int Offset { get; }

        [NotNull]
        public string Name => Components[Offset];

        public bool IsAtStart => Offset == 0;

        public bool IsAtEnd => Offset == Components.Count - 1;

        public AbsolutePath([NotNull] string path)
            : this(ToComponents(path), 0)
        {
        }

        [NotNull]
        [ItemNotNull]
        private static IReadOnlyList<string> ToComponents([NotNull] string path)
        {
            path = WithoutTrailingSeparator(path);
            Guard.NotNullNorWhiteSpace(path, nameof(path));

            var components = path.Split(Path.DirectorySeparatorChar).ToList();

            if (!IsDrive(components) && !IsNetworkShare(components, path))
            {
                throw new ArgumentException("Path must start with drive letter or network share.", nameof(path));
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
                        throw CreateExceptionForInvalid(path);
                    }
                }
                else
                {
                    AssertNetworkShareOrDirectoryOrFileNameIsValid(component, path);
                }
            }

            return components.AsReadOnly();
        }

        [CanBeNull]
        private static string WithoutTrailingSeparator([CanBeNull] string path)
        {
            return path != null && path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? path.Substring(0, path.Length - 1)
                : path;
        }

        private static bool IsDrive([NotNull] [ItemNotNull] List<string> components)
        {
            if (components[0].Length == 2 && components[0][1] == Path.VolumeSeparatorChar)
            {
                char driveLetter = char.ToUpperInvariant(components[0][0]);
                return driveLetter >= 'A' && driveLetter <= 'Z';
            }

            return false;
        }

        private static bool IsNetworkShare([NotNull][ItemNotNull] List<string> components, [NotNull] string path)
        {
            if (components.Count > 2 && components[0] == string.Empty && components[1] == string.Empty)
            {
                string networkShare = components[2];

                AssertNetworkShareOrDirectoryOrFileNameIsValid(networkShare, path);

                components.RemoveRange(0, 2);
                components[0] = TwoDirectorySeparators + networkShare;
                return true;
            }

            return false;
        }

        [AssertionMethod]
        private static void AssertNetworkShareOrDirectoryOrFileNameIsValid([CanBeNull] string component, [NotNull] string path)
        {
            if (string.IsNullOrWhiteSpace(component))
            {
                throw CreateExceptionForInvalid(path);
            }

            foreach (char ch in component)
            {
                if (FileNameCharsInvalid.Contains(ch))
                {
                    throw CreateExceptionForInvalid(path);
                }
            }
        }

        [NotNull]
        private static ArgumentException CreateExceptionForInvalid([NotNull] string path)
        {
            return new ArgumentException($"The path '{path}' is invalid.", nameof(path));
        }

        private AbsolutePath([NotNull] [ItemNotNull] IReadOnlyList<string> components, int offset)
        {
            Components = components;
            Offset = offset;
        }

        [NotNull]
        public AbsolutePath MoveDown()
        {
            if (IsAtEnd)
            {
                throw new InvalidOperationException();
            }

            return new AbsolutePath(Components, Offset + 1);
        }

        [NotNull]
        public AbsolutePath MoveUp()
        {
            if (IsAtStart)
            {
                throw new InvalidOperationException();
            }

            return new AbsolutePath(Components, Offset - 1);
        }

        [NotNull]
        public string GetText()
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), Components);
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

                if (index == Offset)
                {
                    textBuilder.Append('[');
                }

                textBuilder.Append(Components[index]);

                if (index == Offset)
                {
                    textBuilder.Append(']');
                }
            }

            return textBuilder.ToString();
        }
    }
}
