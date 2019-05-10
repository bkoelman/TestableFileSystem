using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using JetBrains.Annotations;
using Xunit.Sdk;

namespace TestableFileSystem.Fakes.Tests.TestAttributes
{
    internal sealed class CanRunOnFileSystemAttribute : DataAttribute
    {
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
            : this(FileSystemRunConditions.None)
        {
        }

        public CanRunOnFileSystemAttribute(FileSystemRunConditions conditions)
        {
            enableRunOnFileSystem = EvaluateRunOnFileSystem(conditions);
        }

        [NotNull]
        [ItemNotNull]
        public override IEnumerable<object[]> GetData([NotNull] MethodInfo testMethod)
        {
            return enableRunOnFileSystem ? TrueFalseArray : TrueArray;
        }

        private bool EvaluateRunOnFileSystem(FileSystemRunConditions conditions)
        {
#if NETCOREAPP3_0
            if (conditions.HasFlag(FileSystemRunConditions.RequiresAdministrativeRights) && !HasAdministrativeRights())
            {
                return false;
            }

            return true;
#else
            return false;
#endif
        }

        private bool HasAdministrativeRights()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
