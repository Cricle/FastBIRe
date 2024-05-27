using Diagnostics.Generator.Core;
using Diagnostics.Traces.Status;
using System.Data.Common;

namespace Diagnostics.Traces.DuckDB.Status
{
    internal class DuckDBPrepare : IDisposable
    {
        public DuckDBPrepare(DbConnection connection, string name, BufferOperator<string> bufferOperator, StatusRemoveMode removeMode)
        {
            Connection = connection;
            Name = name;
            LogPrepareName = $"{name}_log";
            SetPrepareName = $"{name}_set";
            ComplatePrepareName = $"{name}_complate";
            InsertPrepareName = $"{name}_insert";
            this.bufferOperator = bufferOperator;
            this.removeMode = removeMode;
        }
        private readonly StatusRemoveMode removeMode;

        private readonly BufferOperator<string> bufferOperator;

        public DbConnection Connection { get; }

        public string Name { get; }

        public string LogPrepareName { get; }

        public string SetPrepareName { get; }

        public string ComplatePrepareName { get; }

        public string InsertPrepareName { get; }

        public void PrepareLog()
        {
            var sql = $"PREPARE \"{LogPrepareName}\" AS UPDATE \"{Name}\" SET \"logs\" = MAP(map_keys(logs)||[?]::DATETIME[],map_values(logs)||[?]) WHERE time = ? AND complateStatus IS NULL;";
            Connection.ExecuteNoQuery(sql);
        }
        public void PrepareSet()
        {
            var sql = $"PREPARE \"{SetPrepareName}\" AS UPDATE \"{Name}\" SET \"nowStatus\"=?,\"status\" = MAP(map_keys(status)||[?]::DATETIME[],map_values(status)||[?]) WHERE time = ? AND complateStatus IS NULL;";
            Connection.ExecuteNoQuery(sql);
        }
        public void PrepareComplate()
        {
            string sql;
            if (removeMode== StatusRemoveMode.DropAll)
            {
                sql = $"PREPARE \"{ComplatePrepareName}\" AS DELETE FROM \"{Name}\" WHERE time = ?";
            }
            else
            {
                sql = $"PREPARE \"{ComplatePrepareName}\" AS UPDATE \"{Name}\" SET \"nowStatus\"=NULL,\"complateStatus\" = ?,complatedTime=CURRENT_TIMESTAMP WHERE time = ? AND complateStatus IS NULL;";
            }
            Connection.ExecuteNoQuery(sql);
        }
        public void PrepareInsert()
        {
            var sql = $"PREPARE \"{InsertPrepareName}\" AS INSERT INTO \"{Name}\" VALUES(?,NULL,MAP {{}},MAP {{}},NULL,NULL);";
            Connection.ExecuteNoQuery(sql);
        }

        public void Prepare()
        {
            PrepareLog();
            PrepareSet();
            PrepareComplate();
            PrepareInsert();
        }

        public int Log(string key, DateTime datetime, string log)
        {
            var sql = $"EXECUTE \"{LogPrepareName}\"('{datetime:yyyy-MM-dd HH:mm:ss.ffff}','{log}','{key}');";
            bufferOperator.Add(sql);
            return -1;
        }
        public int Set(string key, DateTime datetime, string status)
        {
            var sql = $"EXECUTE \"{SetPrepareName}\"('{status}','{datetime:yyyy-MM-dd HH:mm:ss.ffff}','{status}','{key}');";
            bufferOperator.Add(sql);
            return -1;
        }
        public int Complate(string key, StatuTypes complateStatus)
        {
            string sql;
            if (removeMode == StatusRemoveMode.DropAll)
            {
                sql = $"EXECUTE \"{ComplatePrepareName}\"('{key}');";
                bufferOperator.Add(sql);
            }
            else 
            {
                if (removeMode == StatusRemoveMode.DropSucceed)
                {
                    if (complateStatus != StatuTypes.Fail && complateStatus != StatuTypes.Interrupt)
                    {
                        sql = $"DELETE FROM  \"{Name}\" WHERE \"time\" = '{key}'";
                        bufferOperator.Add(sql);
                        return -1;
                    }
                }
                sql = $"EXECUTE \"{ComplatePrepareName}\"({(int)complateStatus},'{key}');";
                bufferOperator.Add(sql);
            }
            return -1;
        }
        public int Insert(string key)
        {
            var sql = $"EXECUTE \"{InsertPrepareName}\"('{key}');";
            bufferOperator.Add(sql);
            return -1;
        }

        public void Dispose()
        {
            Connection.ExecuteNoQuery($"DEALLOCATE PREPARE \"{LogPrepareName}\";");
            Connection.ExecuteNoQuery($"DEALLOCATE PREPARE \"{SetPrepareName}\";");
            Connection.ExecuteNoQuery($"DEALLOCATE PREPARE \"{ComplatePrepareName}\";");
            Connection.ExecuteNoQuery($"DEALLOCATE PREPARE \"{InsertPrepareName}\";");
        }
    }
}
