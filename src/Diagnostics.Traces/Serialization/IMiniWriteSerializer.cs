namespace Diagnostics.Traces.Serialization
{
    public interface IWritableBuffer
    {
        void Write(ReadOnlySpan<byte> buffer);
    }
    public readonly struct DefaultWritableBuffer : IWritableBuffer, IDisposable
    {
        public DefaultWritableBuffer()
        {
            BufferWriter = new ArrayPoolBufferWriter<byte>();
        }

        public ArrayPoolBufferWriter<byte> BufferWriter { get; }

        public void Dispose()
        {
            BufferWriter.Dispose();
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
            var sp = BufferWriter.GetSpan(buffer.Length);
            buffer.CopyTo(sp);
            BufferWriter.Advance(buffer.Length);
        }
    }
    public interface IMiniWriteSerializer: IWritableBuffer
    {
        bool IsEntryScoped { get; }

        Span<byte> GetScopedBuffer();

        bool TryEntryScope(int hitSize = 0);

        bool DeleteScope();

        bool FlushScope();
 
        bool CanWrite(int length);
    }
    public interface IMiniReadSerializer
    {
        bool CanSeek { get; }

        bool CanRead(int length);

        void Read(Span<byte> buffer);
    }
}
