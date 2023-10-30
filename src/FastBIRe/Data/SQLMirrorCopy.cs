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

        protected string[]? names;
        protected string? namesJoined;

        public IReadOnlyList<string>? Names => names;

        public HashSet<string>? IncludeNames { get; set; }

        protected override Task OnFirstReadAsync()
        {
            names = new string[DataReader.FieldCount];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = DataReader.GetName(i);
            }
            namesJoined = string.Join(",", names.Select(x => Escaper.Quto(x)));
            return base.OnFirstReadAsync();
        }
        protected string GetFieldJoinedSlow()
        {
            var names = new StringBuilder();
            for (int i = 0; i < DataReader.FieldCount; i++)
            {
                names.Append(Escaper.Quto(DataReader.GetName(i)));
                if (i != DataReader.FieldCount - 1) 
                {
                    names.Append(',');
                }
            }
            return names.ToString();
        }
        protected virtual string GetInsertHeader()
        {
            var fieldLine = namesJoined;
            if (string.IsNullOrEmpty(fieldLine))
            {
                fieldLine = GetFieldJoinedSlow();
            }
            return $"INSERT INTO {Target.Named}({fieldLine}) VALUES ";
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
            var includeNames = IncludeNames;
            input.Append('(');
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (includeNames != null && names != null && !includeNames.Contains(names[i]))
                {
                    continue;
                }
                input.Append(Escaper.WrapValue(reader[i]));
                if (reader.FieldCount - 1 != i)
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

        protected override async Task<RowWriteResult<string>> WriteAsync(StringBuilder datas, bool storeWriteResult, bool unbound, CancellationToken token)
        {
            if (datas.Length == 0)
            {
                return RowWriteResult<string>.Empty;
            }
            if (unbound && datas[datas.Length - 1] == ',')
            {
                datas.Remove(datas.Length - 1, 1);
            }
            var sciprt = datas.ToString();
            var affect = await Target.ScriptExecuter.ExecuteAsync(sciprt, token: token);
            return new RowWriteResult<string>(
                names,
                new IQueryTranslateResult[] { new QueryTranslateResult(sciprt) },
                affect);
        }
    }
}
