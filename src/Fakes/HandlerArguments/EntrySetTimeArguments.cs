using System;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class EntrySetTimeArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public FileTimeKind Kind { get; }
        public bool IsInUtc { get; }
        public DateTime TimeValue { get; }

        public EntrySetTimeArguments([NotNull] AbsolutePath path, FileTimeKind kind, bool isInUtc, DateTime timeValue)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            Kind = kind;
            IsInUtc = isInUtc;
            TimeValue = timeValue;
        }
    }
}
