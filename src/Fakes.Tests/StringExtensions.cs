using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Tests
{
    internal static class StringExtensions
    {
        [NotNull]
        public static string TrimLines([NotNull] this string text, bool skipEmpty = true)
        {
            Guard.NotNull(text, nameof(text));

            var newLines = new List<string>();

            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string trimmed = line.Trim();
                    if (!skipEmpty || trimmed.Length > 0)
                    {
                        newLines.Add(trimmed);
                    }
                }
            }

            return string.Join(Environment.NewLine, newLines);
        }
    }
}
