using System.Buffers;
using System.Runtime.CompilerServices;

namespace Diagnostics.Traces
{
    //https://github.com/CommunityToolkit/dotnet/blob/main/src/CommunityToolkit.HighPerformance/Buffers/ArrayPoolBufferWriter%7BT%7D.cs
    public sealed class ArrayPoolBufferWriter<T>
    {
        private const int DefaultInitialBufferSize = 256;

        private readonly ArrayPool<T> pool;

        private T[]? array;

        private int index;

        public ArrayPoolBufferWriter()
            : this(ArrayPool<T>.Shared, DefaultInitialBufferSize)
        {
        }

        public ArrayPoolBufferWriter(ArrayPool<T> pool)
            : this(pool, DefaultInitialBufferSize)
        {
        }

        public ArrayPoolBufferWriter(int initialCapacity)
            : this(ArrayPool<T>.Shared, initialCapacity)
        {
        }

        public ArrayPoolBufferWriter(ArrayPool<T> pool, int initialCapacity)
        {
            this.pool = pool;
            this.array = pool.Rent(initialCapacity);
            this.index = 0;
        }

        public ReadOnlyMemory<T> WrittenMemory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                T[]? array = this.array;

                if (array is null)
                {
                    ThrowObjectDisposedException();
                }

                return array!.AsMemory(0, this.index);
            }
        }

        public ReadOnlySpan<T> WrittenSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                T[]? array = this.array;

                if (array is null)
                {
                    ThrowObjectDisposedException();
                }

                return array!.AsSpan(0, this.index);
            }
        }

        public int WrittenCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.index;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                T[]? array = this.array;

                if (array is null)
                {
                    ThrowObjectDisposedException();
                }

                return array!.Length;
            }
        }

        public int FreeCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                T[]? array = this.array;

                if (array is null)
                {
                    ThrowObjectDisposedException();
                }

                return array!.Length - this.index;
            }
        }

        public void Clear()
        {
            T[]? array = this.array;

            if (array is null)
            {
                ThrowObjectDisposedException();
            }

            array.AsSpan(0, this.index).Clear();

            this.index = 0;
        }

        public void Advance(int count)
        {
            T[]? array = this.array;

            if (array is null)
            {
                ThrowObjectDisposedException();
            }

            if (count < 0)
            {
                ThrowArgumentOutOfRangeExceptionForNegativeCount();
            }

            if (this.index > array!.Length - count)
            {
                ThrowArgumentExceptionForAdvancedTooFar();
            }

            this.index += count;
        }

        public Memory<T> GetMemory(int sizeHint = 0)
        {
            CheckBufferAndEnsureCapacity(sizeHint);

            return this.array.AsMemory(this.index);
        }

        public Span<T> GetSpan(int sizeHint = 0)
        {
            CheckBufferAndEnsureCapacity(sizeHint);

            return this.array.AsSpan(this.index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySegment<T> DangerousGetArray()
        {
            T[]? array = this.array;

            if (array is null)
            {
                ThrowObjectDisposedException();
            }

            return new(array!, 0, this.index);
        }

        public void Dispose()
        {
            T[]? array = this.array;

            if (array is null)
            {
                return;
            }

            this.array = null;

            this.pool.Return(array);
        }

        public override string ToString()
        {
            // See comments in MemoryOwner<T> about this
            if (typeof(T) == typeof(char) &&
                this.array is char[] chars)
            {
                return new(chars, 0, this.index);
            }

            // Same representation used in Span<T>
            return $"CommunityToolkit.HighPerformance.Buffers.ArrayPoolBufferWriter<{typeof(T)}>[{this.index}]";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckBufferAndEnsureCapacity(int sizeHint)
        {
            T[]? array = this.array;

            if (array is null)
            {
                ThrowObjectDisposedException();
            }

            if (sizeHint < 0)
            {
                ThrowArgumentOutOfRangeExceptionForNegativeSizeHint();
            }

            if (sizeHint == 0)
            {
                sizeHint = 1;
            }

            if (sizeHint > array!.Length - this.index)
            {
                ResizeBuffer(sizeHint);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ResizeBuffer(int sizeHint)
        {
            uint minimumSize = (uint)this.index + (uint)sizeHint;

            // The ArrayPool<T> class has a maximum threshold of 1024 * 1024 for the maximum length of
            // pooled arrays, and once this is exceeded it will just allocate a new array every time
            // of exactly the requested size. In that case, we manually round up the requested size to
            // the nearest power of two, to ensure that repeated consecutive writes when the array in
            // use is bigger than that threshold don't end up causing a resize every single time.
            if (minimumSize > 1024 * 1024)
            {
                minimumSize = RoundUpToPowerOf2(minimumSize);
            }

            Resize(ref array, (int)minimumSize);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint RoundUpToPowerOf2(uint value)
        {
            // Based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            --value;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;

            return value + 1;
        }
        private void Resize(ref T[]? array, int newSize, bool clearArray = false)
        {
            // If the old array is null, just create a new one with the requested size
            if (array is null)
            {
                array = pool.Rent(newSize);

                return;
            }

            // If the new size is the same as the current size, do nothing
            if (array.Length == newSize)
            {
                return;
            }

            // Rent a new array with the specified size, and copy as many items from the current array
            // as possible to the new array. This mirrors the behavior of the Array.Resize API from
            // the BCL: if the new size is greater than the length of the current array, copy all the
            // items from the original array into the new one. Otherwise, copy as many items as possible,
            // until the new array is completely filled, and ignore the remaining items in the first array.
            T[] newArray = pool.Rent(newSize);
            int itemsToCopy = Math.Min(array.Length, newSize);

            Array.Copy(array, 0, newArray, 0, itemsToCopy);

            pool.Return(array, clearArray);

            array = newArray;
        }
        private static void ThrowArgumentOutOfRangeExceptionForNegativeCount()
        {
            throw new ArgumentOutOfRangeException("count", "The count can't be a negative value.");
        }

        private static void ThrowArgumentOutOfRangeExceptionForNegativeSizeHint()
        {
            throw new ArgumentOutOfRangeException("sizeHint", "The size hint can't be a negative value.");
        }

        private static void ThrowArgumentExceptionForAdvancedTooFar()
        {
            throw new ArgumentException("The buffer writer has advanced too far.");
        }

        private static void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException("The current buffer has already been disposed.");
        }

    }
}
