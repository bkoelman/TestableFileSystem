using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Xunit.Sdk;

namespace TestableFileSystem.Fakes.Tests.TestAttributes
{
    internal sealed class CanRunOnFileSystemAttribute : DataAttribute
    {
        [NotNull]
        private static readonly Version ExpectedAssemblyVersion = new Version(4, 1, 1, 0);

        private readonly bool enableRunOnFileSystem;

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

        public CanRunOnFileSystemAttribute()
        {
#if DEBUG
            // Requires that .NET Core SDK 2.2 is installed.

            Version assemblyVersion = typeof(File).GetTypeInfo().Assembly.GetName().Version;
            enableRunOnFileSystem = assemblyVersion == ExpectedAssemblyVersion;
#endif
        }

        [NotNull]
        [ItemNotNull]
        public override IEnumerable<object[]> GetData([NotNull] MethodInfo testMethod)
        {
            return enableRunOnFileSystem ? TrueFalseArray : TrueArray;
        }
    }
}
