using System;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class PathNavigator
    {
        [NotNull]
        public AbsolutePath Path { get; }

        private readonly int offset;

        [NotNull]
        public string Name => Path.Components[offset];

        public bool IsAtEnd => offset == Path.Components.Count - 1;

        public PathNavigator([NotNull] AbsolutePath path)
            : this(path, 0)
        {
        }

        private PathNavigator([NotNull] AbsolutePath path, int offset)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            this.offset = offset;
        }

        [NotNull]
        public PathNavigator MoveDown()
        {
            if (IsAtEnd)
            {
                throw new InvalidOperationException();
            }

            return new PathNavigator(Path, offset + 1);
        }

        public override string ToString()
        {
            var textBuilder = new StringBuilder();

            for (int index = 0; index < Path.Components.Count; index++)
            {
                if (textBuilder.Length > 0)
                {
                    textBuilder.Append(System.IO.Path.DirectorySeparatorChar);
                }

                if (index == offset)
                {
                    textBuilder.Append('[');
                }

                textBuilder.Append(Path.Components[index]);

                if (index == offset)
                {
                    textBuilder.Append(']');
                }
            }

            return textBuilder.ToString();
        }
    }
}
