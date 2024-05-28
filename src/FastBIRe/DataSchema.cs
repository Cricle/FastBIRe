using System.Buffers;
using System.Data;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public readonly record struct DataSchema
    {
        public DataSchema(IReadOnlyList<string> names, IReadOnlyList<Type> types)
        {
            Names = names;
            Types = types;
            var typeCodes = new TypeCode[types.Count];
            for (int i = 0; i < typeCodes.Length; i++)
            {
                typeCodes[i] = Convert.GetTypeCode(types[i]);
            }
            TypeCodes = typeCodes;
        }

        public IReadOnlyList<string> Names { get; }

        public IReadOnlyList<Type> Types { get; }

        public IReadOnlyList<TypeCode> TypeCodes { get; }
    }
    public static class DataReaderDynamicExtensions
    {
        public static DataSchema GetDataSchema(this IDataReader reader)
        {
            var names = new string[reader.FieldCount];
            var types = new Type[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                names[i]= reader.GetName(i);
                types[i] = reader.GetFieldType(i);
            }
            return new DataSchema(names, types);
        }
        public static DataSchemaDataTable ToSchemaTable(this IDataReader reader)
        {
            var schema = GetDataSchema(reader);
            return DataSchemaDataTable.FromReader(schema, reader);
        }
    }
    public class DataSchemaDataTable : IDisposable, IDataSchemaDataTable
    {
        public DataSchema DataSchema { get; }

        private object?[]? rowBuffer;
        private object?[]? _arrayFromPool;
        private int _pos;

        public int RowCount
        {
            get
            {
                if (_pos == 0 || DataSchema.Names.Count == 0)
                {
                    return 0;
                }
                return (int)Math.Ceiling(_pos / (double)DataSchema.Names.Count);
            }
        }

        public int Count => _pos;

        public object? this[int index]
        {
            get
            {
                if (_arrayFromPool == null||index >= _pos)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return _arrayFromPool[_pos];
            }
        }
        public object? this[int row,int column]
        {
            get
            {
                var start = row * DataSchema.Names.Count;
                if (_arrayFromPool == null || (start + column) >= _pos)
                {
                    throw new ArgumentOutOfRangeException(nameof(row) + " " + nameof(column));
                }
                return _arrayFromPool[start + column];
            }
        }

        public DataSchemaDataTable(DataSchema dataSchema)
        {
            DataSchema = dataSchema;
        }

        public static DataSchemaDataTable FromReader(DataSchema dataSchema, IDataReader reader)
        {
            var table = new DataSchemaDataTable(dataSchema);
            table.AppendTable(reader);
            return table;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object?[] GetRowBuffer()
        {
            if (rowBuffer == null)
            {
                rowBuffer = new object?[DataSchema.Names.Count];
            }
            return rowBuffer;
        }

        public Span<object?> GetRow(int index)
        {
            var actualIndex = index * DataSchema.Names.Count;
            if (actualIndex >= _pos)
            {
                throw new ArgumentOutOfRangeException($"The row {index} do not exists");
            }
            return _arrayFromPool.AsSpan(actualIndex, DataSchema.Names.Count);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(object? item)
        {
            int pos = _pos;

            // Workaround for https://github.com/dotnet/runtime/issues/72004
            if (_arrayFromPool != null && (uint)pos < (uint)_arrayFromPool.Length)
            {
                _arrayFromPool[pos] = item;
                _pos = pos + 1;
            }
            else
            {
                AddWithResize(item);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(scoped ReadOnlySpan<object?> source)
        {
            int pos = _pos;
            if (_arrayFromPool != null && source.Length == 1 && (uint)pos < (uint)_arrayFromPool.Length)
            {
                _arrayFromPool[pos] = source[0];
                _pos = pos + 1;
            }
            else
            {
                AppendMultiChar(source);
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AppendMultiChar(scoped ReadOnlySpan<object?> source)
        {
            if (_arrayFromPool == null || ((uint)(_pos + source.Length) > (uint)_arrayFromPool.Length))
            {
                Grow(_arrayFromPool?.Length ?? 0 - _pos + source.Length);
            }

            source.CopyTo(_arrayFromPool.AsSpan(_pos));
            _pos += source.Length;
        }
        // Hide uncommon path
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddWithResize(object? item)
        {
            int pos = _pos;
            Grow(1);
            _arrayFromPool![pos] = item;
            _pos = pos + 1;
        }

        // Note that consuming implementations depend on the list only growing if it's absolutely
        // required.  If the list is already large enough to hold the additional items be added,
        // it must not grow. The list is used in a number of places where the reference is checked
        // and it's expected to match the initial reference provided to the constructor if that
        // span was sufficiently large.
        private void Grow(int additionalCapacityRequired = 1)
        {
            const int ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

            var length = _arrayFromPool == null ? 0 : _arrayFromPool.Length;

            // Double the size of the span.  If it's currently empty, default to size 4,
            // although it'll be increased in Rent to the pool's minimum bucket size.
            int nextCapacity = Math.Max(length != 0 ? length * 2 : DataSchema.Names.Count, length + additionalCapacityRequired);

            // If the computed doubled capacity exceeds the possible length of an array, then we
            // want to downgrade to either the maximum array length if that's large enough to hold
            // an additional item, or the current length + 1 if it's larger than the max length, in
            // which case it'll result in an OOM when calling Rent below.  In the exceedingly rare
            // case where _span.Length is already int.MaxValue (in which case it couldn't be a managed
            // array), just use that same value again and let it OOM in Rent as well.
            if ((uint)nextCapacity > ArrayMaxLength)
            {
                nextCapacity = Math.Max(Math.Max(length + 1, ArrayMaxLength), length);
            }

            object?[] array = ArrayPool<object?>.Shared.Rent(nextCapacity);
            if (_arrayFromPool != null)
            {
                _arrayFromPool.AsSpan(_pos).CopyTo(array.AsSpan());
            }

            object?[]? toReturn = _arrayFromPool;
            _arrayFromPool = array;
            if (toReturn != null)
            {
                ArrayPool<object?>.Shared.Return(toReturn);
            }
        }

        public void Dispose()
        {
            object?[]? toReturn = _arrayFromPool;
            if (toReturn != null)
            {
                _arrayFromPool = null;
                ArrayPool<object?>.Shared.Return(toReturn);
            }
        }
    }
}
