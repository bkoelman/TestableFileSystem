using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Analyzer.Tests.RoslynTestFramework
{
    /// <summary />
    internal sealed class DocumentWithSpans
    {
        [NotNull]
        public Document Document { get; }

        [NotNull]
        public IList<TextSpan> TextSpans { get; }

        public DocumentWithSpans([NotNull] Document document, [NotNull] IList<TextSpan> textSpans)
        {
            Guard.NotNull(document, nameof(document));
            Guard.NotNull(textSpans, nameof(textSpans));

            Document = document;
            TextSpans = textSpans;
        }
    }
}
