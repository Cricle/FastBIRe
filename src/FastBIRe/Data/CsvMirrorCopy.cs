using System.Data;
using System.Text;

namespace FastBIRe.Data
{
    public class CsvMirrorCopy : UndefinedDataMirrorCopy<string, RowWriteResult<string>, StreamWriter>, IDisposable
    {
        public static CsvMirrorCopy FromFile(IDataReader dataReader, string path, Encoding? encoding = null, FileMode fileMode = FileMode.Create, FileAccess fileAccess = FileAccess.ReadWrite, FileShare fileShare = FileShare.Read)
        {
            var fs = File.Open(path, fileMode, fileAccess, fileShare);
            encoding ??= Encoding.UTF8;
            return new CsvMirrorCopy(dataReader, new StreamWriter(fs, encoding));
        }

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
        private string? WrapValue(object value)
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

        public void Dispose()
        {
            StreamWriter.Dispose();
        }
    }
}
