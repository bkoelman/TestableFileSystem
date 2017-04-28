using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Tests.Builders
{
    internal interface ITestDataBuilder<out T>
    {
        [NotNull]
        // ReSharper disable once UnusedMemberInSuper.Global
        T Build();
    }
}
