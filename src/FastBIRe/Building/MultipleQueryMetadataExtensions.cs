namespace FastBIRe.Building
{
    public static class MultipleQueryMetadataExtensions
    {
        public static IList<IQueryMetadata> SelectColumn(this IList<IQueryMetadata> metadata, string column, string? alias = null)
        {
            var select = new SelectMetadata(new ValueMetadata<string>(column, true));
            if (alias != null)
            {
                return metadata.Alias(select, alias);
            }
            metadata.Add(select);
            return metadata;
        }
        public static IList<IQueryMetadata> SelectMethod(this IList<IQueryMetadata> metadata, string methodName, string alias, params object[] args)
        {
            if (args == null)
            {
                return metadata.SelectMethod(methodName, alias, null);
            }
            var m = new IQueryMetadata[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                m[i] = new ValueMetadata<object>(args[i], true);
            }
            return metadata.SelectMethod(methodName, alias, m);
        }
        public static IList<IQueryMetadata> SelectMethod(this IList<IQueryMetadata> metadata, string methodName, string alias, params IQueryMetadata[]? args)
        {
            metadata.Add(new AliasMetadata(new SelectMetadata(new MethodMetadata(methodName, args)), alias));
            return metadata;
        }
        public static IList<IQueryMetadata> SortColumn(this IList<IQueryMetadata> metadata, string column, SortMode sort)
        {
            metadata.Add(new SortMetadata(new ValueMetadata(column, true), sort));
            return metadata;
        }
        public static IList<IQueryMetadata> Skip(this IList<IQueryMetadata> metadata, int skip)
        {
            metadata.Add(new SkipMetadata(skip));
            return metadata;
        }
        public static IList<IQueryMetadata> Limit(this IList<IQueryMetadata> metadata, int limit)
        {
            metadata.Add(new LimitMetadata(limit));
            return metadata;
        }
        public static IList<IQueryMetadata> GroupColumn(this IList<IQueryMetadata> metadata, string column)
        {
            metadata.Add(new GroupMetadata(new ValueMetadata(column, true)));
            return metadata;
        }
        public static IList<IQueryMetadata> GroupMethod(this IList<IQueryMetadata> metadata, string methodName, params IQueryMetadata[]? args)
        {
            metadata.Add(new GroupMetadata(new MethodMetadata(methodName, args)));
            return metadata;
        }
        public static IList<IQueryMetadata> GroupMethod(this IList<IQueryMetadata> metadata, string methodName, params object[] args)
        {
            if (args == null)
            {
                return metadata.GroupMethod(methodName, null);
            }
            var m = new IQueryMetadata[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                m[i] = new ValueMetadata<object>(args[i], true);
            }
            return metadata.GroupMethod(methodName, m);
        }

        public static IList<IQueryMetadata> Alias(this IList<IQueryMetadata> metadata, IQueryMetadata query, string alias)
        {
            metadata.Add(new AliasMetadata(query, alias));
            return metadata;
        }
    }
}
