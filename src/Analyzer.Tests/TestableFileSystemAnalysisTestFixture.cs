using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynTestFramework;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Analyzer.Tests
{
    public abstract class TestableFileSystemAnalysisTestFixture : AnalysisTestFixture
    {
        protected override string DiagnosticId => FileSystemUsageAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new FileSystemUsageAnalyzer();
        }

        protected void VerifyFileSystemDiagnostic([NotNull] ParsedSourceCode source,
            [NotNull] [ItemNotNull] params string[] messages)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(messages, nameof(messages));

            AssertDiagnostics(source.TestContext, messages);
        }
    }
}
