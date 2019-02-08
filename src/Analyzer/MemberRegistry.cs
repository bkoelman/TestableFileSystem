using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace TestableFileSystem.Analyzer
{
    internal sealed class MemberRegistry
    {
        private const string TestableInterfaceNamePrefix = CodeNamespace.TestableInterfaces + ".";

        [NotNull]
        private readonly IDictionary<string, string> memberMap = new Dictionary<string, string>();

        [NotNull]
        private readonly IDictionary<string, string> constructorMap = new Dictionary<string, string>();

        public MemberRegistry([NotNull] TypeRegistry typeRegistry)
        {
            foreach (INamedTypeSymbol systemTypeSymbol in typeRegistry.SystemTypes)
            {
                if (systemTypeSymbol.Constructors.Any() && !systemTypeSymbol.IsAbstract)
                {
                    string systemTypeName = systemTypeSymbol.GetCompleteTypeName();
                    string testableMemberName = CodeNamespace.Combine(CodeNamespace.TestableInterfaces,
                        "IFileSystem.Construct" + systemTypeSymbol.Name);

                    constructorMap.Add(systemTypeName, testableMemberName);
                }
            }

            foreach (INamedTypeSymbol testableTypeSymbol in typeRegistry.TestableTypes)
            {
                foreach (string testableMemberName in EnumerateTypeMemberNames(testableTypeSymbol))
                {
                    if (testableMemberName.StartsWith(TestableInterfaceNamePrefix, StringComparison.Ordinal))
                    {
                        string systemMemberName = CodeNamespace.Combine(CodeNamespace.SystemIO,
                            testableMemberName.Substring(TestableInterfaceNamePrefix.Length + 1));

                        memberMap.Add(systemMemberName, testableMemberName);
                    }
                }
            }

            foreach (INamedTypeSymbol testableExtensionTypeSymbol in typeRegistry.TestableExtensionTypes)
            {
                foreach (ISymbol testableExtensionMemberSymbol in GetMembers(testableExtensionTypeSymbol))
                {
                    string systemTypeName = RemoveExtensionsSuffix(testableExtensionTypeSymbol.Name);
                    string systemMemberName = CodeNamespace.Combine(CodeNamespace.SystemIO, systemTypeName,
                        testableExtensionMemberSymbol.Name);

                    memberMap.Add(systemMemberName, testableExtensionMemberSymbol.GetCompleteMemberName());
                }
            }

            // Additional redirects that do not match earlier conventions.
            memberMap["System.Environment.GetLogicalDrives"] = "TestableFileSystem.Interfaces.IDirectory.GetLogicalDrives";
            memberMap["System.IO.DriveInfo.GetDrives"] = "TestableFileSystem.Interfaces.IFileSystem.GetDrives";
            constructorMap["System.IO.FileStream"] = "TestableFileSystem.Interfaces.IFile.Open";
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<string> EnumerateTypeMemberNames([NotNull] INamedTypeSymbol typeSymbol)
        {
            var memberNames = new HashSet<string>(GetMembers(typeSymbol).Select(symbol => symbol.GetCompleteMemberName()));

            foreach (INamedTypeSymbol interfaceSymbol in typeSymbol.AllInterfaces)
            {
                foreach (ISymbol baseMemberSymbol in GetMembers(interfaceSymbol))
                {
                    string memberName = CodeNamespace.Combine(typeSymbol.GetCompleteTypeName(), baseMemberSymbol.Name);
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

        [CanBeNull]
        public string TryResolveSystemConstructor([NotNull] string systemTypeName)
        {
            return constructorMap.ContainsKey(systemTypeName) ? constructorMap[systemTypeName] : null;
        }
    }
}
