using Diagnostics.Generator.Core;
using Diagnostics.Traces.Status;
using Diagnostics.Traces.Stores;
using System.Data;
using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.DuckDB.Status
{
    public class DuckDBStatusManager : StatusManagerBase, IOpetatorHandler<string>, IBatchOperatorHandler<string>
    {
        private static readonly Random random = new Random();
        private readonly BufferOperator<string> bufferOperator;

        public DuckDBStatusManager(IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> databaseSelector, StatusRemoveMode removeMode = StatusRemoveMode.DropSucceed)
        {
            DatabaseSelector = databaseSelector ?? throw new ArgumentNullException(nameof(databaseSelector));
            StatusStorageManager = new DefaultStatusStorageManager();
            bufferOperator = new BufferOperator<string>(this, false, false);
            RemoveMode = removeMode;
        }

        public IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> DatabaseSelector { get; }

        public int UnComplateSqlCount => bufferOperator.UnComplatedCount;

        public StatusRemoveMode RemoveMode { get; }

        public bool WithCheckpoint { get; set; }

        public override IStatusStorageManager StatusStorageManager { get; }

        public event EventHandler<BufferOperatorExceptionEventArgs<string>>? ExceptionRaised
        {
            add { bufferOperator.ExceptionRaised += value; }
            remove { bufferOperator.ExceptionRaised -= value; }
        }

        private string CreateTableSql(string tableName)
        {
            return $"""
                CREATE TABLE IF NOT EXISTS "{tableName}"(
                time DATETIME NOT NULL,
                logs MAP(TIMESTAMP,VARCHAR),
                status MAP(TIMESTAMP,VARCHAR),
                complatedTime TIMESTAMP,
                complateStatus TINYINT
                );
                """;
        }
        private static StatusInfo ReadStautsInfo(IDataRecord record)
        {
            return new StatusInfo(record.GetDateTime(0),
                ReadTimePairs(record[2]),
                ReadTimePairs(record[3]),
                record.IsDBNull(4) ? null : record.GetDateTime(4),
                record.IsDBNull(5) ? null : (StatusTypes)record.GetByte(5));
        }
        private static List<TimePairValue> ReadTimePairs(object? value)
        {
            if (value is Dictionary<DateTime, string> map)
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
            return DatabaseSelector.UsingDatabaseResult<string,long?>(name, static (res, ql) =>
            {
                var tables = new List<string>();
                using (var comm = res.Connection.CreateCommand())
                {
                    comm.CommandText = $"SELECT COUNT(*) FROM \"{ql}\";";
                    using (var reader = comm.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt64(0);
                        }
                    }
                }
                return null;
            });
        }

        public override IReadOnlyList<string> GetNames()
        {
            return DatabaseSelector.UsingDatabaseResult(string.Empty, static (res, ql) =>
            {
                var tables = new List<string>();
                using (var comm = res.Connection.CreateCommand())
                {
                    comm.CommandText = "SHOW TABLES;";
                    using (var reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }
                }
                return tables;
            });
        }
        public override void Dispose()
        {
            try
            {
                bufferOperator.Dispose();

                DatabaseSelector.Dispose();
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

        public override async IAsyncEnumerable<StatusInfo> FindAsync(string name, DateTime? leftTime = null, DateTime? rightTime = null, [EnumeratorCancellation] CancellationToken token = default)
        {
            var sql = CreateQuerySql(name, leftTime, rightTime);

            var res = DatabaseSelector.UsingDatabaseResult(sql,static (res, ql) =>
            {
                var r = new List<StatusInfo>();
                using (var comm = res.Connection.CreateCommand())
                {
                    comm.CommandText = ql;
                    using (var reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            r.Add(ReadStautsInfo(reader));
                        }
                    }
                }
                return r;
            });

            foreach (var item in res)
            {
                yield return item;
            }
        }

        public override Task<StatusInfo?> FindAsync(string name, string key, CancellationToken token = default)
        {
            var sql = CreateQuerySql(name, key);
            return Task.FromResult(DatabaseSelector.UsingDatabaseResult<string, StatusInfo?>(sql, static (res, ql) =>
            {
                using (var comm = res.Connection.CreateCommand())
                {
                    comm.CommandText = ql;
                    using (var reader = comm.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadStautsInfo(reader);
                        }
                    }
                }
                return null;
            }));
        }

        public override IEnumerable<StatusInfo> Find(string name, DateTime? leftTime = null, DateTime? rightTime = null)
        {
            var sql = CreateQuerySql(name, leftTime, rightTime);

            var res = DatabaseSelector.UsingDatabaseResult(sql, static (res, ql) =>
            {
                var r = new List<StatusInfo>();
                using (var comm = res.Connection.CreateCommand())
                {
                    comm.CommandText = ql;
                    using (var reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            r.Add(ReadStautsInfo(reader));
                        }
                    }
                }
                return r;
            });
            return res;
        }

        public override StatusInfo? Find(string name, string key)
        {
            var sql = CreateQuerySql(name, key);

            return DatabaseSelector.UsingDatabaseResult<string, StatusInfo?>(sql, static (res, ql) =>
            {
                using (var comm = res.Connection.CreateCommand())
                {
                    comm.CommandText = ql;
                    using (var reader = comm.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadStautsInfo(reader);
                        }
                    }
                }
                return null;
            });    
        }

        public override IStatusScope CreateScope(string name)
        {
            var time = DateTime.Now;
            var scopeName = time.ToString("yyyy-MM-dd HH:mm:ss.ffff")+ random.Next(1000,9999);

            return new DuckDBStatusScope(scopeName, name,time, RemoveMode, bufferOperator, StatusStorageManager);
        }

        public override bool Initialize(string name)
        {
            var sql = CreateTableSql(name);

            var res = ExecuteSql(sql);
            StatusStorageManager.Add(new InMemoryStatusStorage(name));

            DatabaseSelector.Initializers.Add(new DelegateResultInitializer<DuckDBDatabaseCreatedResult>(r =>
            {
                DuckDBNativeHelper.DuckDBQuery(r.NativeConnection, sql);
            }));
            return res != 0;
        }

        Task IOpetatorHandler<string>.HandleAsync(string input, CancellationToken token)
        {
            ExecuteSql(input);
            DatabaseSelector.ReportInserted(1);
            return Task.CompletedTask;
        }

        Task IBatchOperatorHandler<string>.HandleAsync(BatchData<string> inputs, CancellationToken token)
        {
            foreach (var item in inputs)
            {
                ExecuteSql(item);
            }
            DatabaseSelector.ReportInserted(inputs.Count);
            return Task.CompletedTask;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long ExecuteSql(string sql)
        {
            return DatabaseSelector.UsingDatabaseResult(sql, static (result, ql) =>
            {
                return DuckDBNativeHelper.DuckDBQuery(result.NativeConnection, ql);
            });
        }
    }
}
