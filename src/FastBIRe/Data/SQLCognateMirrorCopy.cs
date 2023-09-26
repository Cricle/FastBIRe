using System.Data.Common;

namespace FastBIRe.Data
{
    public class SQLCognateMirrorCopy : IMirrorCopy<SQLMirrorCopyResult>
    {
        public SQLCognateMirrorCopy(DbConnection connection, string sourceSql, string targetNamed)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            SourceSql = sourceSql ?? throw new ArgumentNullException(nameof(sourceSql));
            TargetNamed = targetNamed ?? throw new ArgumentNullException(nameof(targetNamed));
        }

        public DbConnection Connection { get; }

        public string SourceSql { get; }

        public string TargetNamed { get; }

        public int CommandTimeout { get; set; } = 60 * 5;

        protected virtual string GetInsertQuery()
        {
            return $"INSERT INTO {TargetNamed} {SourceSql}";
        }

        public async Task<IList<SQLMirrorCopyResult>> CopyAsync(CancellationToken token)
        {
            var query = GetInsertQuery();
            using (var command = CreateCommand())
            {
                command.CommandText = query;
                var result = await command.ExecuteNonQueryAsync(token);
                return new[]
                {
                    new SQLMirrorCopyResult(result,query)
                };
            }
        }
        protected virtual DbCommand CreateCommand()
        {
            var comm = Connection.CreateCommand();
            comm.CommandTimeout = CommandTimeout;
            return comm;
        }
    }
}
