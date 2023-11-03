using DatabaseSchemaReader.DataSchema;
using FastBIRe.Functions;
using FastBIRe.Wrapping;
using System.Linq.Expressions;
using System.Text;

namespace FastBIRe.Building
{
    public static class SqlMetadataVisitorToExtensions
    {
        public static string ToSql(this IQueryMetadata metadata,SqlType sqlType)
        {
            var visitor = new SqlMetadataVisitor(sqlType);
            visitor.Visit(metadata, visitor.CreateContext(metadata));
            return visitor.ToSql();
        }
    }
    public class SqlMetadataVisitor : DefaultMetadataVisitor<SqlQueryContext>
    {
        private readonly List<string> selects=new List<string>();
        private readonly List<string> wheres = new List<string>();
        private readonly List<string> froms = new List<string>();
        private readonly List<string> groups= new List<string>();
        private readonly List<string> orders = new List<string>();
        private int? offset;
        private int? limit;

        public SqlMetadataVisitor(SqlType sqlType)
        {
            SqlType = sqlType;
            Escaper = sqlType.GetEscaper();
            FunctionMapper = FunctionMapper.Get(sqlType)!;
            TableHelper = sqlType.GetTableHelper()!;
        }

        public SqlType SqlType { get; }

        public IEscaper Escaper { get; }

        public FunctionMapper FunctionMapper { get; }

        public TableHelper TableHelper { get; }

        public string ToSql()
        {
            var s = new StringBuilder($"SELECT {string.Join(",",selects)} ");
            if (froms.Count!=0)
            {
                s.Append($"FROM {string.Join(",", froms)} ");
            }
            if (wheres.Count != 0)
            {
                s.Append($"WHERE {string.Join(",", wheres)} ");
            }
            if (groups.Count != 0)
            {
                s.Append($"GROUP BY {string.Join(",", groups)} ");
            }
            if (orders.Count != 0)
            {
                s.Append($"ORDER BY {string.Join(",", orders)} ");
            }
            s.Append(TableHelper.Pagging(offset,limit));
            return s.ToString();
        }

        public override SqlQueryContext CreateContext(IQueryMetadata metadata)
        {
            return new SqlQueryContext();
        }
        public override void VisitRaw(RawMetadata value, SqlQueryContext context)
        {
            context.Expression = value.Raw;
        }

        protected override void OnVisitFilter(FilterMetadata value, IQueryMetadata metadata, SqlQueryContext context)
        {
            wheres.Add(context.Expression);
        }

        protected override void OnVisitGroup(GroupMetadata value, SqlQueryContext context, List<string> groups)
        {
           this.groups.AddRange(groups);
        }
        protected override void OnVisitBinary(BinaryMetadata value, SqlQueryContext context, SqlQueryContext leftContext, SqlQueryContext rightContext)
        {
            var tk = " " + value.GetToken() + " ";
            if (value.ExpressionType == ExpressionType.Equal)
            {
                tk = " = ";
            }
            else if (value.ExpressionType == ExpressionType.OrElse)
            {
                tk = " OR ";
            }
            else if (value.ExpressionType == ExpressionType.AndAlso)
            {
                tk = " AND ";
            }
            if (string.Equals(rightContext.Expression, "null", StringComparison.OrdinalIgnoreCase))
            {
                if (value.ExpressionType == ExpressionType.Equal)
                {
                    tk = " IS NULL";
                }
                else
                {
                    tk = " IS NOT NULL";
                }
                context.Expression += leftContext.Expression + tk;
            }
            else
            {
                context.Expression += leftContext.Expression + tk + rightContext.Expression;
            }
        }
        protected override void OnVisitMethod(MethodMetadata method, SqlQueryContext context, string?[] args)
        {
            var fun = method.Function ?? (Enum.TryParse<SQLFunctions>(method.Method, out var parsed) ? parsed : null);
            if (fun != null)
            {
                context.Expression = FunctionMapper.Map(fun.Value, args);
                return;
            }
            throw new NotSupportedException(method.Method);
        }

        protected override void OnVisitSelect(SelectMetadata value, SqlQueryContext context, List<string> selects)
        {
            this.selects.AddRange(selects);
        }
        public override void VisitAlias(AliasMetadata value, SqlQueryContext context)
        {
            var ctx = CreateContext(value.Target);
            Visit(value.Target, ctx);
            context.Expression += $"{ctx.Expression} AS {Escaper.Quto(value.Alias)}";
        }
        public override void VisitValue(ValueMetadata value, SqlQueryContext context)
        {
            if (context.MustQuto || value.Quto)
            {
                context.Expression += Escaper.Quto(value.Value?.ToString());
            }
            else
            {
                context.Expression += Escaper.WrapValue(value.Value);
            }
        }
        protected override void OnVisitSort(SortMetadata method, SqlQueryContext context)
        {
            var dir = method.SortMode == SortMode.Desc ? "DESC" : "ASC";
            orders.Add($"{context.Expression} {dir}");
        }
        public override void VisitSkip(SkipMetadata value, SqlQueryContext context)
        {
            offset = value.Value;
        }
        public override void VisitLimit(LimitMetadata value, SqlQueryContext context)
        {
            limit = value.Value;
        }
        protected override void OnVisitFrom(FromMetadata method, SqlQueryContext context)
        {
            froms.Add(context.Expression);
        }
    }
}
