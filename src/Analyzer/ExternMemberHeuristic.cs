using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace TestableFileSystem.Analyzer
{
    internal static class ExternMemberHeuristic
    {
        public static bool HasFilePathParameter([NotNull] IMethodSymbol methodSymbol)
        {
            return methodSymbol.Parameters.Length > 0 && IsPathParameter(methodSymbol.Parameters.First().Name);
        }

        private static bool IsPathParameter([NotNull] string parameterName)
        {
            return string.Equals(parameterName, "path", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parameterName, "filename", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parameterName, "outputFileName", StringComparison.OrdinalIgnoreCase);
        }
    }
}
