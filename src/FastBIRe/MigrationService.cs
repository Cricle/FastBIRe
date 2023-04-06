using DatabaseSchemaReader.DataSchema;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace FastBIRe
{
    public class MigrationService: DbMigration
    {
        public const string DefaultEffectSuffix = "_effect";

        private string effectSuffix = DefaultEffectSuffix;

        public MigrationService(DbConnection connection) : base(connection)
        {
        }

        public MigrationService(DbConnection connection, string database) : base(connection, database)
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
        public async Task<int> SyncIndexAsync(string destTable, SourceTableDefine tableDef, string? sourceIdxName = null, string? destIdxName = null)
        {
            var res = await SyncIndexAsync(new SyncIndexOptions
            {
                Table = tableDef.Table,
                Columns = tableDef.Columns.Where(x => x.IsGroup).Select(x => x.Field).ToArray(),
                IndexName = sourceIdxName ?? $"IDX_s_{destTable}"
            });
            res += await SyncIndexAsync(new SyncIndexOptions
            {
                Table = destTable,
                Columns = tableDef.Columns.Where(x => x.IsGroup).Select(x => x.DestColumn.Field).ToArray(),
                IndexName = destIdxName ?? $"IDX_{destTable}"
            });
            return res;
        }
        public string RunMigration(string destTable,SourceTableDefine tableDef, IEnumerable<SourceTableColumnDefine> oldRefs)
        {
            var news = tableDef.Columns;
            var table = tableDef.Table;
            var renames = news.Join(oldRefs, x => x.Id, x => x.Id, (x, y) => new { Old = y, New = x });
            var otherScripts = new StringBuilder();
            var migGen = DdlGeneratorFactory.MigrationGenerator();
            var script = CompareWithModify(table, x =>
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
                        col.Name = item.New.Field;
                        var s = migGen.RenameColumn(x, col, item.Old.Field);
                        otherScripts.AppendLine(s);
                        col.Name = item.Old.Field;
                    }
                }
                foreach (var item in news)
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
                            col.DbDataType = col.DbDataType.Remove(7,1);
                        }
                        var leftType = col.DbDataType;
                        var rightType = item.Type!;
                        if (SqlType == SqlType.PostgreSql)
                        {
                            leftType = leftType.ToLower().Replace("decimal", "numeric");
                            rightType = rightType.ToLower().Replace("decimal", "numeric");
                        }
                        if (!string.Equals(leftType,rightType, StringComparison.OrdinalIgnoreCase))
                        {
                            col.DbDataType = item.Type;
                        }
                    }
                }
                var adds = news.Where(n => !olds.Any(y => y.Id == n.Id)).ToList();
                var rms = new HashSet<string>(olds.Where(o => !news.Any(y => y.Id == o.Id)).Select(x => x.Field));
                x.Columns.RemoveAll(x => rms.Contains(x.Name));
                foreach (var col in adds)
                {
                    x.AddColumn(col.Field, col.Type, x =>
                    {
                        x.Nullable = col.Nullable;
                    });
                }
            }).Execute();
            if (tableDef.Columns.Any(x => x.IsGroup))
            {
                var groupColumns = tableDef.Columns.Where(x => x.IsGroup).ToList();
                var effectTableName = destTable + EffectSuffix;
                var refTable = Reader.Table(effectTableName);
                var hasTale = false;
                if (refTable != null)
                {
                    if (refTable.Columns.Count != groupColumns.Count ||
                        !refTable.Columns.Select(x => x.Name).SequenceEqual(groupColumns.Select(x => x.Field)) ||
                        refTable.PrimaryKey == null ||
                        refTable.PrimaryKey.Columns.Count != refTable.Columns.Count ||
                        !refTable.PrimaryKey.Columns.SequenceEqual(refTable.Columns.Select(x => x.Name))||
                        !groupColumns.Select(x=>x.Type).SequenceEqual(refTable.Columns.Select(x => x.DbDataType)))
                    {
                        otherScripts.AppendLine(migGen.DropTable(refTable));
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
                    otherScripts.AppendLine(migGen.AddTable(refTable));
                }
            }
            return script + "\n" + otherScripts;
        }
    }

}
