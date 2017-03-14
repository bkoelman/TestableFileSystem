using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Tests.Builders
{
    internal interface ITestDataBuilder<out T>
    {
        [NotNull]
        T Build();
    }
}
