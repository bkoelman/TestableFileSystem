using JetBrains.Annotations;

namespace TestableFileSystem.Fakes
{
    public interface ILoggedOnUserAccount
    {
        [NotNull]
        string UserName { get; }
    }
}
