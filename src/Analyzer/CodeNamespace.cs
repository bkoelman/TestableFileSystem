using JetBrains.Annotations;

namespace TestableFileSystem.Analyzer
{
    internal static class CodeNamespace
    {
        // ReSharper disable once InconsistentNaming
        public const string SystemIO = "System.IO";
        public const string TestableInterfaces = "TestableFileSystem.Interfaces";

        [NotNull]
        public static string Combine([NotNull] [ItemNotNull] params string[] namespaceNames)
        {
            return string.Join(".", namespaceNames);
        }
    }
}
