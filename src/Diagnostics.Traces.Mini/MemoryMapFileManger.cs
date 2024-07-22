using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.Mini
{
    internal unsafe class MemoryMapFileManger : IDisposable
    {
        private long writed;
        private readonly MemoryMappedFile mappedFile;
        private readonly MemoryMappedViewAccessor viewAccessor;

        public MemoryMapFileManger(MemoryMappedFile mappedFile, long capacity)
        {
            this.mappedFile = mappedFile;
            MappedFile = mappedFile;
            Capacity = capacity;
            viewAccessor = mappedFile.CreateViewAccessor();
        }

        public MemoryMappedFile MappedFile { get; }

        public long Capacity { get; }

        public long Writed => writed;

        public void Seek(int offset,SeekOrigin origin)
        {
            if (origin== SeekOrigin.Begin)
            {
                writed = offset;
            }
            else if (origin== SeekOrigin.End)
            {
                writed = Capacity-offset;
            }
            else
            {
                writed += offset;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanWrite(long length)
        {
            return writed + length < Capacity;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteHead(ReadOnlySpan<byte> buffer)
        {
            byte* ptr = null;
            viewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
            try
            {
                buffer.CopyTo(new Span<byte>(ptr, buffer.Length));
            }
            finally
            {
                viewAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool TryWrite(ReadOnlySpan<byte> buffer)
        {
            if (CanWrite(buffer.Length))
            {
                byte* ptr=null;
                viewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                try
                {
                    buffer.CopyTo(new Span<byte>(ptr + writed, buffer.Length));
                }
                finally
                {
                    viewAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                }
                writed += buffer.Length;
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer)
        {
            if (!TryWrite(buffer))
            {
                throw new InvalidOperationException("The buffer is full");
            }
        }

        public void Dispose()
        {
            viewAccessor.Dispose();
            mappedFile.Dispose();
        }
    }
}
