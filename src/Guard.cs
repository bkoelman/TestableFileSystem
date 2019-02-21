using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace TestableFileSystem.Utilities
{
    /// <summary>
    /// Member precondition checks.
    /// </summary>
    internal static class Guard
    {
        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        public static void NotNull<T>([CanBeNull] [NoEnumeration] T value, [NotNull] [InvokerParameterName] string name)
            where T : class
        {
            if (value is null)
            {
                throw new ArgumentNullException(name);
            }
        }

        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        public static void NotNullNorEmpty<T>([CanBeNull] [ItemCanBeNull] IEnumerable<T> value,
            [NotNull] [InvokerParameterName] string name)
        {
            NotNull(value, name);

            if (!value.Any())
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

        [AssertionMethod]
        public static void InRangeInclusive(int value, [NotNull] [InvokerParameterName] string name, int minValue, int maxValue)
        {
            if (value < minValue || value > maxValue)
            {
                if (minValue == maxValue)
                {
                    throw new ArgumentOutOfRangeException(name, value, $"{name} must be {minValue}.");
                }

                throw new ArgumentOutOfRangeException(name, value, $"{name} must be in range [{minValue}-{maxValue}].");
            }
        }
    }
}
