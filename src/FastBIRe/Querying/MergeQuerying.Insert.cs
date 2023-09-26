using DatabaseSchemaReader.DataSchema;
using System.Text;

namespace FastBIRe.Querying
{
    public partial class MergeQuerying
    {
        public string Insert(MergeQueryInsertRequest request)
        {
            var sql = new StringBuilder();
            //Make the insert field string
            var selectFieldLists = request.AllLinks.Select(x => request.Wrap(x.DestColumn.Name));
            var selectFieldString = string.Join(", ", selectFieldLists);
            sql.AppendLine($"INSERT INTO {request.Wrap(request.SourceTable.Name)}({selectFieldString})");
            if (request.UseView)
            {
                //By select view, use sequential fields
                sql.AppendLine($"SELECT {selectFieldString} FROM {request.Wrap(request.ViewName)}");
            }
            else
            {
                //Raw select
                AppendInsertSelect(sql, request);
            }
            return sql.ToString();//TODO: make the result happy
        }
        protected virtual string GetTableRef(MergeQueryRequest request)
        {
            var sourceTableAlias = SourceTableAlias;
            var sourceTableAliasQuto = request.Wrap(sourceTableAlias);
            var sourceTableAsExp = $"{request.Wrap(request.SourceTable.Name)} AS {sourceTableAliasQuto}";
            if (request.UseEffectTable)
            {
                if (request.EffectTable == null)
                {
                    throw new ArgumentException($"The UseEffectTable = true EffectTable must not null");
                }
                var destTableAlias = DestTableAlias;
                var effectTableAlias = EffectTableAlias;

                var effectTableAliasQuto = request.Wrap(effectTableAlias);
                var sql = new StringBuilder();
                //In sqlserver postgresql sqlite, use exists expression
                var effectQuto = request.Wrap(request.EffectTable.Name);
                var effectAsExp = $"{effectQuto} AS {effectTableAliasQuto}";
                var effectWhere = string.Join(" AND ", request.EffectTable.Columns.Select(x => $"({sourceTableAliasQuto}.{request.Wrap(x.Name)} = {effectTableAliasQuto}.{request.Wrap(x.Name)} OR ({sourceTableAliasQuto}.{request.Wrap(x.Name)} IS NULL AND {effectTableAliasQuto}.{request.Wrap(x.Name)} IS NULL))"));
                if (request.SqlType.Ors(SqlType.SqlServer, SqlType.SqlServerCe, SqlType.PostgreSql, SqlType.SQLite))
                {
                    sql.AppendLine($"{sourceTableAsExp} WHERE EXISTS ( ");
                    sql.AppendLine($"SELECT 1 FROM {effectAsExp} WHERE {effectWhere}");
                    sql.AppendLine(")");
                }
                else
                {
                    sql.AppendLine($"{effectAsExp} INNER JOIN {sourceTableAsExp} ON {effectWhere}");
                }
                return sql.ToString();
            }
            return sourceTableAsExp;
        }
        protected virtual void AppendInsertSelect(StringBuilder sql, MergeQueryInsertRequest request)
        {
            //Actualy the column replace must user defined, so expand field must not auto redired this merge
            var sourceTableAlias = SourceTableAlias;
            var destTableAlias = DestTableAlias;
            var effectTableAlias = EffectTableAlias;

            var sourceTableAliasQuto = request.Wrap(sourceTableAlias);
            var effectTableAliasQuto = request.Wrap(effectTableAlias);
            var links = request.AllLinks.Select(x => x.FormatSql(request.SqlType, sourceTableAlias)).ToList();
            var linkStrings = string.Join(", ", links);
            sql.AppendLine($"SELECT {linkStrings} ");
            //From is redirect to effect table
            if (request.SqlType == SqlType.SQLite)
            {
                // Sqlite needs SELECT *
                sql.Append("FROM ( SELECT * ");
            }
            var sourceTableAsExp = $"{request.Wrap(request.SourceTable.Name)} AS {sourceTableAliasQuto}";
            sql.Append(" FROM ");
            //Is use effect table?
            sql.AppendLine(GetTableRef(request));
            var resultQuto = request.Wrap("result");
            var destTableQuto = request.Wrap(request.DestTable.Name);
            var destTableAliasQuto = request.Wrap(destTableAlias);
            var destTableAsExp = $"{destTableQuto} AS {destTableAliasQuto}";
            var groupLinkWhere = request.GroupLinks.Select(x => $"{x.FormatExpression(request.SqlType, sourceTableAlias)} = {destTableAliasQuto}.{request.Wrap(x.DestColumn.Name)}").ToList();
            var groupLinkStrings = string.Join(" AND ", groupLinkWhere);
            if (request.SqlType == SqlType.SQLite)
            {
                sql.AppendLine($") AS {resultQuto} LEFT JOIN {destTableAsExp} ON {groupLinkStrings}");
                sql.AppendLine($"WHERE {string.Join(" AND ", request.NoGroupLinks.Select(x => $"{request.Wrap(x.DestColumn.Name)} IS NULL"))}");
            }
            else
            {
                if (request.SqlType.Ors(SqlType.SqlServer, SqlType.SqlServerCe, SqlType.PostgreSql, SqlType.SQLite) && request.UseEffectTable)
                {
                    sql.Append(" AND ");
                }
                else
                {
                    sql.Append(" WHERE ");
                }
                var hasAdditionWhere = !string.IsNullOrEmpty(request.AdditionWhere);
                if (hasAdditionWhere)
                {
                    sql.Append(request.AdditionWhere);
                    sql.Append(" AND ");
                }
                sql.AppendLine($"NOT EXISTS ( SELECT 1 AS {request.Wrap("tmp")} FROM {destTableAsExp} WHERE {groupLinkStrings} )");
            }
            var grouping = string.Join(", ", request.GroupLinks.Select(x => x.FormatExpression(request.SqlType, sourceTableAlias)));
            sql.AppendLine($"GROUP BY {grouping}");
        }
    }
}
