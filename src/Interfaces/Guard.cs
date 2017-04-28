using System;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Member precondition checks.
    /// </summary>
    public static class Guard
    {
        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        public static void NotNull<T>([CanBeNull] [NoEnumeration] T value, [NotNull] [InvokerParameterName] string name)
            where T : class
        {
            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(name);
            }
        }

        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        public static void NotNullNorEmpty([CanBeNull] string value, [NotNull] [InvokerParameterName] string name)
        {
            NotNull(value, name);

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"'{name}' cannot be empty.", name);
            }
        }

        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        public static void NotNullNorWhiteSpace([CanBeNull] string value, [NotNull] [InvokerParameterName] string name)
        {
            NotNull(value, name);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"'{name}' cannot be empty or contain only whitespace.", name);
            }
        }

        [NotNull]
        public static Exception Unreachable()
        {
            return new InvalidOperationException("This program location is thought to be unreachable.");
        }
    }
}
