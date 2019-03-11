using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Performs operations on <see cref="string" /> instances that contain file or directory path information.
    /// </summary>
    public interface IPath
    {
        /// <inheritdoc cref="Path.GetFullPath(string)" />
        [NotNull]
        string GetFullPath([NotNull] string path);

        /// <inheritdoc cref="Path.GetTempPath" />
        [NotNull]
        string GetTempPath();

        /// <inheritdoc cref="Path.GetTempFileName" />
        [NotNull]
        string GetTempFileName();
    }
}
