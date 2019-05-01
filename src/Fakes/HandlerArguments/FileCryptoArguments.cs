using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class FileCryptoArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public bool IsEncrypt { get; }

        public FileCryptoArguments([NotNull] AbsolutePath path, bool isEncrypt)
        {
            Path = path;
            IsEncrypt = isEncrypt;
        }
    }
}
