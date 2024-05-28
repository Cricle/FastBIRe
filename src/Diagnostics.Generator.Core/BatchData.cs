using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Diagnostics.Generator.Core
{
    public readonly struct BatchData<T> : IEnumerable<T>, IDisposable
    {
        internal readonly T[] datas;
        internal readonly int count;

        internal BatchData(T[] datas, int count)
        {
            this.datas = datas;
            this.count = count;
        }

        public Span<T> Datas => datas.AsSpan(0, count);

        public int Count => count;

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(datas);
        }

        public T[] DangerousGetDatas()
        {
            return datas;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return datas.Take(count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
