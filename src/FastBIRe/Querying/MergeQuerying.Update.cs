using DatabaseSchemaReader.DataSchema;
using System.Text;

namespace FastBIRe.Querying
{
    public partial class MergeQuerying
    {
        private string GenerateColumnCompare(MergeQueryUpdateRequest request, ITableFieldLink x, string destTableAliasQuto, string tmpQuto)
        {
            if (request.SqlType == SqlType.PostgreSql)
            {
                if (x is DirectTableFieldLink direct)
                {
                    var leftField = request.DestTable.FindColumn(x.DestColumn.Name);
                    var rightField = request.SourceTable.FindColumn(direct.SourceColumn.Name);
                    if (leftField==null)
                        Throws.ThrowFieldNotFound(direct.DestColumn.Name, request.DestTable.Name);
                    if (rightField == null)
                        Throws.ThrowFieldNotFound(direct.SourceColumn.Name, request.SourceTable.Name);
                    if (string.Equals(leftField!.DbDataType , rightField!.DbDataType, StringComparison.OrdinalIgnoreCase))
                    {
                        //Type equals, raw check
                        return GenerateEqualsSql(null, null);
                    }
                }
                return GenerateEqualsSql("::VARCHAR","::VARCHAR");
            }
            return GenerateEqualsSql(null,null);

            string GenerateEqualsSql(string? leftCast,string? rightCast)
            {
                return $"({destTableAliasQuto}.{request.Wrap(x.DestColumn.Name)}{leftCast} != {tmpQuto}.{request.Wrap(x.DestColumn.Name)}{rightCast} OR ({destTableAliasQuto}.{request.Wrap(x.DestColumn.Name)} IS NULL AND {tmpQuto}.{request.Wrap(x.DestColumn.Name)} IS NOT NULL) OR ({destTableAliasQuto}.{request.Wrap(x.DestColumn.Name)} IS NOT NULL AND {tmpQuto}.{request.Wrap(x.DestColumn.Name)} IS NULL))";
            }
        }

        public string Update(MergeQueryUpdateRequest request)
        {
            var sourceTableAlias = SourceTableAlias;
            var destTableAlias = DestTableAlias;
            var effectTableAlias = EffectTableAlias;

            var sourceTableAliasQuto = request.Wrap(sourceTableAlias);
            var effectTableAliasQuto = request.Wrap(effectTableAlias);
            var destTableAliasQuto = request.Wrap(destTableAlias);

            var destTableQuto = request.Wrap(request.DestTable.Name);

            var sql = new StringBuilder();
            //UPDATE [dest] {AS destAlias}
            sql.Append($"UPDATE {destTableQuto}");
            if (request.SqlType.Ors(SqlType.PostgreSql, SqlType.MySql, SqlType.SQLite))
            {
                sql.AppendLine($" AS {destTableAliasQuto}");
            }
            else
            {
                sql.AppendLine();
            }
            var tmpQuto = request.Wrap("tmp");
            var setPrefx = request.SqlType == SqlType.MySql ? $"{destTableAliasQuto}.":string.Empty;
            var setString = string.Join(", ", request.NoGroupLinks.Select(x => $"{setPrefx}{request.Wrap(x.DestColumn.Name)} = {tmpQuto}.{request.Wrap(x.DestColumn.Name)}"));
            if (request.SqlType != SqlType.MySql)
            {
                sql.AppendLine($"SET {setString}");
            }

            var updateSelect = CompileUpdateSelect(request, SourceTableAlias);
            var destGroupOn = string.Join(" AND ", request.GroupLinks.Select(x => $"{destTableAliasQuto}.{request.Wrap(x.DestColumn.Name)} = {tmpQuto}.{request.Wrap(x.DestColumn.Name)}"));
            var destGroupCheck = string.Join(" OR ", request.NoGroupLinks.Where(x => !request.IgnoreCompareFields.Contains(x.DestColumn.Name)).Select(x => GenerateColumnCompare(request,x,destTableAliasQuto,tmpQuto)));

            var updateFrom = CompileUpdateFrom(request, updateSelect, destGroupOn, destGroupCheck, tmpQuto);
            sql.AppendLine(updateFrom);

            if (request.SqlType == SqlType.MySql)
            {
                sql.AppendLine($"SET {setString}");
            }

            return sql.ToString();
        }
        private string CompileUpdateFrom(MergeQueryUpdateRequest request, string updateSelect, string destGroupOn, string destGroupCheck, string tmpQuto)
        {
            switch (request.SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $@"FROM {request.Wrap(request.DestTable.Name)} AS {request.Wrap(DestTableAlias)} INNER JOIN (
{updateSelect}
) AS {tmpQuto} ON {destGroupOn}
AND (
{destGroupCheck}
)";
                case SqlType.MySql:
                    return $@"INNER JOIN (
{updateSelect}
) AS {tmpQuto} ON {destGroupOn}
AND(
{destGroupCheck}
)";
                case SqlType.SQLite:
                    return $@"FROM (
{updateSelect}
) AS {tmpQuto}
WHERE {destGroupOn}
AND (
{destGroupCheck}
)";
                case SqlType.PostgreSql:
                    return $@"FROM (
{updateSelect}
) AS {tmpQuto}
WHERE {destGroupOn}
AND(
{destGroupCheck}
)";
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    throw new NotSupportedException($"Only support sqlserver/mysql/sqlite/postgresql");
            }
        }
        private string CompileUpdateSelect(MergeQueryUpdateRequest request, string? sourceTableAlias)
        {
            //View select?
            if (request.UseView)
            {
                var viewFieldSelects = string.Join(", ", request.AllLinks.Select(x => request.Wrap(x.DestColumn.Name)));
                return $"SELECT {viewFieldSelects} FROM {request.Wrap(request.ViewName)} ";
            }
            var rawSelects = request.AllLinks.Select(x => $"{x.FormatExpression(request.SqlType, sourceTableAlias)} AS {request.Wrap(x.DestColumn.Name)}");
            var rawSelectString = string.Join(", ", rawSelects);//group and aggr

            //Join where
            var where = string.Empty;
            if (!string.IsNullOrEmpty(request.AdditionWhere))
            {
                where = $" WHERE {request.AdditionWhere}";
            }

            //Groups
            var groups = request.GroupLinks.Select(x => x.FormatExpression(request.SqlType, sourceTableAlias));
            var groupString = string.Join(", ", groups);
            var tableRef = GetTableRef(request);
            return @$"SELECT {rawSelectString}
FROM {tableRef}
{where}
GROUP BY {groupString}";
        }
    }
}
