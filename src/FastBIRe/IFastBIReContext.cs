using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using FastBIRe.Wrapping;
using System.Data;
using System.Data.Common;

namespace FastBIRe
{
    public interface IFastBIReContextFactory : IDisposable
    {
        Task<IFastBIReContext> CreateContextAsync(CancellationToken token = default);

        IFastBIReContext CreateContext();
    }
    public class DelegateFastBIReContextFactory : IFastBIReContextFactory
    {
        public DelegateFastBIReContextFactory(Func<IDbScriptExecuter> scriptExecuterFactory, ITableProvider tableProvider)
        {
            ScriptExecuterFactory = scriptExecuterFactory ?? throw new ArgumentNullException(nameof(scriptExecuterFactory));
            TableProvider = tableProvider ?? throw new ArgumentNullException(nameof(tableProvider));
        }

        public Func<IDbScriptExecuter> ScriptExecuterFactory { get; }

        public ITableProvider TableProvider { get; }

        public IFastBIReContext CreateContext()
        {
            var db = ScriptExecuterFactory();
            if (db.Connection.State != ConnectionState.Open)
            {

                db.Connection.Open();
            }
            return new FastBIReContext(db, TableProvider);
        }

        public async Task<IFastBIReContext> CreateContextAsync(CancellationToken token = default)
        {
            var db = ScriptExecuterFactory();
            if (db.Connection.State != ConnectionState.Open)
            {
                await db.Connection.OpenAsync(token);
            }
            return new FastBIReContext(db, TableProvider);
        }

        public void Dispose()
        {
        }
    }
    public abstract class FastBIReContextFactoryBase : IFastBIReContextFactory
    {
        public abstract IFastBIReContext CreateContext();

        public Task<IFastBIReContext> CreateContextAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(CreateContext());
        }

        public virtual void Dispose()
        {
        }
    }
    public interface IFastBIReContext : IDisposable
    {
        SqlType SqlType { get; }

        IEscaper Escaper { get; }

        IDbScriptExecuter Executer { get; }

        DbConnection Connection { get; }

        DatabaseReader DatabaseReader { get; }

        ITableProvider TableProvider { get; }
    }
    public static class FastBIReContextGenExtensions
    {
        public static Task<int> ExecuteMigrationScriptsAsync(this IFastBIReContext context, string tableName, Action<DatabaseTable>? configRemoteTable = null, CancellationToken token = default)
        {
            var res = GetMigrationScripts(context, tableName, configRemoteTable);
            if (res.Scripts.Count == 0)
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
        public static MigrationScriptResult GetMigrationScripts(this IFastBIReContext context, string tableName, Action<DatabaseTable>? configRemoteTable = null)
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
