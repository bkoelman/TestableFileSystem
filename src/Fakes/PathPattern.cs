using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class PathPattern
    {
        private const char AsteriskChar = '*';
        private const char QuestionChar = '?';

        [NotNull]
        private static readonly char[] PatternCharsInvalid = GetPatternCharsInvalid();

        [NotNull]
        private readonly Sequence root;

        [CanBeNull]
        public PathPattern SubPattern { get; }

        [NotNull]
        private static char[] GetPatternCharsInvalid()
        {
            List<char> characters = Path.GetInvalidFileNameChars().ToList();
            characters.Remove(AsteriskChar);
            characters.Remove(QuestionChar);
            return characters.ToArray();
        }

        private PathPattern([NotNull] Sequence root, [CanBeNull] PathPattern subPattern)
        {
            this.root = root;
            SubPattern = subPattern;
        }

        [NotNull]
        public static PathPattern Create([NotNull] string pattern)
        {
            Guard.NotNull(pattern, nameof(pattern));

            pattern = pattern.TrimEnd();

            if (string.IsNullOrWhiteSpace(pattern))
            {
                return new PathPattern(EmptySequence.Default, null);
            }

            if (StartsWithPathSeparator(pattern) || StartsWithDriveLetter(pattern))
            {
                throw ErrorFactory.System.SearchPatternMustNotBeDriveOrUnc(nameof(pattern));
            }

            PathPattern root = null;
            foreach (string directoryPattern in pattern.Split(PathFacts.DirectorySeparatorChars).Reverse())
            {
                AssertDirectoryPatternIsValid(directoryPattern);

                Sequence sequence = ParsePattern(directoryPattern);

                if (root != null && SequenceContainsWildcards(sequence))
                {
                    throw ErrorFactory.System.FileOrDirectoryOrVolumeIsIncorrect();
                }

                root = new PathPattern(sequence, root);
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            return root;
        }

        [AssertionMethod]
        private static void AssertDirectoryPatternIsValid([NotNull] string pattern)
        {
            AssertIsNotWhiteSpace(pattern);
            AssertIsNotParentReference(pattern);
            AssertContainsNoInvalidCharacters(pattern);
        }

        private static void AssertIsNotWhiteSpace([NotNull] string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw ErrorFactory.System.SearchPatternMustNotBeDriveOrUnc(nameof(pattern));
            }
        }

        private static void AssertIsNotParentReference([NotNull] string pattern)
        {
            if (pattern == "..")
            {
                throw ErrorFactory.System.SearchPatternCannotContainParent(nameof(pattern));
            }
        }

        private static void AssertContainsNoInvalidCharacters([NotNull] string pattern)
        {
            foreach (char ch in pattern)
            {
                if (PatternCharsInvalid.Contains(ch))
                {
                    throw ErrorFactory.System.IllegalCharactersInPath(nameof(pattern));
                }
            }
        }

        private static bool SequenceContainsWildcards([NotNull] Sequence sequence)
        {
            Sequence current = sequence;
            while (current != null)
            {
                if (!(current is TextSequence))
                {
                    return true;
                }

                current = current.Next;
            }

            return false;
        }

        private static bool StartsWithPathSeparator([NotNull] string pattern)
        {
            return PathFacts.DirectorySeparatorChars.Contains(pattern[0]);
        }

        private static bool StartsWithDriveLetter([NotNull] string pattern)
        {
            return pattern.Length > 1 && pattern[1] == Path.VolumeSeparatorChar;
        }

        [NotNull]
        private static Sequence ParsePattern([NotNull] string pattern)
        {
            var builder = new StringBuilder(pattern.Length);
            Sequence root = null;

            foreach (char ch in pattern.Reverse())
            {
                if (ch == AsteriskChar)
                {
                    if (builder.Length > 0)
                    {
                        root = new TextSequence(builder.ToString(), root);
                        builder.Clear();
                    }

                    root = new MultiWildcardSequence(root);
                }
                else if (ch == QuestionChar)
                {
                    if (builder.Length > 0)
                    {
                        root = new TextSequence(builder.ToString(), root);
                        builder.Clear();
                    }

                    root = new SingleWildcardSequence(root);
                }
                else
                {
                    builder.Insert(0, ch);
                }
            }

            if (builder.Length > 0)
            {
                root = new TextSequence(builder.ToString(), root);
            }

            if (root == null)
            {
                throw Guard.Unreachable();
            }

            return root;
        }

        public bool IsMatch([NotNull] string text)
        {
            Guard.NotNull(text, nameof(text));

            if (text.IndexOfAny(PathFacts.DirectorySeparatorChars) != -1)
            {
                throw new InvalidOperationException("Only one file or directory level can be matched at a time.");
            }

            return root.IsMatch(text);
        }

        public override string ToString()
        {
            return SubPattern != null ? root + ", " + SubPattern : root.ToString();
        }

        private abstract class Sequence
        {
            [CanBeNull]
            public abstract Sequence Next { get; }

            public abstract bool IsMatch([NotNull] string text);
        }

        private sealed class EmptySequence : Sequence
        {
            [NotNull]
            public static readonly EmptySequence Default = new EmptySequence();

            public override Sequence Next => null;

            private EmptySequence()
            {
            }

            public override bool IsMatch(string text)
            {
                return false;
            }
        }

        private sealed class TextSequence : Sequence
        {
            [NotNull]
            private readonly string characters;

            public override Sequence Next { get; }

            public TextSequence([NotNull] string characters, [CanBeNull] Sequence next)
            {
                Guard.NotNull(characters, nameof(characters));

                this.characters = characters;
                Next = next;
            }

            public override bool IsMatch(string text)
            {
                if (Next == null)
                {
                    return string.Equals(text, characters, StringComparison.OrdinalIgnoreCase);
                }

                if (text.StartsWith(characters, StringComparison.OrdinalIgnoreCase))
                {
                    return Next.IsMatch(text.Substring(characters.Length));
                }

                return false;
            }

            public override string ToString()
            {
                return Next == null ? characters : characters + " > " + Next;
            }
        }

        private sealed class MultiWildcardSequence : Sequence
        {
            public override Sequence Next { get; }

            public MultiWildcardSequence([CanBeNull] Sequence next)
            {
                Next = next;
            }

            public override bool IsMatch(string text)
            {
                if (Next == null)
                {
                    return true;
                }

                for (int index = text.Length - 1; index >= 0; index--)
                {
                    if (Next.IsMatch(text.Substring(index)))
                    {
                        return true;
                    }
                }

                return false;
            }

            public override string ToString()
            {
                return Next == null ? AsteriskChar.ToString() : AsteriskChar + " > " + Next;
            }
        }

        private sealed class SingleWildcardSequence : Sequence
        {
            public override Sequence Next { get; }

            public SingleWildcardSequence([CanBeNull] Sequence next)
            {
                Next = next;
            }

            public override bool IsMatch(string text)
            {
                if (Next == null)
                {
                    return text.Length == 1 || text.Length == 0;
                }

                return Next.IsMatch(text.Substring(1)) || Next.IsMatch(text);
            }

            public override string ToString()
            {
                return Next == null ? QuestionChar.ToString() : QuestionChar + " > " + Next;
            }
        }
    }
}
