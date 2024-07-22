using System.Buffers;
using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.Serialization
{
    public class StreamMiniWriteSerializer : IMiniWriteSerializer
    {
        public StreamMiniWriteSerializer(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer)
        {
#if NETSTANDARD2_0 || NET472
            var copy = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(copy);
                Stream.Write(copy, 0, buffer.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(copy);
            }
#else
            Stream.Write(buffer);
#endif
        }
        public bool Flush()
        {
            Stream.Flush();
            return true;
        }

        public bool CanWrite(int length) => true;
    }
}
