namespace Diagnostics.Traces.Serialization
{
    public interface IMiniWriteSerializer
    {
        bool CanWrite(int length);

        void Write(ReadOnlySpan<byte> buffer);

        bool Flush();
    }
    public interface IMiniReadSerializer
    {
        bool CanSeek { get; }

        bool CanRead(int length);

        void Read(Span<byte> buffer);
    }
}
