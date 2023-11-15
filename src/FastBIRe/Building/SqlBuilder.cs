using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Building
{
    public class SqlBuilder
    {
        public SqlBuilder()
        {
            Metadatas = new MultipleQueryMetadata();
        }

        public MultipleQueryMetadata Metadatas { get; }

        public SqlBuilder OrderBy(string value, SortMode mode, bool quto = true)
        {
            return OrderBy(new ValueMetadata(value, quto), mode);
        }
        public SqlBuilder OrderBy(IQueryMetadata metadata, SortMode mode)
        {
            Metadatas.Add(new SortMetadata(metadata, mode));
            return this;
        }
        public SqlBuilder Group(string value, bool quto = true)
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
        public SqlBuilder Select(string value, string? alias = null, bool quto = true)
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
            return From(new ValueMetadata(value, quto));
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
}
