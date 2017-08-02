using System;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class FileSetTimeArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public FileTimeKind Kind { get; }
        public bool IsInUtc { get; }
        public DateTime TimeValue { get; }

        public FileSetTimeArguments([NotNull] AbsolutePath path, FileTimeKind kind, bool isInUtc, DateTime timeValue)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            Kind = kind;
            IsInUtc = isInUtc;
            TimeValue = timeValue;
        }
    }
}
