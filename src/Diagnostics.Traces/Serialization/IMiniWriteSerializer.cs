namespace Diagnostics.Traces.Serialization
{
    public interface IMiniWriteSerializer
    {
        bool IsEntryScoped { get; }

        ReadOnlySpan<byte> GetScopedBuffer();

        bool TryEntryScope(int hitSize = 0);

        bool DeleteScope();

        bool FlushScope();
 
        bool CanWrite(int length);

        void Write(ReadOnlySpan<byte> buffer);

    }
    public interface IMiniReadSerializer
    {
        bool CanSeek { get; }

        bool CanRead(int length);

        void Read(Span<byte> buffer);
    }
}
