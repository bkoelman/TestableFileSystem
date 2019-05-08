using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;
using Xunit.Sdk;

namespace TestableFileSystem.Fakes.Tests.TestAttributes
{
    internal sealed class WatcherNotifyTestDataAttribute : DataAttribute
    {
        [NotNull]
        [ItemNotNull]
        private readonly IList<object[]> testCases;

        public WatcherNotifyTestDataAttribute([NotNull] string expectations)
        {
            List<WatcherNotifyTestLine> testLines = ParseLines(expectations).ToList();

            var generator = new TestCaseGenerator(testLines);
            testCases = generator.GenerateCases();
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<WatcherNotifyTestLine> ParseLines([NotNull] string expectations)
        {
            using (var reader = new StringReader(expectations))
            {
                string nextLine;
                while ((nextLine = reader.ReadLine()) != null)
                {
                    if (nextLine.Trim().Length > 0)
                    {
                        var parser = new LineParser(nextLine);

                        WatcherNotifyTestLine testLine = parser.Parse();
                        yield return testLine;
                    }
                }
            }
        }

        [NotNull]
        [ItemNotNull]
        public override IEnumerable<object[]> GetData([NotNull] MethodInfo testMethod)
        {
            return testCases;
        }

        private sealed class LineParser
        {
            [NotNull]
            private readonly string line;

            private int position;

            private bool IsAtEnd => position >= line.Length;

            public LineParser([NotNull] string line)
            {
                Guard.NotNull(line, nameof(line));
                this.line = line;
            }

            [NotNull]
            public WatcherNotifyTestLine Parse()
            {
                Reset();

                SkipToMarkerOffset();
                string expectation = line.Substring(0, position).Trim();

                ConsumeSeparators();
                NotifyFilters filters = ParseNotifyFilterList();

                return new WatcherNotifyTestLine(expectation, filters);
            }

            private void Reset()
            {
                position = 0;
            }

            private void SkipToMarkerOffset()
            {
                int markerOffset = line.IndexOf('@');
                if (markerOffset == -1)
                {
                    throw GetErrorForLine("Missing '@'");
                }

                position = markerOffset;
            }

            private NotifyFilters ParseNotifyFilterList()
            {
                var filters = new HashSet<NotifyFilters>();

                while (!IsAtEnd)
                {
                    NotifyFilters filter = ParseNotifyFilter();
                    if (filters.Contains(filter))
                    {
                        throw GetErrorForLine($"Duplicate filter '{filter}'");
                    }

                    filters.Add(filter);
                    ConsumeSeparators();
                }

                if (filters.Count == 0)
                {
                    throw GetErrorForLine("Missing notify filters");
                }

                NotifyFilters value = 0;
                foreach (NotifyFilters filter in filters)
                {
                    value |= filter;
                }

                return value;
            }

            private NotifyFilters ParseNotifyFilter()
            {
                string name = ConsumeText();
                if (!Enum.TryParse(name, false, out NotifyFilters filters))
                {
                    throw GetErrorForLine($"Unexpected notify filter '{name}'");
                }

                return filters;
            }

            [NotNull]
            private string ConsumeText()
            {
                var nameBuilder = new StringBuilder();

                while (!IsAtEnd && !IsSeparator(line[position]))
                {
                    nameBuilder.Append(line[position]);
                    position++;
                }

                return nameBuilder.ToString();
            }

            private void ConsumeSeparators()
            {
                while (!IsAtEnd && IsSeparator(line[position]))
                {
                    position++;
                }
            }

            private static bool IsSeparator(char ch)
            {
                return char.IsWhiteSpace(ch) || ch == ',' || ch == '@';
            }

            [NotNull]
            private Exception GetErrorForLine([NotNull] string message)
            {
                return new Exception($"{message} on line: '{line}'");
            }
        }

        private sealed class TestCaseGenerator
        {
            [NotNull]
            [ItemNotNull]
            private readonly IList<WatcherNotifyTestLine> testLines;

            public TestCaseGenerator([NotNull] [ItemNotNull] IList<WatcherNotifyTestLine> testLines)
            {
                Guard.NotNull(testLines, nameof(testLines));
                this.testLines = testLines.ToList();
            }

            [NotNull]
            [ItemNotNull]
            public IList<object[]> GenerateCases()
            {
                var builder = new TestCasesBuilder();

                if (testLines.Any())
                {
                    AddCaseForAllFilters(builder);
                    AddCasesForSingleFilter(builder);
                }

                AddCaseForFiltersExcept(builder);

                return builder.Build();
            }

            private void AddCaseForAllFilters([NotNull] TestCasesBuilder builder)
            {
                string expectedText = GetExpectedTextForFilters(TestNotifyFilters.All);
                builder.Add(TestNotifyFilters.All, expectedText);
            }

            private void AddCasesForSingleFilter([NotNull] TestCasesBuilder builder)
            {
                int bitmask = 1;

                NotifyFilters filters = GetUnionForFilters();
                while (filters != 0)
                {
                    var currentFilter = (NotifyFilters)bitmask;

                    if (filters.HasFlag(currentFilter))
                    {
                        builder.Add(currentFilter, GetExpectedTextForFilters(currentFilter));
                        filters &= ~currentFilter;
                    }

                    bitmask = bitmask << 1;
                }
            }

            private void AddCaseForFiltersExcept([NotNull] TestCasesBuilder builder)
            {
                NotifyFilters union = GetUnionForFilters();
                NotifyFilters except = ~union & TestNotifyFilters.All;

                builder.Add(except, "");
            }

            private NotifyFilters GetUnionForFilters()
            {
                NotifyFilters filters = 0;

                foreach (WatcherNotifyTestLine testLine in testLines)
                {
                    filters |= testLine.Filters;
                }

                return filters;
            }

            [NotNull]
            private string GetExpectedTextForFilters(NotifyFilters filters)
            {
                var lines = new List<string>();

                foreach (WatcherNotifyTestLine testLine in testLines)
                {
                    if ((testLine.Filters & filters) != 0)
                    {
                        lines.Add(testLine.Text);
                    }
                }

                return string.Join(Environment.NewLine, lines);
            }

            private sealed class TestCasesBuilder
            {
                [NotNull]
                [ItemNotNull]
                private readonly List<object[]> testCases = new List<object[]>();

                [NotNull]
                [ItemNotNull]
                public IList<object[]> Build()
                {
                    return testCases;
                }

                public void Add(NotifyFilters filters, [NotNull] string expectedText)
                {
                    object[] testCase = ToTestCase(filters, expectedText);
                    testCases.Add(testCase);
                }

                [NotNull]
                [ItemNotNull]
                private static object[] ToTestCase(NotifyFilters filters, [NotNull] string expectedText)
                {
                    return new object[]
                    {
                        filters,
                        expectedText
                    };
                }
            }
        }

        private sealed class WatcherNotifyTestLine
        {
            [NotNull]
            public string Text { get; }

            public NotifyFilters Filters { get; }

            public WatcherNotifyTestLine([NotNull] string text, NotifyFilters filters)
            {
                Guard.NotNullNorWhiteSpace(text, nameof(text));

                Text = text;
                Filters = filters;
            }
        }
    }
}
