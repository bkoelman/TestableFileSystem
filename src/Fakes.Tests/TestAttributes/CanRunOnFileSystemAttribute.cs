using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Xunit.Sdk;

namespace TestableFileSystem.Fakes.Tests.TestAttributes
{
    internal sealed class CanRunOnFileSystemAttribute : DataAttribute
    {
        private static readonly bool EnableRunOnFileSystem = EvaluateRunOnFileSystem();

        [NotNull]
        [ItemNotNull]
        private static readonly IEnumerable<object[]> TrueFalseArray = new[]
        {
            new object[]
            {
                true
            },
            new object[]
            {
                false
            }
        };

        [NotNull]
        [ItemNotNull]
        private static readonly IEnumerable<object[]> TrueArray = new[]
        {
            new object[]
            {
                true
            }
        };

        [NotNull]
        [ItemNotNull]
        public override IEnumerable<object[]> GetData([NotNull] MethodInfo testMethod)
        {
            return EnableRunOnFileSystem ? TrueFalseArray : TrueArray;
        }

        private static bool EvaluateRunOnFileSystem()
        {
#if NETCOREAPP3_0
            return true;
#else
            return false;
#endif
        }
    }
}
