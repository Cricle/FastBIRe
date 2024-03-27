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

        public IEnumerable<object?[]> EnumerableRows(string text)
        {
            var reader = new StringReader(text);
            return EnumerableRows(reader);
        }
        public IEnumerable<object?[]> EnumerableRows(TextReader reader)
        {
            var buffer = new object?[Schema.Names.Count];
            var line = reader.ReadLine();
            var types = Schema.Types;
            var typeCodes = Schema.TypeCodes;
            var parser = new Parser(buffer, types, typeCodes);
            while (!string.IsNullOrEmpty(line))
            {
                parser.ParseRow(line);
                yield return buffer;
                line = reader.ReadLine();
            }
        }
        class Parser
        {
            private readonly object?[] Buffer;

            private readonly IReadOnlyList<Type> Types;

            private readonly IReadOnlyList<TypeCode> TypeCodes;

            private static readonly Type byteArrayType = typeof(byte[]);

            public Parser(object?[] buffer, IReadOnlyList<Type> types, IReadOnlyList<TypeCode> typeCodes)
            {
                Buffer = buffer;
                Types = types;
                TypeCodes = typeCodes;
            }

            public void ParseRow(string row)
            {
                var index = 0;
                var start = 0;
                var inQuto = false;
                var len = row.Length;
                for (int i = 0; i < len; i++)
                {
                    var c = row[i];
                    if (c == '\"')
                    {
                        inQuto = !inQuto;
                    }
                    if (c == ',' && !inQuto)
                    {
                        ReadOnlySpan<char> value = default;
                        if (i != start)
                        {
                            value = row.AsSpan(start, i - start);
                        }
                        value = value.Trim();
                        if (value.IsEmpty)
                        {
                            Buffer[index++] = null;
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
                            Buffer[index++] = ToValue(value, TypeCodes[index], Types[index]);
                        }
                        start = i + 1;
                    }
                }
                if (start != len)
                {
                    Buffer[Buffer.Length - 1] = ToValue(row.AsSpan(start), TypeCodes[index], Types[index]);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static object? ToValue(ReadOnlySpan<char> value, TypeCode typeCode, Type type)
            {
#if NETSTANDARD2_0
                if (typeCode == TypeCode.String)
                {
                    return value.ToString();
                }
                if (typeCode == TypeCode.Object && type == byteArrayType)
                {
                    return Convert.FromBase64String(value.Slice(2).ToString());
                }
                return Convert.ChangeType(value.ToString(), typeCode);
#else
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        return bool.Parse(value);
                    case TypeCode.Char:
                        return value[0];
                    case TypeCode.SByte:
                        return sbyte.Parse(value);
                    case TypeCode.Byte:
                        return byte.Parse(value);
                    case TypeCode.Int16:
                        return short.Parse(value);
                    case TypeCode.UInt16:
                        return ushort.Parse(value);
                    case TypeCode.Int32:
                        return int.Parse(value);
                    case TypeCode.UInt32:
                        return uint.Parse(value);
                    case TypeCode.Int64:
                        return long.Parse(value);
                    case TypeCode.UInt64:
                        return ulong.Parse(value);
                    case TypeCode.Single:
                        return float.Parse(value);
                    case TypeCode.Double:
                        return double.Parse(value);
                    case TypeCode.Decimal:
                        return decimal.Parse(value);
                    case TypeCode.DateTime:
                        return DateTime.Parse(value);
                    case TypeCode.String:
                        return value.ToString();
                    case TypeCode.Object:
                        if (type == byteArrayType)
                        {
                            return Convert.FromBase64String(value.Slice(2).ToString());
                        }
                        return null;
                    default:
                        return null;
                }
#endif
            }
        }
    }
}