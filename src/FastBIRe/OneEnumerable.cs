using System.Collections;

namespace FastBIRe
{
    public struct OneEnumerable<T> : IEnumerable<T>, IEnumerator<T>
    {
        private readonly T value;
        private bool isFirst;

        public OneEnumerable(T value)
        {
            this.value = value;
            Reset();
        }

        public T Current => value;

        object? IEnumerator.Current => Current;

        public void Dispose()
        {

        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            if (isFirst)
            {
                isFirst = false;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            isFirst = true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
