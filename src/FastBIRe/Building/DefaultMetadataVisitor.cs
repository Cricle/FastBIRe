using DatabaseSchemaReader.DataSchema;
using FastBIRe.Functions;
using FastBIRe.Wrapping;
using System.Linq.Expressions;
using System.Text;

namespace FastBIRe.Building
{
    public class SqlBuilder
    {
        public SqlBuilder()
        {
            Metadatas = new MultipleQueryMetadata();
        }

        public MultipleQueryMetadata Metadatas { get; }

        public SqlBuilder OrderBy(string value, SortMode mode,bool quto = true)
        {
            return OrderBy(new ValueMetadata(value, quto), mode);
        }
        public SqlBuilder OrderBy(IQueryMetadata metadata,SortMode mode)
        {
            Metadatas.Add(new SortMetadata(metadata, mode));
            return this;
        }
        public SqlBuilder Group(string value,bool quto = true)
        {
            return Group(new ValueMetadata(value, quto));
        }
        public SqlBuilder Group(params IQueryMetadata[] metadatas)
        {
            Metadatas.Add(new GroupMetadata(metadatas));
            return this;
        }
        public SqlBuilder Where(params IQueryMetadata[] metadatas)
        {
            Metadatas.Add(new FilterMetadata(metadatas));
            return this;
        }
        public SqlBuilder Select(string value,string? alias=null, bool quto=true)
        {
            IQueryMetadata m = new ValueMetadata(value, quto);
            if (!string.IsNullOrEmpty(alias))
            {
                m = new AliasMetadata(m, alias!);
            }
            return Select(m);
        }
        public SqlBuilder Select(params IQueryMetadata[] metadatas)
        {
            Metadatas.Add(new SelectMetadata(metadatas));
            return this;
        }
        public SqlBuilder From(string value, bool quto = true)
        {
            return From(new ValueMetadata(value,quto));
        }
        public SqlBuilder From(IQueryMetadata metadata)
        {
            Metadatas.Add(new FromMetadata(metadata));
            return this;
        }
        public SqlBuilder Offset(int offset)
        {
            Metadatas.RemoveAll(x => x is SkipMetadata);
            Metadatas.Add(new SkipMetadata(offset));
            return this;
        }
        public SqlBuilder Limit(int limit)
        {
            Metadatas.RemoveAll(x => x is LimitMetadata);
            Metadatas.Add(new LimitMetadata(limit));
            return this;
        }

        public string ToSql(SqlType sqlType)
        {
            var visitor = new SqlMetadataVisitor(sqlType);
            visitor.Visit(Metadatas, visitor.CreateContext(Metadatas));
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
    public abstract class DefaultMetadataVisitor<T> : MetadataVisitor<T>
        where T : DefaultQueryContext
    {
        public DefaultMetadataVisitor(IList<object?> args)
        {
            Args = args;
        }
        public DefaultMetadataVisitor()
            : this(new List<object?>(0))
        {
        }

        public IList<object?> Args { get; }

        public int Add(object? val)
        {
            Args.Add(val);
            return Args.Count - 1;
        }

        public abstract T CreateContext(IQueryMetadata metadata);

        private void AddIfNotNull(List<string> lst,string? exp)
        {
            if (!string.IsNullOrEmpty(exp))
            {
                lst.Add(exp!);
            }
        }

        public override void VisitSelect(SelectMetadata value, T context)
        {
            var selects = new List<string>(value.Target.Count);
            for (int i = 0; i < value.Target.Count; i++)
            {
                var target = value.Target[i];
                var ctx = CreateContext(target);
                Visit(target, ctx);
                AddIfNotNull(selects, ctx.Expression);
            }
            OnVisitSelect(value, context, selects);
        }
        protected abstract void OnVisitSelect(SelectMetadata value, T context, List<string> selects);

        public override void VisitGroup(GroupMetadata value, T context)
        {
            var groups = new List<string>(value.Target.Count);
            for (int i = 0; i < value.Target.Count; i++)
            {
                var item = value.Target[i];
                var ctx = CreateContext(item);
                Visit(item, ctx);
                AddIfNotNull(groups, ctx.Expression);
            }
            OnVisitGroup(value, context, groups);
        }
        protected abstract void OnVisitGroup(GroupMetadata value, T context, List<string> groups);

        public override void VisitFilter(FilterMetadata value, T context)
        {
            for (int i = 0; i < value.Count; i++)
            {
                var item = value[i];
                var ctx = CreateContext(item);
                Visit(item, ctx);
                OnVisitFilter(value, item, ctx);
            }
        }
        protected abstract void OnVisitFilter(FilterMetadata value, IQueryMetadata metadata, T context);

        public override void VisitBinary(BinaryMetadata value, T context)
        {
            var leftCtx = CreateContext(value.Left);
            var rightCtx = CreateContext(value.Right);
            Visit(value.Left, leftCtx);
            Visit(value.Right, rightCtx);
            OnVisitBinary(value, context, leftCtx, rightCtx);
        }
        protected virtual void OnVisitBinary(BinaryMetadata value, T context, T leftContext, T rightContext)
        {
            var token = value.GetToken();
            context.Expression += leftContext.Expression + token + rightContext.Expression;
        }

        public override void VisitUnary(UnaryMetadata value, T context)
        {
            var token = value.GetToken();
            var ctx = CreateContext(value.Left);
            Visit(value.Left, ctx);
            if (value.IsPreCombine())
            {
                context.Expression += token + ctx.Expression;
            }
            else
            {
                context.Expression += ctx.Expression + token;
            }
        }
        public override void VisitWrapper(WrapperMetadata value, T context)
        {
            var targetCtx = CreateContext(value.Target);
            Visit(value.Target, targetCtx);
            context.Expression += value.Left + targetCtx.Expression + value.Right;
        }
        public override void VisitAlias(AliasMetadata value, T context)
        {
            var ctx = CreateContext(value.Target);
            Visit(value.Target, ctx);
            context.Expression += $"{ctx.Expression} as {value.Alias}";
        }

        public override void VisitValue(ValueMetadata value, T context)
        {
            if (context.MustQuto || value.Quto)
            {
                context.Expression += value.Value?.ToString() ?? "null";
            }
            else
            {
                context.Expression += "@" + Add(value.Value);
            }
        }
        public override void VisitMethod(MethodMetadata method, T context)
        {
            var args = new string?[method.Args != null ? method.Args.Count : 0];
            for (int i = 0; i < args.Length; i++)
            {
                var arg = method.Args![i];
                var ctx = CreateContext(arg);
                Visit(arg, ctx);
                args[i] = ctx.Expression;
            }
            OnVisitMethod(method, context, args);
        }
        protected abstract void OnVisitMethod(MethodMetadata method, T context, string?[] args);

        public override void VisitSort(SortMetadata value, T context)
        {
            var ctx = CreateContext(value.Target);
            Visit(value.Target, ctx);
            OnVisitSort(value, ctx);
        }
        protected abstract void OnVisitSort(SortMetadata method, T context);

        public override void VisitFrom(FromMetadata value, T context)
        {
            var ctx = CreateContext(value.From);
            Visit(value.From, ctx);
            OnVisitFrom(value, ctx);
        }
        protected abstract void OnVisitFrom(FromMetadata method, T context);
    }
}
