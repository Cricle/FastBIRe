using Diagnostics.Traces.Stores;
using ParquetSharp;

namespace Diagnostics.Traces.Parquet
{
    public class ParquetDatabaseCreatedResult : DatabaseCreatedResultBase
    {
        ~ParquetDatabaseCreatedResult()
        {
            Close();
        }

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
        private ParquetFileWriter? writer;

        public ParquetFileWriter Writer
        {
            get
            {
                if (writer==null)
                {
                    lock (Root)
                    {
                        if (writer==null)
                        {
                            writer = new ParquetFileWriter(FilePath, columns, Compression);
                        }
                    }
                }
                return writer;
            }
        }

        private readonly Column[] columns;

        public Compression Compression { get; }

        private void Close()
        {
            writer?.Dispose();
        }

        protected override void OnDisposed()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}
