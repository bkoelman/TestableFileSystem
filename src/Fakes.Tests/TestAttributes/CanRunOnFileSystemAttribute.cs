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
        private static readonly Version ExpectedAssemblyVersion = new Version(4, 1, 0, 0);

        [NotNull]
        private static readonly Version CurrentAssemblyVersion = GetCurrentAssemblyVersion();

        private readonly bool enableRunOnFileSystem = CurrentAssemblyVersion >= ExpectedAssemblyVersion;

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
            return enableRunOnFileSystem ? TrueFalseArray : TrueArray;
        }

        [NotNull]
        private static Version GetCurrentAssemblyVersion()
        {
            Version assemblyVersion = typeof(File).GetTypeInfo().Assembly.GetName().Version;

            Console.WriteLine(
                $"Detected assembly version '{assemblyVersion}' in: '{typeof(File).GetTypeInfo().Assembly.FullName}'.");

            return assemblyVersion;
        }
    }
}
