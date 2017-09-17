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

            builder.IncludePair("System.IO.Directory", "TestableFileSystem.Interfaces.IDirectory");
            builder.IncludePair("System.IO.DirectoryInfo", "TestableFileSystem.Interfaces.IDirectoryInfo");
            builder.IncludePair("System.IO.File", "TestableFileSystem.Interfaces.IFile");
            builder.IncludePair("System.IO.FileInfo", "TestableFileSystem.Interfaces.IFileInfo");
            builder.IncludePair("System.IO.FileSystemInfo", "TestableFileSystem.Interfaces.IFileSystemInfo");
            builder.IncludePair("System.IO.FileStream", "TestableFileSystem.Interfaces.IFileStream");

            typeMap = builder.Build();

            TestableExtensionTypes = new List<INamedTypeSymbol>
            {
                compilation.GetTypeByMetadataName("TestableFileSystem.Interfaces.FileExtensions"),
                compilation.GetTypeByMetadataName("TestableFileSystem.Interfaces.FileInfoExtensions")
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
                INamedTypeSymbol systemTypeSymbol = compilation.GetTypeByMetadataName(systemTypeName);
                INamedTypeSymbol testableTypeSymbol = compilation.GetTypeByMetadataName(testableTypeName);

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
