using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.Mini
{
    internal unsafe class MemoryMapFileManger : IDisposable
    {
        private long writed;
        private long capacity;
        private readonly long addCapacity;
        private readonly string filePath;
        private MemoryMappedFile mappedFile;
        private MemoryMappedViewAccessor viewAccessor;

        public MemoryMapFileManger(string filePath, long capacity)
        {
            addCapacity = capacity;
            this.filePath = filePath;
            mappedFile = MemoryMappedFile.CreateFromFile(filePath, FileMode.Create,null,capacity);
            this.capacity = capacity;
            viewAccessor = mappedFile.CreateViewAccessor();
        }

        public MemoryMappedFile MappedFile => mappedFile;

        public long Capacity => capacity;

        public long Writed => writed;

        private void EnsureCapacity(long size)
        {
            if (writed + size >= capacity)
            {
                capacity += addCapacity;
                Dispose();
                mappedFile = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, capacity);
                viewAccessor = mappedFile.CreateViewAccessor();
            }
        }

        public void Seek(int offset,SeekOrigin origin)
        {
            if (origin== SeekOrigin.Begin)
            {
                writed = offset;
            }
            else if (origin== SeekOrigin.End)
            {
                writed = Capacity - offset;
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
            EnsureCapacity(buffer.Length);
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
            EnsureCapacity(buffer.Length);
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
