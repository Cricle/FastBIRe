using System.Collections;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public struct OneEnumerable<T> : IEnumerable<T>, IEnumerator<T>
    {
        private readonly T value;
        private bool isFirst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OneEnumerable(T value)
        {
            this.value = value;
            Reset();
        }

        public readonly T Current => value;

        readonly object? IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (isFirst)
            {
                isFirst = false;
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            isFirst = true;
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
