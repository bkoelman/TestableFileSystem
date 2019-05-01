using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Tests
{
    internal static class BufferFactory
    {
        [NotNull]
        public static byte[] SingleByte(byte value)
        {
            return new[]
            {
                value
            };
        }

        [NotNull]
        public static byte[] Create(int size, byte value = (byte)'X')
        {
            var buffer = new byte[size];
            for (int index = 0; index < size; index++)
            {
                buffer[index] = value;
            }

            return buffer;
        }
    }
}
