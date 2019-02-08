using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace TestableFileSystem.Analyzer
{
    internal sealed class TypeRegistry
    {
        [NotNull]
        private readonly IDictionary<INamedTypeSymbol, INamedTypeSymbol> typeMap;

        [NotNull]
        [ItemNotNull]
        public ICollection<INamedTypeSymbol> SystemTypes => typeMap.Keys;

        [NotNull]
        [ItemNotNull]
        public ICollection<INamedTypeSymbol> TestableTypes => typeMap.Values;

        [NotNull]
        [ItemNotNull]
        public ICollection<INamedTypeSymbol> TestableExtensionTypes { get; }

        public bool IsComplete { get; }

        public TypeRegistry([NotNull] Compilation compilation)
        {
            var builder = new RegistryBuilder(compilation);

            // TODO: Verify that loading a subset of the types still works on NetStandard13 / NetCore11.

            builder.IncludePair("Directory", "IDirectory");
            builder.IncludePair("DirectoryInfo", "IDirectoryInfo");
            builder.IncludePair("File", "IFile");
            builder.IncludePair("FileInfo", "IFileInfo");
            builder.IncludePair("FileSystemInfo", "IFileSystemInfo");
            builder.IncludePair("DriveInfo", "IDriveInfo");
            builder.IncludePair("FileStream", "IFileStream");
            builder.IncludePair("FileSystemWatcher", "IFileSystemWatcher");

            typeMap = builder.Build();

            TestableExtensionTypes = new List<INamedTypeSymbol>
            {
                compilation.GetTypeByMetadataName(CodeNamespace.Combine(CodeNamespace.TestableInterfaces, "FileExtensions")),
                compilation.GetTypeByMetadataName(CodeNamespace.Combine(CodeNamespace.TestableInterfaces, "FileInfoExtensions"))
            };

            IsComplete = builder.FoundAll && TestableExtensionTypes.All(x => x != null);
        }

        [CanBeNull]
        public INamedTypeSymbol TryResolveSystemType([NotNull] ITypeSymbol systemTypeSymbol)
        {
            return systemTypeSymbol is INamedTypeSymbol typeSymbol && typeMap.ContainsKey(typeSymbol)
                ? typeMap[typeSymbol]
                : null;
        }

        private sealed class RegistryBuilder
        {
            [NotNull]
            private readonly Compilation compilation;

            [NotNull]
            private readonly Dictionary<INamedTypeSymbol, INamedTypeSymbol> map =
                new Dictionary<INamedTypeSymbol, INamedTypeSymbol>();

            public bool FoundAll = true;

            public RegistryBuilder([NotNull] Compilation compilation)
            {
                this.compilation = compilation;
            }

            [NotNull]
            public IDictionary<INamedTypeSymbol, INamedTypeSymbol> Build()
            {
                return map;
            }

            public void IncludePair([NotNull] string systemTypeName, [NotNull] string testableTypeName)
            {
                INamedTypeSymbol systemTypeSymbol =
                    compilation.GetTypeByMetadataName(CodeNamespace.Combine(CodeNamespace.SystemIO, systemTypeName));
                INamedTypeSymbol testableTypeSymbol =
                    compilation.GetTypeByMetadataName(CodeNamespace.Combine(CodeNamespace.TestableInterfaces, testableTypeName));

                if (systemTypeSymbol == null || testableTypeSymbol == null)
                {
                    FoundAll = false;
                }
                else
                {
                    map.Add(systemTypeSymbol, testableTypeSymbol);
                }
            }
        }
    }
}
