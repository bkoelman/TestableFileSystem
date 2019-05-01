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

            builder.IncludePair("File", "IFile");
            builder.IncludePair("Directory", "IDirectory");
            builder.IncludePair("Path", "IPath");
            builder.IncludePair("FileInfo", "IFileInfo");
            builder.IncludePair("DirectoryInfo", "IDirectoryInfo");
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

            IsComplete = typeMap.Any() && TestableExtensionTypes.All(x => x != null);
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

                if (systemTypeSymbol != null && testableTypeSymbol != null)
                {
                    map.Add(systemTypeSymbol, testableTypeSymbol);
                }
            }
        }
    }
}
