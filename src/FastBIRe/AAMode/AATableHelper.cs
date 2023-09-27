using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using FastBIRe.Naming;
using FastBIRe.Timing;
using FastBIRe.Triggering;
using System.Data.Common;

namespace FastBIRe.AAMode
{
    public delegate DatabaseTable MigrationTableHandler(DatabaseTable old, DatabaseTable @new);
    public partial class AATableHelper
    {
        public const string DefaultInsertTag = "insert";

        public const string DefaultUpdateTag = "update";

        public static readonly INameGenerator DefaultExpandTriggerNameGenerator = new RegexNameGenerator("EXP_{0}_{1}");

        public static readonly INameGenerator DefaultIndexNameGenerator = new RegexNameGenerator("IX_{0}");

        public static readonly INameGenerator DefaultEffectInsertNameGenerator = new RegexNameGenerator("EFF_{0}_INSERT");

        public static readonly INameGenerator DefaultEffectUpdateNameGenerator = new RegexNameGenerator("EFF_{0}_UPDATE");

        public AATableHelper(string tableName, DbConnection dbConnection)
            : this(tableName,
                  dbConnection,
                  Triggering.TriggerWriter.Default,
                  DefaultExpandTriggerNameGenerator,
                  DefaultIndexNameGenerator,
                  DefaultEffectInsertNameGenerator,
                  DefaultEffectUpdateNameGenerator,
                  EffectTableCreateAAModelHelper.DefaultEffectTableNameGenerator,
                  DefaultInsertTag,
                  DefaultUpdateTag,
                  null)
        {
        }
        public AATableHelper(string tableName, DbConnection dbConnection, ITriggerWriter triggerWriter, INameGenerator expandTriggerNameGenerator, INameGenerator indexNameGenerator, INameGenerator effectInsertTriggerNameGenerator, INameGenerator effectUpdateTriggerNameGenerator, INameGenerator effectTableNameGenerator, string insertTag, string updateTag, ITimeExpandHelper? timeExpandHelper)
        {
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            DbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            DatabaseReader = new DatabaseReader(dbConnection) { Owner = dbConnection.Database };
            ExpandTriggerNameGenerator = expandTriggerNameGenerator;
            InsertTag = insertTag;
            UpdateTag = updateTag;
            IndexNameGenerator = indexNameGenerator;
            EffectInsertTriggerNameGenerator = effectInsertTriggerNameGenerator;
            EffectUpdateTriggerNameGenerator = effectUpdateTriggerNameGenerator;
            EffectTableNameGenerator = effectTableNameGenerator;
            TriggerWriter = triggerWriter;
            TimeExpandHelper = timeExpandHelper ?? Timing.TimeExpandHelper.GetDefault(SqlType)??throw new ArgumentNullException($"{nameof(timeExpandHelper)} null or sql type not support");
        }

        public string TableName { get; }

        public DbConnection DbConnection { get; }

        public DatabaseReader DatabaseReader { get; }

        public DatabaseTable Table => DatabaseReader.Table(TableName);

        public SqlType SqlType => DatabaseReader.SqlType!.Value;

        public INameGenerator ExpandTriggerNameGenerator { get; }

        public INameGenerator IndexNameGenerator { get; }

        public INameGenerator EffectInsertTriggerNameGenerator { get; }

        public INameGenerator EffectUpdateTriggerNameGenerator { get; }

        public INameGenerator EffectTableNameGenerator { get; }

        public ITriggerWriter TriggerWriter { get; }

        public ITimeExpandHelper TimeExpandHelper { get; }

        public string InsertTag { get; }

        public string UpdateTag { get; }

        public FunctionMapper? FunctionMapper => FunctionMapper.Get(SqlType);

        public virtual IList<string> ExpandTimeMigrationScript(IEnumerable<string> columns, TimeTypes timeTypes = TimeTypes.ExceptSecond)
        {
            var exp = new TableExpandTimeAAModelHelper(new TimeExpandHelper(SqlType));
            var request = new TableExpandTimeRequest(TableName, columns, timeTypes);
            exp.Apply(DatabaseReader, request);
            return request.Scripts;
        }
        
        public virtual IList<string> EffectTableScript(string destTableName, IEnumerable<string> sourceColumnNames)
        {
            var request = EffectTableCreateAAModelRequest.From(DatabaseReader, TableName, destTableName, sourceColumnNames);
            EffectTableCreateAAModelHelper.Default.Apply(DatabaseReader, request);
            return request.Scripts;
        }
        public virtual IList<string> EffectTableScript(string destTableName, IReadOnlyList<EffectTableSettingItem> settingItems)
        {
            var sourceTable = Table;
            if (sourceTable == null)
                Throws.ThrowTableNotFound(TableName);
            var destTable = DatabaseReader.Table(destTableName);
            if (destTable == null)
                Throws.ThrowTableNotFound(destTableName);
            var request = new EffectTableCreateAAModelRequest(sourceTable!, destTable!, settingItems);
            EffectTableCreateAAModelHelper.Default.Apply(DatabaseReader, request);
            return request.Scripts;
        }

