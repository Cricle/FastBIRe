using Diagnostics.Generator.Core;
using Diagnostics.Traces.Status;
using DuckDB.NET.Data;
using DuckDB.NET.Native;
using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.DuckDB.Status
{
    public class DuckDBStatusManager : StatusManagerBase, IOpetatorHandler<string>, IBatchOperatorHandler<string>
    {
        public static readonly TimeSpan DefaultVacuumTime = TimeSpan.FromMinutes(1);

        private readonly ConcurrentDictionary<string, DuckDBPrepare> prepares = new ConcurrentDictionary<string, DuckDBPrepare>();
        private readonly BufferOperator<string> bufferOperator;
        private readonly DuckDBNativeConnection nativeConnection;
        private readonly TimerHandler? vacuumTimerHandler;

        public DuckDBStatusManager(DuckDBConnection connection, TimeSpan? vacuumDelayTime, StatusRemoveMode removeMode= StatusRemoveMode.DropSucceed)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            bufferOperator = new BufferOperator<string>(this, false, false);
            nativeConnection = DuckDBNativeHelper.GetNativeConnection(connection);
            RemoveMode = removeMode;
            if (vacuumDelayTime != null)
            {
                vacuumTimerHandler = new TimerHandler(vacuumDelayTime.Value, VacuumHandle);
            }
        }

        public TimeSpan? VacuumDelayTime { get; }

        public DuckDBConnection Connection { get; }

        public int UnComplateSqlCount => bufferOperator.UnComplatedCount;

        public StatusRemoveMode RemoveMode { get; }

        public bool WithCheckpoint { get; set; }

        public event EventHandler<BufferOperatorExceptionEventArgs<string>>? ExceptionRaised
        {
            add { bufferOperator.ExceptionRaised += value; }
            remove { bufferOperator.ExceptionRaised -= value; }
        }

        private void VacuumHandle()
        {
            if (WithCheckpoint)
            {
                bufferOperator.Add("CHECKPOINT;");
            }
            bufferOperator.Add("VACUUM;");
        }

        private string CreateTableSql(string tableName)
        {
            return $"""
                CREATE TABLE IF NOT EXISTS "{tableName}"(
                time DATETIME NOT NULL,
                nowStatus VARCHAR,
                logs MAP(TIMESTAMP,VARCHAR),
                status MAP(TIMESTAMP,VARCHAR),
                complatedTime TIMESTAMP,
                complateStatus TINYINT
                );
                CREATE INDEX IF NOT EXISTS "IX_{tableName}_TIME" ON "{tableName}" ("time") ;
                """;
        }
        // $"INSERT INTO \"{name}\" VALUES('{scopeName}',NULL,MAP {{}},MAP {{}},NULL,NULL);";
        //        CREATE INDEX IF NOT EXISTS "IX_{tableName}_TIME" ON "{tableName}" ("time") ;
        private static StatusInfo ReadStautsInfo(IDataRecord record)
        {
            return new StatusInfo(record.GetDateTime(0),
                record.IsDBNull(1) ? null : record.GetString(1),
                ReadTimePairs(record[2]),
                ReadTimePairs(record[3]),
                record.IsDBNull(4) ? null : record.GetDateTime(4),
                record.IsDBNull(5) ? null : (StatusTypes)record.GetByte(5));
        }
        private static List<TimePairValue> ReadTimePairs(object? value)
        {
            if (value is Dictionary<DateTime,string> map)
            {
                var lst = new List<TimePairValue>();
                foreach (var item in map)
                {
                    lst.Add(new TimePairValue(item.Key, item.Value));
                }
                return lst;
            }
            return new List<TimePairValue>(0);
        }
        public override long? Count(string name)
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = $"SELECT COUNT(*) FROM \"{name}\";";
                using (var reader = comm.ExecuteReader())
                {
                    var tables = new List<string>();
                    if (reader.Read())
                    {
                        return reader.GetInt64(0);
                    }
                }
            }
            return null;
        }

        public override IReadOnlyList<string> GetNames()
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = "SHOW TABLES;";
                using (var reader = comm.ExecuteReader())
                {
                    var tables = new List<string>();
                    while (reader.Read())
                    {
                        tables.Add(reader.GetString(0));
                    }
                    return tables;
                }
            }
        }
        public override void Dispose()
        {
            try
            {
                foreach (var item in prepares)
                {
                    item.Value.Dispose();
                }
                vacuumTimerHandler?.Dispose();
                prepares.Clear();
                bufferOperator.Dispose();

                Connection.Dispose();
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
            }
        }

        public override Task<long> CleanBeforeAsync(string name, DateTime time, CancellationToken token = default)
        {
            bufferOperator.Add($"DELETE FROM \"{name}\" WHERE \"ts\" <= '{time:yyyy-MM-dd HH:mm:ss}';");
            return Task.FromResult(-1L);
        }

        public override Task<long> CleanAsync(string name, CancellationToken token = default)
        {
            bufferOperator.Add($"DELETE FROM \"{name}\";");
            return Task.FromResult(-1L);
        }

        private string CreateQuerySql(string name, DateTime? leftTime, DateTime? rightTime)
        {
            var sql = $"SELECT * FROM \"{name}\" ";
            if (leftTime != null && rightTime != null)
            {
                sql += "WHERE ";
            }

            if (leftTime != null)
            {
                sql += $" \"time\" >= {leftTime.Value:yyyy-MM-dd HH:mm:ss}";
            }
            if (rightTime != null)
            {
                if (leftTime != null)
                {
                    sql += " AND ";
                }
                sql += $" \"time\" >= {rightTime.Value:yyyy-MM-dd HH:mm:ss}";
            }
            return sql;
        }
        private string CreateQuerySql(string name, string key)
        {
            return $"SELECT * FROM \"{name}\" WHERE \"time\" = '{key}';";
        }

        public override async IAsyncEnumerable<StatusInfo> FindAsync(string name, DateTime? leftTime = null, DateTime? rightTime = null,[EnumeratorCancellation] CancellationToken token = default)
        {
            var sql = CreateQuerySql(name, leftTime, rightTime);

            using (var comm=Connection.CreateCommand())
            {
                comm.CommandText = sql;
                using (var reader=await comm.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        yield return ReadStautsInfo(reader);
                    }
                }
            }
        }

        public override async Task<StatusInfo?> FindAsync(string name, string key, CancellationToken token = default)
        {
            var sql = CreateQuerySql(name, key);

            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = sql;
                using (var reader = await comm.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return ReadStautsInfo(reader);
                    }
                }
            }
            return null;
        }

        public override IEnumerable<StatusInfo> Find(string name, DateTime? leftTime=null, DateTime? rightTime = null)
        {
            var sql = CreateQuerySql(name, leftTime, rightTime);

            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = sql;
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return ReadStautsInfo(reader);
                    }
                }
            }
        }

        public override StatusInfo? Find(string name, string key)
        {
            var sql = CreateQuerySql(name, key);

            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = sql;
                using (var reader = comm.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadStautsInfo(reader);
                    }
                }
            }
            return null;
        }

        private DuckDBPrepare GetPrepare(string name)
        {
            return prepares.GetOrAdd(name, (k) =>
            {
                var pre = new DuckDBPrepare(Connection, k, bufferOperator, RemoveMode);
                pre.Prepare();
                return pre;
            });
        }

        public override IStatusScope CreateScope(string name)
        {
            var time = DateTime.Now;
            var scopeName = time.ToString("yyyy-MM-dd HH:mm:ss.ffff");
            var prepare = GetPrepare(name);

            prepare.Insert(scopeName);

            return new DuckDBStatusScope(Connection, scopeName, name, prepare);
        }

        public override bool Initialize(string name)
        {
            var sql = CreateTableSql(name);
            var res = Connection.ExecuteNoQuery(sql);
            _ = GetPrepare(name);
            return res != 0;
        }

        Task IOpetatorHandler<string>.HandleAsync(string input, CancellationToken token)
        {
            DuckDBNativeHelper.DuckDBQuery(nativeConnection,input);
            return Task.CompletedTask;
        }

        Task IBatchOperatorHandler<string>.HandleAsync(BatchData<string> inputs, CancellationToken token)
        {
            foreach (var item in inputs)
            {
                DuckDBNativeHelper.DuckDBQuery(nativeConnection, item);
            }
            return Task.CompletedTask;
        }
    }
}
