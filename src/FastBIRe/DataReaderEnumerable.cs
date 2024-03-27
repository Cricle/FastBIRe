using System.Collections;
using System.Data;
using System.Diagnostics;

namespace FastBIRe
{
    public readonly struct DataReaderEnumerable<T> : IEnumerable<T>
    {
        public DataReaderEnumerable(IDataReader dataReader)
        {
            DataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
        }

        public IDataReader DataReader { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return new DataReaderEnumerator(DataReader);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        private readonly struct DataReaderEnumerator : IEnumerator<T?>
        {
            private readonly IDataReader dataReader;

            public DataReaderEnumerator(IDataReader dataReader)
            {
                Debug.Assert(dataReader != null);
                this.dataReader = dataReader!;
            }

            public T? Current => RecordToObjectManager<T>.To(dataReader);

            object? IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return dataReader.Read();
            }

            public void Reset()
            {
            }
        }
    }
}
