using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Builders;
using DatabaseSchemaReader.SqlGen;
using DatabaseSchemaReader.Utilities;
using FastBIRe.Comparing;
using FastBIRe.Naming;
using FastBIRe.Store;
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

        public static readonly INameGenerator DefaultIndexNameGenerator = new RegexNameGenerator("IX_{0}_{1}");

        public static readonly INameGenerator DefaultEffectInsertNameGenerator = new RegexNameGenerator("EFF_{0}_INSERT");

        public static readonly INameGenerator DefaultEffectUpdateNameGenerator = new RegexNameGenerator("EFF_{0}_UPDATE");

        public static readonly INameGenerator DefaultPrimaryKeyNameGenerator = new RegexNameGenerator("PK_{0}");

        public AATableHelper(string tableName, DbConnection dbConnection)
            : this(tableName,
                  dbConnection,
                  Triggering.TriggerWriter.Default,
                  DefaultExpandTriggerNameGenerator,
                  DefaultIndexNameGenerator,
                  DefaultEffectInsertNameGenerator,
                  DefaultEffectUpdateNameGenerator,
                  EffectTableCreateAAModelHelper.DefaultEffectTableNameGenerator,
                  DefaultPrimaryKeyNameGenerator,
                  DefaultInsertTag,
                  DefaultUpdateTag,
                  null,
                  SqlComparer.Instance)
        {
        }
        public AATableHelper(string tableName, DbConnection dbConnection, ITriggerWriter triggerWriter, INameGenerator expandTriggerNameGenerator, INameGenerator indexNameGenerator, INameGenerator effectInsertTriggerNameGenerator, INameGenerator effectUpdateTriggerNameGenerator, INameGenerator effectTableNameGenerator, INameGenerator primaryKeyNameGenerator, string insertTag, string updateTag, ITimeExpandHelper? timeExpandHelper, IEqualityComparer<string?> sqlComparer)
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
            TimeExpandHelper = timeExpandHelper ?? Timing.TimeExpandHelper.GetDefault(SqlType) ?? throw new ArgumentNullException($"{nameof(timeExpandHelper)} null or sql type not support");
            PrimaryKeyNameGenerator = primaryKeyNameGenerator;
            SqlEqualityComparer = sqlComparer;
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

        public INameGenerator PrimaryKeyNameGenerator { get; }

        public ITriggerWriter TriggerWriter { get; }

        public ITimeExpandHelper TimeExpandHelper { get; }

        public string InsertTag { get; }

        public string UpdateTag { get; }

        public FunctionMapper? FunctionMapper => FunctionMapper.Get(SqlType);

        public string PrimaryKeyName => PrimaryKeyNameGenerator.Create(new[] { TableName });

        public IEqualityComparer<string?> SqlEqualityComparer { get; }

        public IDataStore? TriggerDataStore { get; set; }

        public string GetEffectTableName(string table)
        {
            return EffectTableCreateAAModelHelper.Default.EffectNameGenerator.Create(new[] { table });
        }

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
        public virtual IList<string> DropEffectScript(string destTableName, string effectTableName)
        {
            var triggers = DatabaseReader.Table(destTableName);
            if (triggers == null)
            {
                return Array.Empty<string>();
            }
            return triggers.Triggers.SelectMany(x => TriggerWriter.Drop(SqlType, x.Name, x.TableName)).ToList();
        }
        protected virtual IList<string> EffectScriptCore(string destTableName, string effectTableName, Action<EffectTriggerAAModelHelper>? helperDesc)
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
            extInsert.TriggerDataStore = TriggerDataStore;
            extInsert.SqlEqualityComparer = SqlEqualityComparer;
            extInsert.CheckRemote = SqlType != SqlType.PostgreSql;
            helperDesc?.Invoke(extInsert);
            var request = new EffectTriggerAAModelRequest(sourceTable, destTable, effectTable);
            request.SettingItems.AddRange(effectTable.Columns.Select(x => EffectTriggerSettingItem.Trigger(x.Name, SqlType)));
            extInsert.Apply(DatabaseReader, request);
            scripts.AddRange(request.Scripts);

            var extUpdate = new EffectUpdateTriggerAAModelHelper(EffectUpdateTriggerNameGenerator, TriggerWriter);
            extUpdate.TriggerDataStore = TriggerDataStore;
            extUpdate.SqlEqualityComparer = SqlEqualityComparer;
            extUpdate.CheckRemote = SqlType != SqlType.PostgreSql;
            helperDesc?.Invoke(extUpdate);
            request = new EffectTriggerAAModelRequest(sourceTable, destTable, effectTable);
            request.SettingItems.AddRange(effectTable.Columns.Select(x => EffectTriggerSettingItem.Trigger(x.Name, SqlType)));
            extUpdate.Apply(DatabaseReader, request);
            scripts.AddRange(request.Scripts);

            return scripts;
        }
        public virtual IList<string> EffectScript(string destTableName, string effectTableName)
        {
            return EffectScriptCore(destTableName, effectTableName, null);
        }
        public virtual IList<string> DropIndexScript(string field)
        {
            var table = Table;
            var name = IndexNameGenerator.Create(new[] { TableName, field });
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
            var name = IndexNameGenerator.Create(new[] { TableName, field });
            var index = table.Indexes.FirstOrDefault(x => x.Name == name);
            var tableHelper = new TableHelper(SqlType);
            if (index != null)
            {
                //Check index ok?
                if (index.Columns.Count == 1 &&
                    index.Columns[0].Name == field)
                {
                    if (SqlType == SqlType.SQLite)
                    {
                        return scripts;
                    }
                    else if (index.ColumnOrderDescs.Count == 1 && index.ColumnOrderDescs[0] == desc)
                    {

                        return scripts;
                    }
                }
                //Drop index
                scripts.Add(tableHelper.DropIndex(name, TableName));
            }

            scripts.Add(tableHelper.CreateIndex(name, TableName, new[] { field }, new[] { desc }));
            return scripts;
        }
        public virtual IList<string> CreatePrimaryKeyScripts(IReadOnlyList<string> columns)
        {
            var scripts = new List<string>();
            var table = Table;
            var ddl = new DdlGeneratorFactory(SqlType).MigrationGenerator();
            var pkName = PrimaryKeyName;
            if (table.PrimaryKey != null && table.PrimaryKey.Columns.Count != 0)
            {
                var equals = SqlType == SqlType.SQLite || table.PrimaryKey.Name != pkName;
                if (equals)
                {
                    if (table.PrimaryKey.Columns.Count == columns.Count &&
                    table.PrimaryKey.Columns.All(x => columns.Contains(x)))
                    {
                        return Array.Empty<string>();

                    }
                }
                scripts.Add(ddl.DropConstraint(table, table.PrimaryKey));
            }
            var pk = new DatabaseConstraint
            {
                Name = pkName,
                ConstraintType = ConstraintType.PrimaryKey
            };
            table.AddConstraint(pk);
            pk.Columns.AddRange(columns);
            scripts.Add(ddl.AddConstraint(table, table.PrimaryKey));
            return scripts;
        }
        private string? GetStoredTriggerScripts(string name)
        {
            return TriggerDataStore?.GetString(name);
        }
        protected virtual IList<string> CreateExpandTriggerScript(IEnumerable<string> columns, TriggerTypes triggerType, TimeTypes timeTypes = TimeTypes.ExceptSecond)
        {
            var scripts = new List<string>();
            var table = Table;
            var tag = triggerType == TriggerTypes.InsteadOfInsert || triggerType == TriggerTypes.AfterInsert ? InsertTag : UpdateTag;
            var triggerName = ExpandTriggerNameGenerator.Create(new[] { TableName, tag });
            var trigger = table.Triggers.FirstOrDefault(x => x.Name == triggerName);
            IEnumerable<string>? dropTriggers = null;
            if (trigger != null)
            {
                dropTriggers = TriggerWriter.Drop(SqlType, triggerName, TableName);
            }

            var expandResults = columns.SelectMany(x => TimeExpandHelper.Create(x, timeTypes)).OfType<IExpandResult>().ToList();

            //Re read table 
            table = Table;
            var autoColumns = table.Columns.Where(x => x.IsAutoNumber).Select(x => x.Name).ToList();
            var hasIdentity = autoColumns.Count != 0;
            var insertScripts = TriggerWriter.CreateExpand(SqlType, triggerName, triggerType, table, expandResults, hasIdentity, autoColumns);
            var insertStoredScript = GetStoredTriggerScripts(triggerName);

            var nowScripts = string.Join("\n", insertScripts);
            var needReCreate = trigger == null;
            if (!needReCreate)
            {
                //Check remote if need
                if (SqlType != SqlType.PostgreSql)
                {
                    if (!SqlEqualityComparer.Equals(trigger!.TriggerBody, nowScripts))
                    {
                        needReCreate = true;
                    }
                }
                else if (string.IsNullOrEmpty(insertStoredScript) ||
                    !SqlEqualityComparer.Equals(insertStoredScript, nowScripts))
                {
                    needReCreate = true;
                }
            }
            if (needReCreate)
            {
                if (dropTriggers != null)
                {
                    scripts.AddRange(dropTriggers);
                }
                scripts.AddRange(insertScripts);
                TriggerDataStore?.SetString(triggerName, nowScripts);
            }
            return scripts;
        }
        public virtual IList<string> ExpandTriggerScript(IEnumerable<string> columns, TimeTypes timeTypes = TimeTypes.ExceptSecond)
        {
            var scripts = new List<string>();
            var table = DatabaseReader.Table(TableName, ReadTypes.Triggers);
            var triggerInsertName = ExpandTriggerNameGenerator.Create(new[] { TableName, InsertTag });
            var triggerUpdateName = ExpandTriggerNameGenerator.Create(new[] { TableName, UpdateTag });
            var triggerInsert = table.Triggers.FirstOrDefault(x => x.Name == triggerInsertName);
            var triggerUpdate = table.Triggers.FirstOrDefault(x => x.Name == triggerUpdateName);
            IEnumerable<string>? dropInsertTriggers = null;
            IEnumerable<string>? dropUpdateTriggers = null;
            if (triggerInsert != null)
            {
                dropInsertTriggers = TriggerWriter.Drop(SqlType, triggerInsertName, TableName);
            }
            if (triggerUpdate != null)
            {
                dropUpdateTriggers = TriggerWriter.Drop(SqlType, triggerUpdateName, TableName);
            }

            var expandResults = columns.SelectMany(x => TimeExpandHelper.Create(x, timeTypes)).OfType<IExpandResult>().ToList();

            //Re read table 
            table = DatabaseReader.Table(TableName, ReadTypes.Columns | ReadTypes.CheckConstraints | ReadTypes.Pks);
            var insertTriggerTypes = SqlType == SqlType.SqlServer || SqlType == SqlType.SqlServerCe ? TriggerTypes.InsteadOfInsert : TriggerTypes.BeforeInsert;
            var updateTriggerTypes = SqlType == SqlType.SqlServer || SqlType == SqlType.SqlServerCe ? TriggerTypes.InsteadOfUpdate : TriggerTypes.BeforeUpdate;
            if (SqlType == SqlType.SQLite)
            {
                insertTriggerTypes = TriggerTypes.AfterInsert;
                updateTriggerTypes = TriggerTypes.AfterUpdate;
            }
            var autoColumns = table.Columns.Where(x => x.IsAutoNumber).Select(x => x.Name).ToList();
            var hasIdentity = autoColumns.Count != 0;
            var insertScripts = TriggerWriter.CreateExpand(SqlType, triggerInsertName, insertTriggerTypes, table, expandResults, hasIdentity, autoColumns);
            var updateScripts = TriggerWriter.CreateExpand(SqlType, triggerUpdateName, updateTriggerTypes, table, expandResults, hasIdentity, autoColumns);
            var insertStoredScript = GetStoredTriggerScripts(triggerInsertName);
            var updateStoredScript = GetStoredTriggerScripts(triggerUpdateName);

            var insertStored = string.Join("\n", insertScripts);
            var updateStored = string.Join("\n", updateScripts);

            if (triggerInsert == null ||
                string.IsNullOrEmpty(insertStoredScript) ||
                !SqlEqualityComparer.Equals(insertStoredScript, insertStored))
            {
                if (dropInsertTriggers != null)
                {
                    scripts.AddRange(dropInsertTriggers);
                }
                scripts.AddRange(insertScripts);
                TriggerDataStore?.SetString(triggerInsertName, insertStored);
            }
            if (triggerUpdate == null ||
                string.IsNullOrEmpty(updateStoredScript) ||
                !SqlEqualityComparer.Equals(updateStoredScript, updateStored))
            {
                if (dropUpdateTriggers != null)
                {
                    scripts.AddRange(dropUpdateTriggers);
                }
                scripts.AddRange(updateScripts);
                TriggerDataStore?.SetString(triggerUpdateName, updateStored);
            }
            return scripts;
        }
        public virtual IList<string> CreateTableOrMigrationScript(Func<DatabaseTable> tableCreator, MigrationTableHandler changeFun)
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
            var oldTable = DatabaseReader.Table(TableName, ReadTypes.AllColumns);
            var newTable = oldTable.Clone();
            newTable = changeFun(oldTable, newTable);
            var comp = CompareSchemas.FromTable(DatabaseReader.DatabaseSchema.ConnectionString, SqlType, oldTable, newTable).ExecuteResult();
            var scripts = comp.Select(x => x.Script).ToList();
            return scripts;
        }
    }
}
