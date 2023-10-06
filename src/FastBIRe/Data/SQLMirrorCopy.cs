using FastBIRe.Wrapping;
using System.Data;
using System.Text;

namespace FastBIRe.Data
{
    public class SQLMirrorCopy : UndefinedDataMirrorCopy<string, RowWriteResult<string>, StringBuilder>
    {
        public SQLMirrorCopy(IDataReader dataReader, SQLMirrorTarget target, IEscaper escaper)
            : base(dataReader)
        {
            Target = target;
            Escaper = escaper ?? throw new ArgumentNullException(nameof(escaper));
        }

        public SQLMirrorCopy(IDataReader dataReader, SQLMirrorTarget target, IEscaper escaper, int batchSize)
            : base(dataReader, batchSize)
        {
            Target = target;
            Escaper = escaper ?? throw new ArgumentNullException(nameof(escaper));
        }

        public SQLMirrorTarget Target { get; }

        public IEscaper Escaper { get; }

        private string[]? names;

        public IReadOnlyList<string>? Names => names;

        public int CommandTimeout { get; set; } = 60;

        protected override Task OnFirstReadAsync()
        {
            names = new string[DataReader.FieldCount];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = DataReader.GetName(i);
            }
            return base.OnFirstReadAsync();
        }

        protected virtual string GetInsertHeader()
        {
            return $"INSERT INTO {Target.Named} VALUES ";
        }

        protected virtual string? CompileInsertScript(IEnumerable<IEnumerable<object?>> datas)
        {
            if (!datas.Any())
            {
                return null;
            }
            var scriptBuilder = new StringBuilder(GetInsertHeader());
            var count = datas.Count();
            foreach (var item in datas)
            {
                scriptBuilder.Append('(');
                scriptBuilder.Append(string.Join(",", item.Select(x => Escaper.WrapValue(x))));
                scriptBuilder.Append(')');
                count--;
                if (count > 0)
                {
                    scriptBuilder.Append(',');
                }
            }
            return scriptBuilder.ToString();
        }

        protected override StringBuilder CreateInput()
        {
            return new StringBuilder(GetInsertHeader());
        }

        protected override void AppendRecord(StringBuilder input, IDataReader reader, bool lastBatch)
        {
            input.Append('(');
            for (int i = 0; i < reader.FieldCount; i++)
            {
                input.Append(Escaper.WrapValue(reader[i]));
                if (reader.FieldCount-1 != i)
                {
                    input.Append(',');
                }
            }
            input.Append(')');
            if (!lastBatch)
            {
                input.Append(',');
            }
        }

        protected override async Task<RowWriteResult<string>> WriteAsync(StringBuilder datas, bool storeWriteResult,bool unbound, CancellationToken token)
        {
            if (datas.Length == 0)
            {
                return RowWriteResult<string>.Empty;
            }
            if (unbound)
            {
                datas.Remove(datas.Length - 1, 1);
            }
            var sciprt = datas.ToString();
            var affect = await Target.ScriptExecuter.ExecuteAsync(sciprt, token);
            return new RowWriteResult<string>(
                names,
                new IQueryTranslateResult[] { new QueryTranslateResult(sciprt) },
                affect);
        }
    }
}
