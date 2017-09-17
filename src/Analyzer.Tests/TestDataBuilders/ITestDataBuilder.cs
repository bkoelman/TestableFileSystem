using JetBrains.Annotations;

namespace TestableFileSystem.Analyzer.Tests.TestDataBuilders
{
    internal interface ITestDataBuilder<out T>
    {
        [NotNull]
        // ReSharper disable once UnusedMemberInSuper.Global
        T Build();
    }
}
