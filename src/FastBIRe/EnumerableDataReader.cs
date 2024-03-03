using System.Data;
#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public readonly struct EnumerableDataReader<T> : IDataReader
    {
        public object this[int i] => GetValue(i);

        public object this[string name] => GetValue(GetOrdinal(name));

        public int Depth => 0;

        public bool IsClosed => false;

        public int RecordsAffected => 0;

        public int FieldCount => Schema.Names.Count;

        private readonly IEnumerator<IReadOnlyList<T>> enumerable;

        private IReadOnlyList<T>? Values => enumerable.Current;

        public EnumerableDataReader(IEnumerator<IReadOnlyList<T>> enumerable, DataSchema schema)
        {
            this.enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
            Schema = schema;
        }

        public DataSchema Schema { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfValuesNull()
        {
            if (Values == null)
            {
                throw new InvalidOperationException("Must use Read");
            }
        }

        public void Close()
        {
            enumerable.Dispose();
        }

        public void Dispose()
        {
            Close();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TInst? GetOrCase<TInst>(int i)
        {
            ThrowIfValuesNull();
            var val = Values![i];
            if (val == null)
            {
                return default;
            }
            if (val is TInst tinst)
            {
                return tinst;
            }
            if (typeof(TInst) == typeof(Guid))
            {
                Guid guid = Guid.Parse(val.ToString()!);
                return Unsafe.As<Guid, TInst?>(ref guid);
            }
            return (TInst)Convert.ChangeType(val, typeof(TInst))!;
        }
        public bool GetBoolean(int i)
        {
            return GetOrCase<bool>(i);
        }

        public byte GetByte(int i)
        {
            return GetOrCase<byte>(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return GetOrCase<char>(i);
        }

        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            return Schema.Types[i].FullName!;
        }

        public DateTime GetDateTime(int i)
        {
            return GetOrCase<DateTime>(i);
        }

        public decimal GetDecimal(int i)
        {
            return GetOrCase<decimal>(i);
        }

        public double GetDouble(int i)
        {
            return GetOrCase<double>(i);
        }
#if NET6_0_OR_GREATER
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        public Type GetFieldType(int i)
        {
            return Schema.Types[i];
        }

        public float GetFloat(int i)
        {
            return GetOrCase<float>(i);
        }

        public Guid GetGuid(int i)
        {
            return GetOrCase<Guid>(i);
        }

        public short GetInt16(int i)
        {
            return GetOrCase<short>(i);
        }

        public int GetInt32(int i)
        {
            return GetOrCase<int>(i);
        }

        public long GetInt64(int i)
        {
            return GetOrCase<long>(i);
        }

        public string GetName(int i)
        {
            return Schema.Names[i];
        }

        public int GetOrdinal(string name)
        {
            for (int i = 0; i < Schema.Names.Count; i++)
            {
                if (Schema.Names[i] == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public DataTable? GetSchemaTable()
        {
            return null;
        }

        public string GetString(int i)
        {
            ThrowIfValuesNull();
            return Values![i]?.ToString()!;
        }

        public object GetValue(int i)
        {
            return Values![i]!;
        }

        public int GetValues(object?[] values)
        {
            ThrowIfValuesNull();
            var copyCount = Math.Min(values.Length, this.Values!.Count);
            for (int i = 0; i < copyCount; i++)
            {
                values[i] = this.Values[i];
            }
            return copyCount;
        }

        public bool IsDBNull(int i)
        {
            ThrowIfValuesNull();
            var val = Values![i];
            return val == null ||DBNull.Value.Equals(val);
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            if (enumerable.MoveNext())
            {
                return true;
            }
            return false;
        }
    }
}
