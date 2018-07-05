using System;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Tests
{
    internal static class TestNotifyFilters
    {
        [NotNull]
        private static readonly Lazy<NotifyFilters> LazyAll = new Lazy<NotifyFilters>(CombineEnumValues<NotifyFilters>);

        public static NotifyFilters All => LazyAll.Value;

        [NotNull]
        private static TEnum CombineEnumValues<TEnum>()
            where TEnum : Enum
        {
            int result = 0;
            foreach (int value in Enum.GetValues(typeof(TEnum)))
            {
                result |= value;
            }

            return (TEnum)(object)result;
        }
    }
}
