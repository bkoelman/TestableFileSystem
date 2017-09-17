using JetBrains.Annotations;
using TestableFileSystem.Analyzer.Tests.RoslynTestFramework;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Analyzer.Tests
{
    public sealed class ParsedSourceCode
    {
        [NotNull]
        public AnalyzerTestContext TestContext { get; }

        public ParsedSourceCode([NotNull] AnalyzerTestContext testContext)
        {
            Guard.NotNull(testContext, nameof(testContext));

            TestContext = testContext;
        }
    }
}
