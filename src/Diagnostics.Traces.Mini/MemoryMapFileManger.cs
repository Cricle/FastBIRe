using Diagnostics.Traces.Mini.Exceptions;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using ZstdSharp.Unsafe;

namespace Diagnostics.Traces.Mini
{
    internal unsafe class MemoryMapFileManger : IDisposable
    {
        private long writed;
        private long capacity;
        private readonly long addCapacity;
        private readonly string filePath;
        private readonly bool autoCapacity;
        private MemoryMappedFile mappedFile;
        private MemoryMappedViewAccessor viewAccessor;
        private SafeMemoryMappedViewHandle SafeMemoryMappedViewHandle => viewAccessor.SafeMemoryMappedViewHandle;
        public MemoryMapFileManger(string filePath, long capacity, bool autoCapacity)
        {
            addCapacity = capacity;
            this.filePath = filePath;
            mappedFile = MemoryMappedFile.CreateFromFile(filePath, FileMode.Create, null, capacity);
            this.capacity = capacity;
            viewAccessor = mappedFile.CreateViewAccessor();
            this.autoCapacity = autoCapacity;
        }

        public MemoryMappedFile MappedFile => mappedFile;

        public long Capacity => capacity;

        public long Writed => writed;

        private void EnsureCapacity(long size)
        {
            if (writed + size >= capacity)
            {
                if (!autoCapacity)
                {
                    ThrowNoEnoughMemory(size);
                }
                capacity += addCapacity;
                Dispose();
                mappedFile = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, capacity);
                viewAccessor = mappedFile.CreateViewAccessor();
            }
        }

        public void Seek(int offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                writed = offset;
            }
            else if (origin == SeekOrigin.End)
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
            EnsureCapacity(buffer.Length);
#if NET8_0_OR_GREATER
            SafeMemoryMappedViewHandle.WriteSpan((ulong)writed, buffer);
#else
            byte* ptr = null;
            SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
            try
            {
                buffer.CopyTo(new Span<byte>(ptr, buffer.Length));
            }
            finally
            {
                SafeMemoryMappedViewHandle.ReleasePointer();
            }
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool TryWrite(ReadOnlySpan<byte> buffer)
        {
            EnsureCapacity(buffer.Length);
            if (CanWrite(buffer.Length))
            {
#if NET8_0_OR_GREATER
                SafeMemoryMappedViewHandle.WriteSpan((ulong)writed, buffer);
#else
                byte* ptr = null;
                SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                try
                {
                    buffer.CopyTo(new Span<byte>(ptr + writed, buffer.Length));
                }
                finally
                {
                    SafeMemoryMappedViewHandle.ReleasePointer();
                }
#endif
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
                ThrowNoEnoughMemory(buffer.Length);
            }
        }
        private void ThrowNoEnoughMemory(long writeCount)
        {
            throw new MemoryMapFileBufferFullException($"The buffer is full, Capacity = {capacity}, written = {writed}, needs = {writeCount}", capacity, writed, writeCount);
        }

        public void Dispose()
        {
            viewAccessor.Dispose();
            mappedFile.Dispose();
        }
    }
}
