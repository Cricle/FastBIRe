using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.Timing;
using System.Data;

namespace FastBIRe.AAMode
{
    public class TableExpandTimeAAModelHelper : IModeHelper<TableExpandTimeRequest>
    {
        public TableExpandTimeAAModelHelper(ITimeExpandHelper timeExpandHelper)
        {
            TimeExpandHelper = timeExpandHelper;
        }

        public ITimeExpandHelper TimeExpandHelper { get; }

        public void Apply(DatabaseReader reader, TableExpandTimeRequest request)
        {
            //Read the origin table
            var rawTable = reader.Table(request.TableName);
            var changedTable = reader.Table(request.TableName);
            var results = request.Columns.SelectMany(x => TimeExpandHelper.Create(x, request.TimeTypes)).ToList();
            var affectColumns=new List<TimeExpandResult>();
            foreach (var item in results)
            {
                var dbType = reader.FindDataTypesByDbType(DbType.DateTime);
                var col = changedTable.Columns.FirstOrDefault(x => x.Name == item.Name);
                if (col == null)
                {
                    col = new DatabaseColumn
                    {
                        Name = item.Name,
                        Nullable = true
                    };
                    col.SetType(dbType);
                    changedTable.AddColumn(col);
                    affectColumns.Add(item);
                }
                else if (!string.Equals(dbType, col.DbDataType, StringComparison.OrdinalIgnoreCase))
                {
                    col.SetType(dbType);
                    affectColumns.Add(item);
                }
            }
            var cmp = CompareSchemas.FromTable(reader.DatabaseSchema.ConnectionString, reader.SqlType!.Value, rawTable, changedTable).ExecuteResult();
            request.AddScripts(cmp.Select(x => x.Script));
            if (request.WithDataMigration&&cmp.Count != 0)
            {
                //Some column migrate
                var sqlType = reader.SqlType!.Value;
                foreach (var item in affectColumns)
                {
                    request.Scripts.Add($"UPDATE {sqlType.Wrap(request.TableName)} SET {sqlType.Wrap(item.Name)} = {item.FormatExpression(string.Empty)}");
                }
            }
        }
    }
}
