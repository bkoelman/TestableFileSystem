using System;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestableFileSystem.Analyzer.Tests.RoslynTestFramework;
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

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotSupportedException();
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
