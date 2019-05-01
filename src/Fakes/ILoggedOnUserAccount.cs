using JetBrains.Annotations;

namespace TestableFileSystem.Fakes
{
    /// <summary>
    /// Provides access to the user account under which the code is currently executing.
    /// </summary>
    public interface ILoggedOnUserAccount
    {
        /// <summary>
        /// Name of the user account.
        /// </summary>
        [NotNull]
        string UserName { get; }
    }
}
