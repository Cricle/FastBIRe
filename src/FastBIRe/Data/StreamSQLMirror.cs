using FastBIRe.Wrapping;
using System.Data;
using System.Text;

namespace FastBIRe.Data
{
    public class StreamSQLMirror : SQLMirrorCopy
    {
        public StreamSQLMirror(IDataReader dataReader, SQLMirrorTarget target, IEscaper escaper, StreamWriter stream) 
            : base(dataReader, target, escaper)
        {
            StreamWriter= stream;
        }

        public StreamSQLMirror(IDataReader dataReader, SQLMirrorTarget target, IEscaper escaper, StreamWriter stream, int batchSize)
            : base(dataReader, target, escaper, batchSize)
        {
            StreamWriter = stream;
        }

        public StreamWriter StreamWriter { get; }

        protected override async Task<RowWriteResult<string>> WriteAsync(StringBuilder datas, bool storeWriteResult, bool unbound, CancellationToken token)
        {
            if (unbound)
            {
                datas.Remove(datas.Length - 1, 1);
            }
#if NETSTANDARD2_0
            var script = datas.ToString();
            await StreamWriter.WriteLineAsync(script);
#else
                await StreamWriter.WriteLineAsync(datas,token);
#endif
            return RowWriteResult<string>.Empty;
        }
    }
}
