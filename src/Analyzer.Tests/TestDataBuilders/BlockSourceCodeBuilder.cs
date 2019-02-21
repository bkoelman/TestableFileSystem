using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Analyzer.Tests.TestDataBuilders
{
    /// <summary />
    internal sealed class BlockSourceCodeBuilder : SourceCodeBuilder
    {
        [NotNull]
        [ItemNotNull]
        private readonly List<string> blocks = new List<string>();

        public BlockSourceCodeBuilder()
            : base(DefaultNamespaceImports)
        {
        }

        protected override string GetSourceCode()
        {
            var builder = new StringBuilder();

            AppendClassStart(builder);
            AppendMethodStart(builder);
            AppendCodeBlocks(builder);
            AppendScopeEnd(builder);
            AppendScopeEnd(builder);

            return builder.ToString();
        }

        private static void AppendClassStart([NotNull] StringBuilder builder)
        {
            builder.AppendLine("public class Test");
            builder.AppendLine("{");
        }

        private static void AppendMethodStart([NotNull] StringBuilder builder)
        {
            builder.AppendLine("public void Method()");
            builder.AppendLine("{");
        }

        private void AppendCodeBlocks([NotNull] StringBuilder builder)
        {
            string codeBlock = GetLinesOfCode(blocks);
            builder.AppendLine(codeBlock);
        }

        private static void AppendScopeEnd([NotNull] StringBuilder builder)
        {
            builder.AppendLine("}");
        }

        [NotNull]
        public BlockSourceCodeBuilder InDefaultMethod([NotNull] string blockCode)
        {
            Guard.NotNull(blockCode, nameof(blockCode));

            blocks.Add(blockCode);
            return this;
        }
    }
}
