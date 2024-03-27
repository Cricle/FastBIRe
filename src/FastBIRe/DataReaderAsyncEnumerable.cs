using System.Data;

namespace FastBIRe
{
    public readonly struct DataReaderAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        public DataReaderAsyncEnumerable(IDataReader dataReader)
        {
            DataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
        }

        public IDataReader DataReader { get; }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new DataReaderAsyncEnumerator(DataReader);
        }
        private readonly struct DataReaderAsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly IDataReader dataReader;

            public DataReaderAsyncEnumerator(IDataReader dataReader)
            {
                this.dataReader = dataReader;
            }

            public T Current => throw new NotImplementedException();

            public ValueTask DisposeAsync()
            {
                return default;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(dataReader.Read());
            }
        }
    }
}
