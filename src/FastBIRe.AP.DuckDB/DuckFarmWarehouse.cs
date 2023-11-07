using DatabaseSchemaReader;
using DuckDB.NET.Data;
using FastBIRe.Farm;
using System.Data;
using System.Runtime.CompilerServices;

namespace FastBIRe.AP.DuckDB
{
    public class DuckFarmWarehouse : FarmWarehouse
    {
        private readonly DuckDBConnection duckDBConnection;
        public DuckFarmWarehouse(IDbScriptExecuter scriptExecuter)
            : base(scriptExecuter)
        {
            duckDBConnection = (DuckDBConnection)scriptExecuter.Connection;
        }

        public DuckFarmWarehouse(IDbScriptExecuter scriptExecuter, DatabaseReader databaseReader)
            : base(scriptExecuter, databaseReader)
        {
            duckDBConnection = (DuckDBConnection)scriptExecuter.Connection;
        }
        public override Task<int> InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<IEnumerable<object?>> values, CancellationToken token = default)
        {
            var count = 0;
            using (var appender = duckDBConnection.CreateAppender(tableName))
            {
                foreach (var row in values)
                {
                    var rowAppender = appender.CreateRow();
                    foreach (var item in row)
                    {
                        Add(rowAppender, item);
                    }
                    rowAppender.EndRow();
                    count++;
                }
            }
            return Task.FromResult(count);
        }
        public override Task InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<object?> values, CancellationToken token = default)
        {
            using (var appender = duckDBConnection.CreateAppender(tableName))
            {
                var row = appender.CreateRow();
                foreach (var item in values)
                {
                    Add(row, item);
                }
                row.EndRow();
            }
            return Task.CompletedTask;
        }
        private static Action<DuckDBAppenderRow, IDataReader>[] BuildFetcher(IDataRecord record)
        {
            var fetcher = new Action<DuckDBAppenderRow, IDataReader>[record.FieldCount];
            for (int i = 0; i < fetcher.Length; i++)
            {
                var index = i;
                var type = record.GetFieldType(i);
                if (type == typeof(bool))
                    fetcher[i] = (row, reader) => row.AppendValue(reader.GetBoolean(index));
                else if (type == typeof(sbyte))
                    fetcher[i] = (row, reader) => row.AppendValue((sbyte)reader.GetByte(index));
                else if (type == typeof(byte))
                    fetcher[i] = (row, reader) => row.AppendValue(reader.GetByte(index));
                else if (type == typeof(short))
                    fetcher[i] = (row, reader) => row.AppendValue(reader.GetInt16(index));
                else if (type == typeof(ushort))
                    fetcher[i] = (row, reader) => row.AppendValue((ushort)reader.GetInt16(index));
                else if (type == typeof(int))
                    fetcher[i] = (row, reader) => row.AppendValue(reader.GetInt32(index));
                else if (type == typeof(uint))
                    fetcher[i] = (row, reader) => row.AppendValue((uint)reader.GetInt32(index));
                else if (type == typeof(long))
                    fetcher[i] = (row, reader) => row.AppendValue(reader.GetInt64(index));
                else if (type == typeof(ulong))
                    fetcher[i] = (row, reader) => row.AppendValue((ulong)reader.GetInt64(index));
                else if (type == typeof(float))
                    fetcher[i] = (row, reader) => row.AppendValue(reader.GetFloat(index));
                else if (type == typeof(double))
                    fetcher[i] = (row, reader) => row.AppendValue(reader.GetDouble(index));
                else if (type == typeof(decimal))
                    fetcher[i] = (row, reader) => row.AppendValue((double)reader.GetDecimal(index));
                else if (type == typeof(DateTime))
                    fetcher[i] = (row, reader) => row.AppendValue(reader.GetDateTime(index));
                else if (type == typeof(string))
                    fetcher[i] = (row, reader) => row.AppendValue(reader.GetString(index));
                else
                    throw new NotSupportedException(type.ToString());
            }
            return fetcher;
        }
        public override Task<int> SyncDataAsync(string tableName, IEnumerable<string> columnNames, IDataReader reader, int batchSize, CancellationToken token = default)
        {
            var res = 0;
            var fetcher = BuildFetcher(reader);
            using (var appender = duckDBConnection.CreateAppender(tableName))
            {
                var fieldCount = reader.FieldCount;
                while (reader.Read())
                {
                    var row = appender.CreateRow();
                    for (int i = 0; i < fieldCount; i++)
                    {
                        fetcher[i](row, reader);
                    }
                    row.EndRow();
                    res++;
                }
            }
            return Task.FromResult(res);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Add(DuckDBAppenderRow row, object? item)
        {
            switch (item)
            {
                case null: row.AppendNullValue(); break;
                case bool: row.AppendValue((bool)item); break;
                case string: row.AppendValue((string)item); break;
                case sbyte: row.AppendValue((sbyte)item); break;
                case short: row.AppendValue((short)item); break;
                case int: row.AppendValue((int)item); break;
                case long: row.AppendValue((long)item); break;
                case byte: row.AppendValue((byte)item); break;
                case ushort: row.AppendValue((ushort)item); break;
                case uint: row.AppendValue((uint)item); break;
                case ulong: row.AppendValue((ulong)item); break;
                case float: row.AppendValue((float)item); break;
                case double: row.AppendValue((double)item); break;
                case decimal: row.AppendValue((double)(decimal)item); break;
                case DateTime: row.AppendValue(((DateTime)item)); break;
#if NET6_0_OR_GREATER
                case DateOnly: row.AppendValue((DateOnly)item); break;
                case TimeOnly: row.AppendValue((TimeOnly)item); break;
#endif
                default:
                    break;
            }
        }

    }
}