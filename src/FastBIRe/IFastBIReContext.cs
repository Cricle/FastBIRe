using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

namespace FastBIRe
{
    public interface IFastBIReContext : IDisposable
    {
        SqlType SqlType { get; }

        IDbScriptExecuter Executer { get; }

        DatabaseReader DatabaseReader { get; }

        ITableProvider TableProvider { get; }
    }
    public static class FastBIReContextGenExtensions
    {
        public static Task<int> ExecuteMigrationScriptsAsync(this IFastBIReContext context,string tableName, Action<DatabaseTable>? configRemoteTable = null, CancellationToken token = default)
        {
            var res = GetMigrationScripts(context, tableName, configRemoteTable);
            if (res.Scripts.Count==0)
            {
                return Task.FromResult(0);
            }
            return context.Executer.ExecuteBatchAsync(res.Scripts, token: token);
        }

        public static Task<int> ExecuteMigrationScriptsAsync(this IFastBIReContext context, Action<DatabaseTable>? configRemoteTable = null, CancellationToken token = default)
        {
            var res = GetMigrationScripts(context, configRemoteTable);
            if (res.Scripts.Count == 0)
            {
                return Task.FromResult(0);
            }
            return context.Executer.ExecuteBatchAsync(res.Scripts, token: token);
        }

        public static MigrationScriptsResult GetMigrationScripts(this IFastBIReContext context, Action<DatabaseTable>? configRemoteTable = null)
        {
            var tables = new List<string>();
            var results = new List<string>();
            foreach (var item in context.TableProvider)
            {
                var res = GetMigrationScripts(context, item.Name, configRemoteTable);
                tables.Add(res.TableName);
                results.AddRange(res.Scripts);
            }
            return new MigrationScriptsResult(tables, results);
        }
        public static MigrationScriptResult GetMigrationScripts(this IFastBIReContext context, string tableName,Action<DatabaseTable>? configRemoteTable=null)
        {
            var localTable = context.TableProvider.GetTable(tableName);
            if (localTable == null)
            {
                Throws.ThrowTableNotFound(tableName);
            }
            var remoteTable = context.DatabaseReader.Table(tableName);
            var ddlGen = new DdlGeneratorFactory(context.SqlType);
            if (remoteTable == null)
            {
                return new MigrationScriptResult(tableName, new[] { ddlGen.TableGenerator(localTable).Write() });
            }
            configRemoteTable?.Invoke(remoteTable);
            var comapreSchemas = CompareSchemas.FromTable(context.Executer.Connection.ConnectionString,
                context.SqlType,
                remoteTable,
                localTable);

            var results = comapreSchemas.ExecuteResult();
            return new MigrationScriptResult(tableName, results.Select(x => x.Script).ToList());
        }
    }
}
