using Diagnostics.Traces.Stores;
using ParquetSharp;
using ParquetSharp.IO;

namespace Diagnostics.Traces.Parquet
{
    public readonly struct ParquetBox<T> : IDisposable
        where T:IDisposable
    {
        internal ParquetBox(T @operator, Stream stream)
        {
            Operator = @operator;
            Stream = stream;
        }

        public T Operator { get; }

        public Stream Stream { get; }

        public void Dispose()
        {
            Operator.Dispose();
            Stream.Dispose();
        }
    }
    public class ParquetDatabaseCreatedResult : DatabaseCreatedResultBase
    {
        public ParquetDatabaseCreatedResult(string filePath, string key, Column[] columns, Compression compression = Compression.Snappy)
            : base(filePath, key)
        {
            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            this.columns = columns;
            Compression = compression;
        }

        private readonly Column[] columns;

        public Compression Compression { get; }

        public ParquetBox<ParquetFileWriter> GetWriter()
        {
            var fs = File.Open(FilePath!, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            return new ParquetBox<ParquetFileWriter>(new ParquetFileWriter(new ManagedOutputStream(fs), columns, Compression), fs);
        }

        public ParquetBox<ParquetFileReader> GetReader()
        {
            var fs = File.Open(FilePath!, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new ParquetBox<ParquetFileReader>(new ParquetFileReader(new ManagedRandomAccessFile(fs)), fs);
        }
    }
}
