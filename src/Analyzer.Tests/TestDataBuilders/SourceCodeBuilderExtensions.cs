using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Analyzer.Tests.TestDataBuilders
{
    /// <summary />
    internal static class SourceCodeBuilderExtensions
    {
        [NotNull]
        public static TBuilder Using<TBuilder>([NotNull] this TBuilder source, [CanBeNull] string codeNamespace)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(codeNamespace))
            {
                source.Editor.IncludeNamespaceImport(codeNamespace);
            }

            return source;
        }

        [NotNull]
        public static TBuilder WithReference<TBuilder>([NotNull] this TBuilder source, [NotNull] Assembly assembly)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(assembly, nameof(assembly));

            PortableExecutableReference reference = MetadataReference.CreateFromFile(assembly.Location);

            source.Editor.UpdateTestContext(context => context.WithReferences(context.References.Add(reference)));

            return source;
        }
    }
}
