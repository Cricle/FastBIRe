using DatabaseSchemaReader.DataSchema;
using System.Data;
using System.Data.Common;

namespace FastBIRe
{
    public partial class MigrationService : DbMigration
    {
        public const string AutoGenIndexPrefx = "IDX_AG_";
        public const string DefaultEffectSuffix = "_effect";

        private string effectSuffix = DefaultEffectSuffix;

        public MigrationService(DbConnection connection) : base(connection)
        {
        }

        public MigrationService(DbConnection connection, string database) 
            : base(connection, database)
        {
        }

        public string EffectSuffix
        {
            get => effectSuffix;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException($"The EffectSuffix can't be null or empty");
                }
                effectSuffix = value;
            }
        }

        public bool EffectMode { get; set; }

        public bool EffectTrigger { get; set; }

        public bool ImmediatelyAggregate { get; set; }

        public string CreateTable(string table)
        {
            var migGen = DdlGeneratorFactory.MigrationGenerator();
            var tb = new DatabaseTable
            {
                Name = table,
            };
            tb.AddColumn("_id", DbType.Int64, x => x.AddIdentity().AddPrimaryKey($"PK_{table}_id"));
            tb.AddColumn("记录时间", DbType.DateTime, x =>
            {
                x.Nullable = false;
                x.AddIndex($"IDX_{table}_记录时间");
            });
            return migGen.AddTable(tb);
        }
        private bool IsIndexAck(DatabaseIndex idx, IEnumerable<string> fields)
        {
            if (idx == null)
            {
                return false;
            }
            if (idx.Columns.Count == fields.Count() &&
                idx.Columns.Select(x => x.Name).SequenceEqual(fields))
            {
                return true;
            }
            return false;
        }
        public async Task<int> SyncIndexAutoAsync(string tableName, IEnumerable<TableColumnDefine> columns, string? idxName = null, Action<SyncIndexOptions>? optionDec = null, CancellationToken token = default)
        {
            var len = await IndexByteLenHelper.GetIndexByteLenAsync(Connection, SqlType, timeOut: CommandTimeout);//bytes
            idxName ??= $"{AutoGenIndexPrefx}{tableName}";
            var res = 0;
            var table = Reader.Table(tableName);
            var needDrops = table.Indexes.Where(x=>x.Name.StartsWith(AutoGenIndexPrefx)).Select(x => x.Name).ToList();
            if (table != null)
            {
                var sourceIndexSize = columns.Sum(x => x.Length);
                var cols = columns.Select(x => x.Field).ToList();
                if (sourceIndexSize > len)
                {
                    var createdIdxs=new List<string>();
                    res += await SyncIndexSingleAsync(tableName, cols, createdIdxs,optionDec, token: token);
                    foreach (var item in createdIdxs)
                    {
                        needDrops.Remove(item);
                    }
                }
                else
                {
                    needDrops.Remove(idxName);
                    res += await SyncIndexAsync(tableName, cols, idxName, optionDec, token: token);
                }
            }
            if (needDrops.Count!=0)
            {
                foreach (var item in needDrops)
                {
                    var sql = TableHelper.DropIndex(item, tableName);
                    res += await ExecuteNonQueryAsync(sql, token: token);
                }
            }
            return res;
        }
        public async Task<int> SyncIndexAutoAsync(string destTable, SourceTableDefine tableDef, string? sourceIdxName = null, string? destIdxName = null, Action<SyncIndexOptions>? optionDec = null,CancellationToken token=default)
        {
            sourceIdxName ??= $"{AutoGenIndexPrefx}s_{tableDef.Table}";
            destIdxName ??= $"{AutoGenIndexPrefx}{destTable}";
            var groupColumns = tableDef.Columns.Where(x => x.IsGroup);
            var res = await SyncIndexAutoAsync(tableDef.Table, groupColumns, sourceIdxName, optionDec, token: token);
            res += await SyncIndexAutoAsync(destTable, groupColumns.Select(x=>x.DestColumn), sourceIdxName, optionDec, token: token);
            return res;
        }
        public Task<int> SyncIndexSingleAsync(string table, IEnumerable<string> columns, List<string>? outIndexNames = null, Action<SyncIndexOptions>? optionDec = null, CancellationToken token = default)
        {
            var option = new SyncIndexOptions
            {
                Table = table,
                Columns = columns,
                IndexNameCreator = s => $"{AutoGenIndexPrefx}{table}_{s}"
            };
            optionDec?.Invoke(option);
            return SyncIndexSingleAsync(option, outIndexNames, token: token);
        }
        public Task<int> SyncIndexAsync(string table, IEnumerable<string> columns, string? idxName = null, Action<SyncIndexOptions>? optionDec = null, CancellationToken token = default)
        {
            var opt = new SyncIndexOptions
            {
                Table = table,
                Columns = columns,
                IndexName = idxName,
                IndexNameCreator = s => $"{AutoGenIndexPrefx}{table}_{s}"
            };
            optionDec?.Invoke(opt);
            return SyncIndexAsync(opt,token: token);
        }
        public List<string> RunMigration(string table, IReadOnlyList<TableColumnDefine> news, IEnumerable<TableColumnDefine> oldRefs)
        {
            var s = new List<string>();
            var str = RunMigration(s, table, news, oldRefs);
            str.AddRange(s);
            return str;
        }
        public List<string> RunMigration(List<string> scripts,string table, IReadOnlyList<TableColumnDefine> news, IEnumerable<TableColumnDefine> oldRefs)
        {
            var groupNews = news.GroupBy(x => x.Field).Select(x => x.First()).ToList();
            var renames = groupNews.Join(oldRefs, x => x.Id, x => x.Id, (x, y) => new { Old = y, New = x });
            var migGen = DdlGeneratorFactory.MigrationGenerator();
            return CompareWithModify(table, x =>
            {
                var olds = x.Columns.Select(x =>
                {
                    var old = oldRefs.FirstOrDefault(y => y.Field == x.Name);
                    if (old != null)
                    {
                        old.Type = x.DbDataType;                        
                        return old;
                    }
                    return null;
                }).Where(x => x != null).ToList();
                foreach (var item in renames)
                {
                    var col = x.FindColumn(item.Old.Field);
                    if (col != null && col.Name != item.New.Field)
                    {
                        foreach (var idx in x.Indexes.Where(x => x.Columns.Any(y => y.Name == col.Name)))
                        {
                            scripts.Add(TableHelper.DropIndex(idx.Name, idx.TableName));
                        }
                        col.Name = item.New.Field;
                        var s = migGen.RenameColumn(x, col, item.Old.Field);
                        scripts.Add(s);
                        col.Name = item.Old.Field;
                    }
                }
                foreach (var item in groupNews)
                {
                    if (string.IsNullOrEmpty(item.Type))
                    {
                        continue;
                    }
                    var col = x.FindColumn(item.Field);
                    if (col != null)
                    {
                        //String will special
                        if (col.DbDataType.StartsWith("VARCHAR ", StringComparison.OrdinalIgnoreCase))
                        {
                            col.DbDataType = col.DbDataType.Remove(7, 1);
                        }
                        var leftType = col.DbDataType;
                        var rightType = item.Type!;
                        if (SqlType == SqlType.PostgreSql)
                        {
                            leftType = leftType.ToLower().Replace("decimal", "numeric");
                            rightType = rightType.ToLower().Replace("decimal", "numeric");
                        }
                        if (!string.Equals(leftType, rightType, StringComparison.OrdinalIgnoreCase))
                        {
                            col.DbDataType = item.Type;
                        }
                    }
                }
                var adds = groupNews.Where(n => !olds.Any(y => y.Id == n.Id)).ToList();
                var rms = new HashSet<string>(olds.Where(o => !groupNews.Any(y => y.Id == o.Id)).Select(x => x.Field));
                x.Columns.RemoveAll(x => rms.Contains(x.Name));
                foreach (var col in adds)
                {
                    x.AddColumn(col.Field, col.Type, x =>
                    {
                        x.Nullable = col.Nullable;
                    });
                }
            }).Execute();
        }
        private bool IsTimePart(ToRawMethod method) 
        {
            switch (method)
            {
                case ToRawMethod.Year:
                case ToRawMethod.Month:
                case ToRawMethod.Day:
                case ToRawMethod.Hour:
                case ToRawMethod.Minute:
                case ToRawMethod.Second:
                case ToRawMethod.Quarter:
                case ToRawMethod.Weak:
                    return true;
                default:
                    return false;
            }
        }
        public List<string> RunMigration(string destTable, SourceTableDefine tableDef, IEnumerable<SourceTableColumnDefine> oldRefs)
        {
            var news = tableDef.Columns.GroupBy(x=>x.Field).Select(x=>x.First()).ToList();
            var table = tableDef.Table;
            var scripts = new List<string>();
            var migGen = DdlGeneratorFactory.MigrationGenerator();
            var script = RunMigration(scripts, tableDef.Table, tableDef.Columns, oldRefs);
            var effectTableName = destTable + EffectSuffix;
            var triggerName = "trigger_"+effectTableName;
            var triggerHelper = TriggerHelper.Instance;
            
            scripts.Add(triggerHelper.Drop(triggerName, tableDef.Table, SqlType));
            var groupColumns = news.Where(x => x.IsGroup).ToList();
            if (EffectMode && !string.IsNullOrEmpty(destTable) && tableDef.Columns.Any(x => x.IsGroup))
            {
                var refTable = Reader.Table(effectTableName);
                var hasTale = false;
                if (refTable != null)
                {
                    if (refTable.Columns.Count != groupColumns.Count ||
                        !refTable.Columns.Select(x => x.Name).SequenceEqual(groupColumns.Select(x => x.Field)) ||
                        refTable.PrimaryKey == null ||
                        refTable.PrimaryKey.Columns.Count != refTable.Columns.Count ||
                        !refTable.PrimaryKey.Columns.SequenceEqual(refTable.Columns.Select(x => x.Name)) ||
                        !groupColumns.Select(x => x.Type).SequenceEqual(refTable.Columns.Select(x => x.DbDataType), StringComparer.OrdinalIgnoreCase))
                    {
                        scripts.Add(migGen.DropTable(refTable));
                    }
                    else
                    {
                        hasTale = true;
                    }
                }
                if (!hasTale)
                {
                    refTable = new DatabaseTable
                    {
                        Name = effectTableName
                    };
                    foreach (var item in groupColumns)
                    {
                        refTable.AddColumn(item.Field, item.Type);
                    }
                    var constrain = new DatabaseConstraint();
                    constrain.ConstraintType = ConstraintType.PrimaryKey;
                    constrain.Name = "FK_" + effectTableName;
                    constrain.TableName = effectTableName;
                    constrain.Columns.AddRange(groupColumns.Select(x => x.Field));
                    refTable.AddConstraint(constrain);
                    scripts.Add(migGen.AddTable(refTable));
                }
            }
            if (EffectTrigger)
            {
                var helper = new MergeHelper(SqlType);
                scripts.Add(triggerHelper.Create(triggerName, tableDef.Table, effectTableName, groupColumns.Select(x =>
                {
                    //if (IsTimePart(x.Method))
                    //{
                    //    return new TriggerField(x.Field, helper.ToRaw(x.Method, $"NEW.{helper.Wrap(x.Field)}", false));
                    //}
                    return new TriggerField(x.Field, $"NEW.{helper.Wrap(x.Field)}");
                }), SqlType)!);
            }
            var imdtriggerName = destTable + "_imd";
            var imdtriggerHelper = RealTriggerHelper.Instance;
            scripts.Add(imdtriggerHelper.Drop(imdtriggerName, tableDef.Table, SqlType));
            if (ImmediatelyAggregate)
            {
                scripts.Add(imdtriggerHelper.Create(imdtriggerName, destTable, tableDef, SqlType));
            }
            script.AddRange(scripts);
            return script;
        }
    }
}
