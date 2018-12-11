using System.IO;

namespace TestableFileSystem.Fakes.Tests
{
    internal static class NotifyFiltersExtensions
    {
        public static NotifyFilters Except(this NotifyFilters baseFilter, NotifyFilters exclude)
        {
            return baseFilter & ~exclude;
        }
    }
}