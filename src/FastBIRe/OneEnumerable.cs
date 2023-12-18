using System.Collections;

namespace FastBIRe
{
    internal readonly struct OneEnumerable<T> : IEnumerable<T>
    {
        private readonly T value;

        public OneEnumerable(T value)
        {
            this.value = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(value);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        struct Enumerator : IEnumerator<T>
        {
            private readonly T value;
            private bool first;

            public Enumerator(T value)
            {
                this.value = value;
                this.first = false;
            }

            public T Current => value;

            object? IEnumerator.Current => value;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (first)
                {
                    first = false;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
            }
        }
    }
}
