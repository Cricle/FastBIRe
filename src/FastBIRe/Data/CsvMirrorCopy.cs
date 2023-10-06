using System.Data;

namespace FastBIRe.Data
{
    public class CsvMirrorCopy : UndefinedDataMirrorCopy<string, RowWriteResult<string>, StreamWriter>
    {
        public CsvMirrorCopy(IDataReader dataReader, StreamWriter streamWriter)
            : base(dataReader)
        {
            StreamWriter = streamWriter ?? throw new ArgumentNullException(nameof(streamWriter));
        }

        public CsvMirrorCopy(IDataReader dataReader, int batchSize, StreamWriter streamWriter)
            : base(dataReader, batchSize)
        {
            StreamWriter = streamWriter ?? throw new ArgumentNullException(nameof(streamWriter));
        }

        public StreamWriter StreamWriter { get; }

        protected override StreamWriter CreateInput()
        {
            return StreamWriter;
        }
        private string WrapValue(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            if (value is string str)
            {
                return $"'{str}'";
            }
            if (value is DateTime dt)
            {
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";
            }
            return value.ToString();
        }
        protected override void CloseInput(StreamWriter? input)
        {
            //Do not close input
        }
        protected override void AppendRecord(StreamWriter input, IDataReader reader, bool lastBatch)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                input.Write(WrapValue(reader[i]));
                if (reader.FieldCount - 1 != i)
                {
                    input.Write(",");
                }
            }
            input.WriteLine();
        }

        protected override Task<RowWriteResult<string>> WriteAsync(StreamWriter datas, bool storeWriteResult, bool unbound, CancellationToken token)
        {
            return RowWriteResult<string>.AsyncEmpty;
        }
    }
}
