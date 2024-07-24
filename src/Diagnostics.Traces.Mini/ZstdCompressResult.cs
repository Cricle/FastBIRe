using System.Buffers;

namespace Diagnostics.Traces.Mini
{
    public readonly struct ZstdCompressResult:IDisposable
    {
        public readonly byte[] Result;

        public readonly int Length;

        internal ZstdCompressResult(byte[] result, int length)
        {
            Result = result;
            Length = length;
        }

        public Span<byte> Span => Result.AsSpan(0, Length);

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(Result);
        }
    }
}
