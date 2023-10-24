using System.Runtime.CompilerServices;

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
                ParseRow(line, buffer);
                yield return buffer;
                line = reader.ReadLine();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseRow(string row, object?[] ret)
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
                        ret[index++] = ToValue(value.ToString(), Schema.Types[index]);
                    }
                    start = i + 1;
                }
            }
            if (start != row.Length)
            {
                ret[ret.Length - 1] = ToValue(row.Substring(start), Schema.Types[index]);
            }
        }
        private static readonly Type byteArrayType = typeof(byte[]);
        private static readonly Type stringType = typeof(string);

        private static object ToValue(string value, Type type)
        {
            if (type == stringType)
            {
                return value;
            }
            if (type == byteArrayType)
            {
                return Convert.FromBase64String(value.Substring(2));
            }
            return Convert.ChangeType(value, type);
        }
    }
}
