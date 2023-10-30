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
                }
                else if (!string.Equals(dbType, col.DbDataType, StringComparison.OrdinalIgnoreCase))
                {
                    col.SetType(dbType);
                }
            }
            var cmp = CompareSchemas.FromTable(reader.DatabaseSchema.ConnectionString, reader.SqlType!.Value, rawTable, changedTable).ExecuteResult();
            request.AddScripts(cmp.Select(x => x.Script));
            if (cmp.Count != 0)
            {
                //All column migrate
                var sqlType = reader.SqlType!.Value;
                foreach (var item in results)
                {
                    request.Scripts.Add($"UPDATE {sqlType.Wrap(request.TableName)} SET {sqlType.Wrap(item.Name)} = {item.FormatExpression(string.Empty)}");
                }
            }
        }
    }
}
