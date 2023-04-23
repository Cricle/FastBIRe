using DatabaseSchemaReader.DataSchema;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;

namespace FastBIRe
{
    internal static class MD5Helper
    {
        private static readonly MD5 instance = MD5.Create();

        public static string ComputeHash(string input)
        {
            var buffer = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(instance.ComputeHash(buffer));
        }
    }
    public partial class MigrationService : DbMigration
    {
        public const string AutoTimeTriggerPrefx = "AGT_";
        public const string AutoGenIndexPrefx = "IXAG_";
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
                foreach (var item in needDrops)
                {
                    var sql = TableHelper.DropIndex(item, tableName);
                    res += await ExecuteNonQueryAsync(sql, token: token);
                }
            }
            return res;
        }
        public async Task<int> SyncIndexAutoAsync(string destTable, SourceTableDefine tableDef, string? sourceIdxName = null, string? destIdxName = null, Action<SyncIndexOptions>? optionDec = null, CancellationToken token = default)
        {
            sourceIdxName ??= $"{AutoGenIndexPrefx}{MD5Helper.ComputeHash(destTable)}";
            destIdxName ??= sourceIdxName + "_g";
            var groupColumns = tableDef.Columns.Where(x => x.IsGroup);
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
            var tb = Reader.Table(table);
            s.AddRange(tb.Triggers.Where(x => x.Name.StartsWith(AutoTimeTriggerPrefx)).Select(x => ComputeTriggerHelper.Instance.Drop(x.Name, x.TableName, SqlType))!);
            var str = RunMigration(s, table, news, oldRefs);
            str.AddRange(s);
            return str;
        }
        private void AddDatePart(DatabaseTable table,TableColumnDefine define)
        {
            var year = define.Field + DefaultDateTimePartNames.Year;
            var builder = GetColumnBuilder();
            var dateTimeType = builder.Type(DbType.DateTime);
            if (table.Columns.Any(x => x.Name == define.Field && x.DbDataType == dateTimeType))
            {

            }
        }
        public List<string> RunMigration(List<string> scripts, string table, IReadOnlyList<TableColumnDefine> news, IEnumerable<TableColumnDefine> oldRefs)
        {
            var groupNews = news.GroupBy(x => x.Field).Select(x => x.First()).ToList();
            var renames = groupNews.Join(oldRefs, x => x.Id, x => x.Id, (x, y) => new { Old = y, New = x });
            var migGen = DdlGeneratorFactory.MigrationGenerator();
            var tb = Reader.Table(table);
            var res= CompareWithModify(table, x =>
            {
                PrepareTable(x);
                var olds = x.Columns.Select(x =>
                {
                    var @new = news.FirstOrDefault(y => y.Field == x.Name);
                    if (@new != null)
                    {
                        x.Tag = true;
                    }
                    var old = oldRefs.FirstOrDefault(y => y.Field == x.Name);
                    if (old != null)
                    {
                        old.Type = x.DbDataType;
                        return old;
                    }
                    return null;
                }).Where(x => x != null).ToList();
                var expends = x.Columns.Where(x => x.Name.StartsWith("@")).ToList();
                foreach (var item in news)
                {
                    if (item.ExpandDateTime)
                    {
                        expends.RemoveAll(x => x.Name.StartsWith("@" + item.Field));
                    }
                }
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
                var rms = new HashSet<string>(x.Columns.Where(x => !x.Name.StartsWith("@")&&x.Tag == null && (NotRemoveColumns == null || !NotRemoveColumns.Contains(x.Name))).Select(x => x.Name));
                x.Columns.RemoveAll(x => rms.Contains(x.Name));
                foreach (var col in adds)
                {
                    x.AddColumn(col.Field, col.Type, x =>
                    {
                        x.Nullable = col.Nullable;
                    });
                }
            }).Execute();

            res.AddRange(tb.Triggers.Where(x => x.Name.StartsWith(AutoTimeTriggerPrefx)).Select(x => ComputeTriggerHelper.Instance.Drop(x.Name, x.TableName, SqlType))!);
            var computeField = news.Where(x => x.ExpandDateTime).ToList();
            if (computeField.Count != 0)
            {
                var key = AutoTimeTriggerPrefx + table;
                res.Add(ComputeTriggerHelper.Instance.Create(key, table, computeField.Select(x => new TriggerField(x.Field, x.ComputeDefine)), SqlType)!);
            }
            return res;
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
        public List<string> RunMigration(string destTable, SourceTableDefine tableDef, IEnumerable<TableColumnDefine> oldRefs)
        {
            var news = tableDef.Columns.GroupBy(x => x.Field).Select(x => x.First()).ToList();
            var table = tableDef.Table;
            var scripts = new List<string>();
            var migGen = DdlGeneratorFactory.MigrationGenerator();
            var script = RunMigration(scripts, tableDef.Table, tableDef.Columns, oldRefs);
            var effectTableName = destTable + EffectSuffix;
            var triggerName = "trigger_" + effectTableName;
            var triggerHelper = EffectTriggerHelper.Instance;

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
            else if (Reader.TableExists(effectTableName))
            {
                scripts.Add(TableHelper.CreateDropTable(effectTableName));
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
