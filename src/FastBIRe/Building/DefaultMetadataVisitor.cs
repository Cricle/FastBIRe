namespace FastBIRe.Building
{
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

        private void AddIfNotNull(List<string> lst, string? exp)
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