        public virtual IList<string> EffectScript(string destTableName, string effectTableName)
        {
            var sourceTable = Table;
            if (sourceTable == null)
                Throws.ThrowTableNotFound(TableName);
            var destTable = DatabaseReader.Table(destTableName);
            if (destTable == null)
                Throws.ThrowTableNotFound(destTableName);
            var effectTable = DatabaseReader.Table(effectTableName);
            if (effectTable == null)
                Throws.ThrowTableNotFound(effectTableName);
            var scripts = new List<string>();

            var extInsert = new EffectInsertTriggerAAModelHelper(EffectInsertTriggerNameGenerator, TriggerWriter);
            var request = new EffectTriggerAAModelRequest(sourceTable, destTable, effectTable);
            request.SettingItems.AddRange(effectTable.Columns.Select(x => EffectTriggerSettingItem.Trigger(x.Name, SqlType)));
            extInsert.Apply(DatabaseReader, request);
            scripts.AddRange(request.Scripts);

            var extUpdate = new EffectUpdateTriggerAAModelHelper(EffectUpdateTriggerNameGenerator, TriggerWriter);
            request = new EffectTriggerAAModelRequest(sourceTable, destTable, effectTable);
            request.SettingItems.AddRange(effectTable.Columns.Select(x => EffectTriggerSettingItem.Trigger(x.Name, SqlType)));
            extUpdate.Apply(DatabaseReader, request);
            scripts.AddRange(request.Scripts);

            return scripts;
        }
        public virtual IList<string> DropIndexScript(string field)
        {
            var table = Table;
            var name = IndexNameGenerator.Create(new[] { field });
            var index = table.Indexes.FirstOrDefault(x => x.Name == name);
            if (index != null)
            {
                var tableHelper = new TableHelper(SqlType);
                return new[] { tableHelper.DropIndex(name, TableName) }; 
            }
            return Array.Empty<string>();
        }
        public virtual IList<string> CreateIndexScript(string field, bool desc)
        {
            var scripts = new List<string>();
            var table = Table;
            //Check the index exists
            var name = IndexNameGenerator.Create(new[] { field });
            var index = table.Indexes.FirstOrDefault(x => x.Name == name);
            var tableHelper = new TableHelper(SqlType);
            if (index != null)
            {
                //Check index ok?
                if (index.Columns.Count == 1 &&
                    index.Columns[0].Name == field &&
                    index.ColumnOrderDescs.Count == 1 &&
                    index.ColumnOrderDescs[0] == desc)
                {
                    return scripts;
                }
                //Drop index
                scripts.Add(tableHelper.DropIndex(name, TableName));
            }

            scripts.Add(tableHelper.CreateIndex(name, TableName, new[] { field }, new[] { desc }));
            return scripts;
        }

        public virtual IList<string> ExpandTriggerScript(IEnumerable<string> columns, TimeTypes timeTypes = TimeTypes.ExceptSecond)
        {
            var scripts = new List<string>();
            var table = Table;
            var triggerInsertName = ExpandTriggerNameGenerator.Create(new[] { TableName, InsertTag });
            var triggerUpdateName = ExpandTriggerNameGenerator.Create(new[] { TableName, UpdateTag });
            var triggerInsert = table.Triggers.FirstOrDefault(x => x.Name == triggerInsertName);
            var triggerUpdate = table.Triggers.FirstOrDefault(x => x.Name == triggerUpdateName);
            if (triggerInsert != null)
            {
                scripts.AddRange(TriggerWriter.Drop(SqlType, triggerInsertName));
            }
            if (triggerUpdate != null)
            {
                scripts.AddRange(TriggerWriter.Drop(SqlType, triggerUpdateName));
            }

            var expandResults = columns.SelectMany(x => TimeExpandHelper.Create(x, timeTypes)).OfType<IExpandResult>().ToList();

            //Re read table 
            table = Table;
            if (SqlType == SqlType.SqlServer ||
                SqlType == SqlType.SqlServerCe)
            {
                scripts.AddRange(TriggerWriter.CreateExpand(SqlType, triggerInsertName, TriggerTypes.InsteadOfInsert, table, expandResults));
                scripts.AddRange(TriggerWriter.CreateExpand(SqlType, triggerUpdateName, TriggerTypes.InsteadOfUpdate, table, expandResults));
            }
            else
            {
                scripts.AddRange(TriggerWriter.CreateExpand(SqlType, triggerInsertName, TriggerTypes.AfterInsert, table, expandResults));
                scripts.AddRange(TriggerWriter.CreateExpand(SqlType, triggerUpdateName, TriggerTypes.AfterUpdate, table, expandResults));
            }
            return scripts;
        }
        public virtual IList<string> CreateTableOrMigrationScript(Func<DatabaseTable> tableCreator,MigrationTableHandler changeFun)
        {
            if (DatabaseReader.TableExists(TableName))
            {
                return GetTableMigrationScript(changeFun);
            }
            var table = tableCreator();
            var script = new DdlGeneratorFactory(SqlType).TableGenerator(table).Write();
            return new[] { script };
        }
        public virtual IList<string> CreateTableIfNotExistsScript(Func<DatabaseTable> tableCreator)
        {
            if (DatabaseReader.TableExists(TableName))
            {
                return Array.Empty<string>();
            }
            var table = tableCreator();
            var script = new DdlGeneratorFactory(SqlType).TableGenerator(table).Write();
            return new[] { script };
        }
        public virtual IList<string> GetTableMigrationScript(MigrationTableHandler changeFun)
        {
            var oldTable = DatabaseReader.Table(TableName);
            var newTable = DatabaseReader.Table(TableName);
            newTable = changeFun(oldTable, newTable);
            var comp = CompareSchemas.FromTable(DatabaseReader.DatabaseSchema.ConnectionString, SqlType, oldTable, newTable).ExecuteResult();
            var scripts = comp.Select(x => x.Script).ToList();
            return scripts;
        }
    }
}
