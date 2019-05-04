using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace TestableFileSystem.Analyzer
{
    internal static class SymbolExtensions
    {
        [NotNull]
        private static readonly ISet<MethodKind> AccessorKinds = new HashSet<MethodKind>
        {
            MethodKind.PropertyGet,
            MethodKind.PropertySet,
            MethodKind.EventAdd,
            MethodKind.EventRemove
        };

        [NotNull]
        public static string GetCompleteTypeName([NotNull] this ITypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString();
        }

        [NotNull]
        public static string GetCompleteMemberName([NotNull] this ISymbol memberSymbol)
        {
            return memberSymbol.ContainingType != null
                ? memberSymbol.ContainingType.ToDisplayString() + "." + memberSymbol.Name
                : memberSymbol.ToDisplayString();
        }

        public static bool IsAccessor([NotNull] this ISymbol memberSymbol)
        {
            return memberSymbol is IMethodSymbol methodSymbol && AccessorKinds.Contains(methodSymbol.MethodKind);
        }
    }
}
