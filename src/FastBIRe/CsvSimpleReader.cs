namespace FastBIRe
{
    public class CsvSimpleReader
    {
        public CsvSimpleReader(DataSchema schema)
        {
            Schema = schema;
        }

        public DataSchema Schema { get; }

        public IEnumerable<IEnumerable<object?>> EnumerableRows(string text)
        {
            var reader = new StringReader(text);
            return EnumerableRows(reader);
        }
        public IEnumerable<IEnumerable<object?>> EnumerableRows(TextReader reader)
        {
            var buffer = new object?[Schema.Names.Count];
            var line = reader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                yield return ParseRow(line, buffer);
                line = reader.ReadLine();
            }
        }
        private object?[] ParseRow(string row, object?[] ret)
        {
            var index = 0;
            var start = 0;
            var inQuto = false;
            for (int i = 0; i < row.Length; i++)
            {
                var c = row[i];
                if (c == '\"')
                {
                    inQuto = !inQuto;
                }
                var value = ReadOnlySpan<char>.Empty;
                if (c == ',' && !inQuto)
                {
                    if (i != start)
                    {
                        value = row.AsSpan(start, i - start);
                    }
                    value = value.Trim();
                    if (value.IsEmpty)
                    {
                        ret[index++] = null;
                    }
                    else
                    {
                        if (value[0] == '\"')
                        {
                            value = value.Slice(1);
                        }
                        if (value[value.Length - 1] == '\"')
                        {
                            value = value.Slice(0, value.Length - 1);
                        }
                        ret[index++] = Convert.ChangeType(value.ToString(), Schema.Types[index]);
                    }
                    start = i + 1;
                }
            }
            if (start != row.Length)
            {
                ret[ret.Length - 1] = Convert.ChangeType(row.Substring(start), Schema.Types[index]);
            }
            return ret;
        }
    }
}
