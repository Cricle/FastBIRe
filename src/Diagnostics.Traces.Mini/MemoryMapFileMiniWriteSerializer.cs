using Diagnostics.Traces.Serialization;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.Mini
{
    public class MemoryMapFileMiniWriteSerializer : IMiniWriteSerializer, IDisposable
    {
        private readonly MemoryMapFileManger memoryMapFileManger;

        public long Writed => memoryMapFileManger.Writed;

        public MiniWriteTraceHelper TraceHelper { get; }

        public MemoryMapFileMiniWriteSerializer(MemoryMappedFile file,long capacity)
        {
            memoryMapFileManger = new MemoryMapFileManger(file, capacity);
            memoryMapFileManger.Seek(TraceHeader.Size, SeekOrigin.Begin);
            TraceHelper = new MiniWriteTraceHelper(this);
        }

        public bool CanWrite(int length)
        {
            return memoryMapFileManger.CanWrite(length);
        }

        public bool Flush()
        {
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteHead(TraceHeader header)
        {
            byte* buffer = stackalloc byte[TraceHeader.Size];
            Unsafe.Write(buffer, header);
            memoryMapFileManger.WriteHead(new ReadOnlySpan<byte>(buffer, TraceHeader.Size));
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
            memoryMapFileManger.Write(buffer);
        }
        public void Seek(int offset, SeekOrigin origin)
        {
            memoryMapFileManger.Seek(offset, origin);
        }

        public void Dispose()
        {
            memoryMapFileManger.Dispose();
        }
    }
}
