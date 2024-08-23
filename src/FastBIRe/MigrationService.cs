using DatabaseSchemaReader.DataSchema;
using System.Data;
using System.Data.Common;
using System.Dynamic;

namespace FastBIRe
{
    public partial class MigrationService : DbMigration
    {
        public const string DefaultInsertQueryViewFormat = "vq_{0}_insert";

        public const string DefaultUpdateQueryViewFormat = "vq_{0}_update";

        public const string AutoTimeTriggerPrefx = "AGT_";
        public const string AutoGenIndexPrefx = "IXAG_";
        public const string AutoGenForceIndexPrefx = "IXFG_";
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

        public IReadOnlyList<string>? NotRemoveColumns { get; set; } = new string[] { "_id", "记录时间" };

        public Func<string, string> IdColumnFetcher { get; set; } = s => "_id";

        public bool ViewMode { get; set; }

        public string InsertQueryViewFormat { get; set; } = DefaultInsertQueryViewFormat;

        public string UpdateQueryViewFormat { get; set; } = DefaultUpdateQueryViewFormat;

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
        public async Task<int> SyncIndexAutoAsync(string tableName, IEnumerable<TableColumnDefine> columns, string? idxName = null, string? refTable = null, Action<SyncIndexOptions>? optionDec = null, CancellationToken token = default)
        {
            var tableEncod = MD5Helper.ComputeHash(refTable ?? tableName);
            var len = await IndexByteLenHelper.GetIndexByteLenAsync(Connection, SqlType, timeOut: CommandTimeout);//bytes
            idxName ??= $"{AutoGenIndexPrefx}{tableEncod}";
            var res = 0;
            var table = Reader.Table(tableName);
            var needDrops = table.Indexes.Where(x => x.Name.StartsWith(AutoGenIndexPrefx + tableEncod)).Select(x => x.Name).ToList();
            if (table != null)
            {
                var sourceIndexSize = columns.Sum(x => x.Length);
                var cols = columns.Select(x => x.Field).ToList();
                if (sourceIndexSize > len)
                {
                    var createdIdxs = new List<string>();
                    res += await SyncIndexSingleAsync(tableName, cols, createdIdxs, optionDec, refTable: refTable ?? tableName, token: token);
                    foreach (var item in createdIdxs)
                    {
                        needDrops.Remove(item);
                    }
                }
                else
                {
                    needDrops.Remove(idxName);
                    res += await SyncIndexAsync(tableName, cols, idxName, optionDec, refTable: refTable ?? tableName, token: token);
                }
            }
            if (needDrops.Count != 0)
            {
                table = Reader.Table(tableName);
                foreach (var item in needDrops)
                {
                    if (table.Indexes.Any(x => x.Name == item))
                    {
                        var sql = TableHelper.DropIndex(item, tableName);
                        res += await ExecuteNonQueryAsync(sql, token: token);
                    }
                }
            }
            return res;
        }
        public async Task<int> SyncIndexAutoAsync(string destTable, SourceTableDefine tableDef, string? sourceIdxName = null, string? destIdxName = null, Action<SyncIndexOptions>? optionDec = null, CancellationToken token = default)
        {
            sourceIdxName ??= $"{AutoGenIndexPrefx}{MD5Helper.ComputeHash(destTable)}";
            destIdxName ??= sourceIdxName + "_g";
            var groupColumns = new List<SourceTableColumnDefine>();
            var helper = GetMergeHelper();
            foreach (var item in tableDef.Columns)
            {
                if (item.IsGroup)
                {
                    if (item.ExpandDateTime)
                    {
                        var clone = item.Copy();
                        var field = DefaultDateTimePartNames.GetField(clone.Method, clone.Field, out var ok);
                        if (ok)
                        {
                            clone.Field = field;
                            clone.Raw = clone.RawFormat = helper.Wrap(clone.Field);
                        }
                        groupColumns.Add(clone);
                    }
                    else
                    {
                        groupColumns.Add(item);
                    }
                }
            }
            var res = await SyncIndexAutoAsync(tableDef.Table, groupColumns, sourceIdxName, destTable, optionDec, token: token);
            res += await SyncIndexAutoAsync(destTable, groupColumns.Select(x => x.DestColumn), destIdxName, destTable, optionDec, token: token);
            return res;
        }
        public Task<int> SyncIndexSingleAsync(string table, IEnumerable<string> columns, List<string>? outIndexNames = null, Action<SyncIndexOptions>? optionDec = null, string? refTable = null, CancellationToken token = default)
        {
            var option = new SyncIndexOptions
            {
                Table = table,
                Columns = columns,
                IndexNameCreator = s => $"{AutoGenIndexPrefx}{MD5Helper.ComputeHash(refTable ?? table)}_{s}"
            };
            optionDec?.Invoke(option);
            return SyncIndexSingleAsync(option, outIndexNames, token: token);
        }
        public Task<int> SyncIndexAsync(string table, IEnumerable<string> columns, string? idxName = null, Action<SyncIndexOptions>? optionDec = null, string? refTable = null, CancellationToken token = default)
        {
            var opt = new SyncIndexOptions
            {
                Table = table,
                Columns = columns,
                IndexName = idxName,
                IndexNameCreator = s => $"{AutoGenIndexPrefx}{MD5Helper.ComputeHash(refTable ?? table)}_{s}"
            };
            optionDec?.Invoke(opt);
            return SyncIndexAsync(opt, token: token);
        }
        public List<string> RunMigration(string table, IReadOnlyList<TableColumnDefine> news, IEnumerable<TableColumnDefine> oldRefs)
        {
            var s = new List<string>();
            var str = RunMigration(s, table, news, oldRefs);
            str.AddRange(s);
            return str;
        }
        public virtual IReadOnlyList<KeyValuePair<string, ToRawMethod>> AddDateTimePartColumns(DatabaseTable table, string field)
        {
            var builder = GetColumnBuilder();
            var dateTimeType = builder.Type(DbType.DateTime);
            var parts = DefaultDateTimePartNames.GetDatePartNames(field);
            var ret = new List<KeyValuePair<string, ToRawMethod>>();
            foreach (var item in parts)
            {
                if (!table.Columns.Any(x => x.Name == item.Key))
                {
                    table.AddColumn(item.Key, dateTimeType, x => x.Nullable = true);
                    ret.Add(item);
                }
            }
            return ret;
        }
        public static string MakeForceIndexName(string table, IEnumerable<string> columns, IEnumerable<bool> orders)
        {
            var tableMd5 = MD5Helper.ComputeHash(table);
            var colsMd5 = MD5Helper.ComputeHash(string.Join("_", columns.Concat(orders.Select(x => x ? "1" : "0"))));
            return $"{AutoGenForceIndexPrefx}_{tableMd5}_{colsMd5}";
        }
        public List<string> RunMigration(List<string> scripts, string table, IReadOnlyList<TableColumnDefine> news, IEnumerable<TableColumnDefine> oldRefs)
        {
            var groupNews = news.GroupBy(x => x.Field).Select(x => x.First()).ToList();
            var renames = groupNews.Join(oldRefs, x => x.Id, x => x.Id, (x, y) => new { Old = y, New = x, IsRename = x.Field != y.Field });
            var migGen = DdlGeneratorFactory.MigrationGenerator();
            var tb = Reader.Table(table);
            var helper = GetMergeHelper();
            var scriptBefore = new List<string>();
            var dropedIndexs = new HashSet<string>();
            var res = CompareWithModify(table, x =>
            {
                PrepareTable(x);
                var olds = x.Columns.Select(x =>
                {
                    var @new = news.FirstOrDefault(y => y.Field == x.Name);
                    if (@new != null)
                    {
                        x.Tag = true;
                        x.Length = @new.Length;
                        x.Nullable = @new.Nullable;
                        x.DefaultValue = @new.DefaultValue;
                    }
                    var old = oldRefs.FirstOrDefault(y => y.Field == x.Name);
                    if (old != null)
                    {
                        old.Type = x.DbDataType;
                        return old;
                    }
                    return null;
                }).Where(x => x != null).ToList();
                var partColumns = new HashSet<string>(x.Columns.Where(x => x.Name.StartsWith(DefaultDateTimePartNames.SystemPrefx)).Select(x => x.Name));
                foreach (var item in news)
                {
                    if (item.ExpandDateTime)
                    {
                        foreach (var name in DefaultDateTimePartNames.GetDatePartNames(item.Field))
                        {
                            partColumns.Remove(name.Key);
                        }
                        var res = AddDateTimePartColumns(x, item.Field);

                        if (SqlType != SqlType.SQLite)
                        {
                            foreach (var r in res)
                            {
                                scripts.Add($"UPDATE {helper.Wrap(x.Name)} SET {helper.Wrap(r.Key)} = {helper.ToRaw(r.Value, item.Field, true)};");
                            }
                        }
                    }
                }
                if (partColumns.Count != 0)
                {
                    foreach (var item in partColumns)
                    {
                        x.Columns.RemoveAll(x => x.Name == item);
                    }
                }
                var existsingIndexs = new HashSet<string>(x.Indexes.Select(x => x.Name));
                foreach (var item in renames)
                {
                    var col = x.FindColumn(item.Old.Field);
                    if (col != null && col.Name != item.New.Field)
                    {
                        //var idxs = x.Indexes.Where(x => x.Columns.Any(y => y.Name == col.Name)).ToList();
                        //foreach (var idx in idxs)
                        //{
                        //    if (dropedIndexs.Add(idx.Name))
                        //    {
                        //        scriptBefore.Add(TableHelper.DropIndex(idx.Name, idx.TableName));
                        //        existsingIndexs.Remove(idx.Name);
                        //    }
                        //}
                        col.Name = item.New.Field;
                        var s = migGen.RenameColumn(x, col, item.Old.Field);
                        scriptBefore.Add(s);
                        col.Name = item.Old.Field;
                    }
                }
                var affectIndexs = new List<string>();
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
                            if (col.IsIndexed)
                            {
                                affectIndexs.AddRange(x.Indexes.Where(y => y.Columns.Any(q => q.Name == col.Name)).Select(y => y.Name));
                            }
                            //var idxs = x.Indexes.Where(x => existsingIndexs.Contains(x.Name) && x.Columns.Any(y => y.Name == col.Name)).ToList();
                            //foreach (var idx in idxs)
                            //{
                            //    if (dropedIndexs.Add(idx.Name))
                            //    {
                            //        scriptBefore.Add(TableHelper.DropIndex(idx.Name, idx.TableName));
                            //        existsingIndexs.Remove(idx.Name);
                            //    }
                            //}
                        }
                    }
                }
                var adds = groupNews.Where(n => !olds.Any(y => y.Id == n.Id)).ToList();
                var rms = new HashSet<string>(
                    x.Columns.Where(x => !x.Name.StartsWith(DefaultDateTimePartNames.SystemPrefx) && x.Tag == null && (NotRemoveColumns == null || !NotRemoveColumns.Contains(x.Name)))
                    .Select(x => x.Name)
                    .Where(x => !news.Any(y => y.Field == x))
                    .Where(x => !renames.Any(y => y.IsRename && y.Old.Field == x)));
                x.Columns.RemoveAll(x => rms.Contains(x.Name));
                foreach (var col in adds)
                {
                    x.AddColumn(col.Field, col.Type, x =>
                    {
                        x.Nullable = col.Nullable;
                        x.DefaultValue = col.DefaultValue;
                        x.Length = col.Length;
                    });
                }
                var alls = new HashSet<string>(x.Indexes.Where(y => y.Name.StartsWith(AutoGenForceIndexPrefx)).Select(y => y.Name).Except(affectIndexs));
                foreach (var item in news.Where(x => !string.IsNullOrEmpty(x.IndexGroup)).GroupBy(x => x.IndexGroup))
                {
                    var fields = item.OrderBy(y => y.IndexOrder).Select(y => y.Field).ToArray();
                    var idxName = MakeForceIndexName(x.Name, fields, item.OrderBy(y => y.IndexOrder).Select(x => x.Desc));
                    if (!x.Indexes.Any(y => y.Name == idxName))
                    {
                        scripts.Add(TableHelper.CreateIndex(idxName, table, fields, item.OrderBy(y => y.IndexOrder).Select(y => y.Desc).ToArray(), item.Any(x => x.IndexUnique)));
                    }
                    alls.Remove(idxName);
                }
                if (alls.Count != 0)
                {
                    foreach (var item in alls)
                    {
                        if (dropedIndexs.Add(item))
                        {
                            scripts.Add(TableHelper.DropIndex(item, x.Name));
                        }
                    }
                }
            }).Execute();
            res = scriptBefore.Concat(res).ToList();
            var key = AutoTimeTriggerPrefx + table;
            res.AddRange(tb.Triggers.Where(x => x.Name.StartsWith(key)).Select(x => ComputeTriggerHelper.Instance.DropRaw(x.Name, x.TableName, SqlType))!);
            res.AddRange(tb.Triggers.Where(x => x.Name.StartsWith("trigger_")).Select(x => ComputeTriggerHelper.Instance.DropRaw(x.Name, x.TableName, SqlType))!);
            var computeField = news.Where(x => x.ExpandDateTime).ToList();
            if (computeField.Count != 0)
            {
                var triggerSqls = CreateExpandTriggerSql(key, table, helper, computeField);
                res.AddRange(triggerSqls);
            }
            return res;
        }
        private IEnumerable<string> CreateExpandTriggerSql(string key,string table,MergeHelper helper,IEnumerable<TableColumnDefine> columnDefines)
        {
            var triggers = new List<TriggerField>();
            foreach (var item in columnDefines)
            {
                foreach (var part in DefaultDateTimePartNames.GetDatePartNames(item.Field))
                {
                    triggers.Add(new TriggerField(part.Key,
                        helper.ToRaw(part.Value, $"{(SqlType == SqlType.SqlServer ? string.Empty : "NEW.")}{helper.Wrap(item.Field)}", false)!, item.Field, item.Type, item.RawFormat));
                }
            }
            var id = IdColumnFetcher(table);
            return ComputeTriggerHelper.Instance.Create(key, table, triggers, id, SqlType)!;
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
                case ToRawMethod.Week:
                    return true;
                default:
                    return false;
            }
        }
        public List<string> RunMigration(string destTable, SourceTableDefine tableDef, IEnumerable<TableColumnDefine> oldRefs, bool syncSource)
        {
            var news = tableDef.Columns;
            var table = tableDef.Table;
            var scripts = new List<string>();
            var migGen = DdlGeneratorFactory.MigrationGenerator();
            var script = new List<string>();
            if (syncSource)
            {
                script = RunMigration(scripts, tableDef.Table, tableDef.Columns, oldRefs);
            }
            var effectTableName = destTable + EffectSuffix;
            var triggerName = "trigger_" + effectTableName;
            var triggerHelper = EffectTriggerHelper.Instance;

            scripts.AddRange(triggerHelper.Drop(triggerName, tableDef.Table, SqlType));
            var groupColumns = news.Where(x => x.IsGroup).Select(x => x.Copy()).ToList();
            if (EffectMode && !string.IsNullOrEmpty(destTable) && tableDef.Columns.Any(x => x.IsGroup))
            {
                var refTable = Reader.Table(effectTableName);
                var oldRefTable = refTable;
                var hasTale = false;
                if (refTable != null)
                {
                    //FIXME: 判断要看expand
                    if (refTable.Columns.Count != groupColumns.Count ||
                        !refTable.Columns.Select(x => x.Name).SequenceEqual(groupColumns.Select(x =>
                        {
                            var field = DefaultDateTimePartNames.GetField(x.Method, x.Field, out var ok);
                            if (ok)
                            {
                                return field;
                            }
                            return x.Field;
                        })) ||
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
                        var field = item.Field;
                        if (item.ExpandDateTime)
                        {
                            var f = DefaultDateTimePartNames.GetField(item.Method, item.Field, out var ok);
                            if (ok)
                            {
                                field = f;
                            }
                        }
                        var column=refTable.AddColumn(field, item.Type);
                        column.Nullable = item.Nullable;
                        column.Length = item.Length;
                        column.DefaultValue = item.DefaultValue;
                    }
                    var constrain = new DatabaseConstraint();
                    constrain.ConstraintType = ConstraintType.PrimaryKey;
                    constrain.Name = "FK_" + MD5Helper.ComputeHash(effectTableName);
                    constrain.TableName = effectTableName;
                    constrain.Columns.AddRange(refTable.Columns.Select(x=>x.Name));
                    refTable.AddConstraint(constrain);
                    scripts.Add(migGen.AddTable(refTable));
                }
            }
            else if (Reader.TableExists(effectTableName))
            {
                scripts.Add(TableHelper.CreateDropTable(effectTableName));
            }
            var helper = new MergeHelper(SqlType);
            if (EffectTrigger)
            {
                scripts.AddRange(triggerHelper.Create(triggerName, tableDef.Table, effectTableName, groupColumns.Select(x =>
                {
                    var field = x.Field;
                    if (x.ExpandDateTime)
                    {
                        var f = DefaultDateTimePartNames.GetField(x.Method, x.Field, out var ok);
                        if (ok)
                        {
                            field = f;
                        }
                    }
                    return new TriggerField(field, $"NEW.{helper.Wrap(field)}", field, x.Type, x.RawFormat);
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

            var tableName = MD5Helper.ComputeHash(destTable + tableDef.Table);
            var insertName = string.Format(InsertQueryViewFormat, tableName);
            var updateName = string.Format(UpdateQueryViewFormat, tableName);

            script.Add(ViewHelper.Drop(insertName, SqlType));
            script.Add(ViewHelper.Drop(updateName, SqlType));
            if (ViewMode)
            {
                CompileOptions? opt = null;
                if (EffectTrigger)
                {
                    opt = CompileOptions.EffectJoin(effectTableName);
                    opt.EffectTable = effectTableName;
                    opt.IncludeEffectJoin = true;
                }
                script.Add(ViewHelper.Create(insertName, helper.CompileInsertSelect(destTable, tableDef, opt), SqlType));
                script.Add(ViewHelper.Create(updateName, helper.CompileUpdateSelect(destTable, tableDef, opt), SqlType));
            }
            return script;
        }
    }
}
