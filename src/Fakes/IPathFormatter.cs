using JetBrains.Annotations;

namespace TestableFileSystem.Fakes
{
    internal interface IPathFormatter
    {
        [NotNull]
        AbsolutePath GetPath();
    }
}
