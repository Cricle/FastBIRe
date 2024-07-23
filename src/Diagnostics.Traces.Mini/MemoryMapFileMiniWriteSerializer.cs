using Diagnostics.Traces.Serialization;
using System.Buffers;
using System.Runtime.CompilerServices;
using ZstdSharp;

namespace Diagnostics.Traces.Mini
{
    public class MemoryMapFileMiniWriteSerializer : BufferMiniWriteSerializer
    {
        private readonly MemoryMapFileManger memoryMapFileManger;
        private readonly Compressor compressor;

        public long Writed => memoryMapFileManger.Writed;

        public MiniWriteTraceHelper TraceHelper { get; }

        public MemoryMapFileMiniWriteSerializer(string filePath, long capacity)
        {
            memoryMapFileManger = new MemoryMapFileManger(filePath, capacity);
            memoryMapFileManger.Seek(TraceHeader.Size, SeekOrigin.Begin);
            TraceHelper = new MiniWriteTraceHelper(this);
            compressor = new Compressor();
        }

        public override bool CanWrite(int length)
        {
            return memoryMapFileManger.CanWrite(length);
        }
        protected override void WriteCore(ReadOnlySpan<byte> buffer)
        {
            memoryMapFileManger.Write(buffer);
        }
        protected override bool OnFlushScope()
        {
            if (bufferWriter!=null)
            {
                var sp = bufferWriter.WrittenSpan;
                var bound = Compressor.GetCompressBound(sp.Length);
                var buffer = ArrayPool<byte>.Shared.Rent(bound);
                try
                {

                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteHead(TraceHeader header)
        {
            byte* buffer = stackalloc byte[TraceHeader.Size];
            Unsafe.Write(buffer, header);
            memoryMapFileManger.WriteHead(new ReadOnlySpan<byte>(buffer, TraceHeader.Size));
        }

        public void Seek(int offset, SeekOrigin origin)
        {
            memoryMapFileManger.Seek(offset, origin);
        }
        protected override void OnDisposed()
        {
            memoryMapFileManger.Dispose();
            compressor.Dispose();
        }
    }
}
