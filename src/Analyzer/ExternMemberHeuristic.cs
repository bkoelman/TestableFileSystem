using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace TestableFileSystem.Analyzer
{
    internal sealed class ExternMemberHeuristic
    {
        // TODO: Add heuristic to not report on overloads with ReadOnlySpan<char> arguments from NetCore21.

        [CanBeNull]
        private readonly INamedTypeSymbol streamType;

        [CanBeNull]
        private readonly INamedTypeSymbol stringType;

        private bool HasKnownTypes => streamType != null && stringType != null;

        public ExternMemberHeuristic([NotNull] Compilation compilation)
        {
            streamType = compilation.GetTypeByMetadataName("System.IO.Stream");
            stringType = compilation.GetSpecialType(SpecialType.System_String);
        }

        public bool HasPathAsStringParameter([NotNull] IMethodSymbol methodSymbol)
        {
            return HasKnownTypes && FirstParameterIsPathString(methodSymbol) && HasOverloadWithStreamParameter(methodSymbol);
        }

        private bool FirstParameterIsPathString([NotNull] IMethodSymbol methodSymbol)
        {
            if (methodSymbol.Parameters.Length > 0)
            {
                IParameterSymbol firstParameter = methodSymbol.Parameters.First();

                return IsParameterNamePath(firstParameter.Name) && IsParameterTypeString(firstParameter.Type);
            }

            return false;
        }

        private static bool IsParameterNamePath([NotNull] string parameterName)
        {
            return string.Equals(parameterName, "path", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parameterName, "filename", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parameterName, "outputFileName", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsParameterTypeString([NotNull] ITypeSymbol parameterType)
        {
            return parameterType.Equals(stringType);
        }

        private bool HasOverloadWithStreamParameter([NotNull] IMethodSymbol methodSymbol)
        {
            return methodSymbol.ContainingType.GetMembers(methodSymbol.Name).OfType<IMethodSymbol>()
                .SelectMany(overload => overload.Parameters).Any(IsParameterTypeStream);
        }

        private bool IsParameterTypeStream([NotNull] IParameterSymbol parameter)
        {
            return parameter.Type.Equals(streamType);
        }
    }
}
