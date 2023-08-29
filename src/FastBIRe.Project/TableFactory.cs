using FastBIRe.Project.Models;

namespace FastBIRe.Project
{
    public class TableFactory<TResult, TProject, TId> : ITableFactory<TResult, TProject, TId>
        where TProject : IProject<TId>
        where TResult : ProjectCreateContextResult<TProject, TId>
    {
        public TableFactory(MigrationService service, ITableIniter tableIniter, TResult result)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            TableIniter = tableIniter ?? throw new ArgumentNullException(nameof(tableIniter));
            ProjectResult = result ?? throw new ArgumentNullException(nameof(result));
        }

        public TResult ProjectResult { get; }

        public MigrationService Service { get; }

        public ITableIniter TableIniter { get; }

        public bool OldColumnActual { get; set; } = true;

        public bool TableExists(string tableName) => Service.Reader.TableExists(tableName);

        public bool TryGetCreateScript(string tableName, out string? script)
        {
            if (!TableExists(tableName))
            {
                script = TableIniter.GetInitScript(Service, tableName);
                return true;
            }
            script = null;
            return false;
        }

        public async Task<MigrateToSqlRestul> MigrateToSqlAsync(string tableName, IReadOnlyList<TableColumnDefine> news, IEnumerable<TableColumnDefine>? olds, CancellationToken token = default)
        {
            if (TryGetCreateScript(tableName, out var tableCreateScript) && !string.IsNullOrEmpty(tableCreateScript))
            {
                await Service.ExecuteNonQueryAsync(tableCreateScript, token);
            }
            if (OldColumnActual && olds != null)
            {
                var actualColumns = Service.Reader.Table(tableName);
                var hasColumns = new HashSet<string>(olds.Where(x => x.Field != null).Select(x => x.Field)!);
                olds = olds.Where(x => x.Field != null && hasColumns.Contains(x.Field));
            }
            var res = Service.RunMigration(tableName, news, olds ?? Array.Empty<TableColumnDefine>());
            return new MigrateToSqlRestul(res, Service);
        }

        public void Dispose()
        {
            Service.Dispose();
        }
    }

}
