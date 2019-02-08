using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace TestableFileSystem.Analyzer
{
    internal sealed class MemberRegistry
    {
        private const string TestableInterfaceNamePrefix = "TestableFileSystem.Interfaces.";

        [NotNull]
        private readonly IDictionary<string, string> memberMap = new Dictionary<string, string>();

        public MemberRegistry([NotNull] TypeRegistry typeRegistry)
        {
            foreach (INamedTypeSymbol testableTypeSymbol in typeRegistry.TestableTypes)
            {
                foreach (string testableMemberName in EnumerateTypeMemberNames(testableTypeSymbol))
                {
                    if (testableMemberName.StartsWith(TestableInterfaceNamePrefix, StringComparison.Ordinal))
                    {
                        string systemMemberName =
                            "System.IO." + testableMemberName.Substring(TestableInterfaceNamePrefix.Length + 1);
                        memberMap.Add(systemMemberName, testableMemberName);
                    }
                }
            }

            foreach (INamedTypeSymbol testableExtensionTypeSymbol in typeRegistry.TestableExtensionTypes)
            {
                foreach (ISymbol testableExtensionMemberSymbol in GetMembers(testableExtensionTypeSymbol))
                {
                    string systemTypeName = RemoveExtensionsSuffix(testableExtensionTypeSymbol.Name);

                    string systemMemberName = "System.IO." + systemTypeName + "." + testableExtensionMemberSymbol.Name;

                    memberMap.Add(systemMemberName, testableExtensionMemberSymbol.GetCompleteMemberName());
                }
            }

            memberMap.Add("System.Environment.GetLogicalDrives", "TestableFileSystem.Interfaces.IDirectory.GetLogicalDrives");
            memberMap.Add("System.IO.DriveInfo.GetDrives", "TestableFileSystem.Interfaces.IFileSystem.GetDrives");
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<string> EnumerateTypeMemberNames([NotNull] INamedTypeSymbol typeSymbol)
        {
            var memberNames = new HashSet<string>(GetMembers(typeSymbol).Select(symbol => symbol.GetCompleteMemberName()));

            foreach (INamedTypeSymbol iface in typeSymbol.AllInterfaces)
            {
                foreach (ISymbol baseMemberSymbol in GetMembers(iface))
                {
                    string memberName = typeSymbol.GetCompleteTypeName() + "." + baseMemberSymbol.Name;
                    memberNames.Add(memberName);
                }
            }

            return memberNames;
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<ISymbol> GetMembers([NotNull] ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers().Where(symbol => !symbol.IsAccessor());
        }

        [NotNull]
        private static string RemoveExtensionsSuffix([NotNull] string testableExtensionTypeName)
        {
            return testableExtensionTypeName.Substring(0, testableExtensionTypeName.Length - "Extensions".Length);
        }

        [CanBeNull]
        public string TryResolveSystemMember([NotNull] string systemMemberName)
        {
            return memberMap.ContainsKey(systemMemberName) ? memberMap[systemMemberName] : null;
        }
    }
}
