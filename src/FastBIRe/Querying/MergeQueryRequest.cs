using DatabaseSchemaReader.DataSchema;
using System.Runtime.CompilerServices;

namespace FastBIRe.Querying
{
    /// <summary>
    /// The merge query request
    /// </summary>
    public abstract record class MergeQueryRequest
    {
        protected MergeQueryRequest(SqlType sqlType, DatabaseTable sourceTable, DatabaseTable destTable, IReadOnlyList<ITableFieldLink> noGroupLinks, IReadOnlyList<ITableFieldLink> groupLinks)
        {
            SqlType = sqlType;
            NoGroupLinks = noGroupLinks;
            SourceTable = sourceTable;
            DestTable = destTable;
            GroupLinks = groupLinks;
            AllLinks = groupLinks.Concat(noGroupLinks).ToList();

            if (IsLinkDestColumnDuplice(out var duplicateName))
            {
                throw new ArgumentException($"The link dest column name {duplicateName} has duplicated");
            }
        }
        /// <summary>
        /// The sql type
        /// </summary>
        public SqlType SqlType { get; }

        /// <summary>
        /// The source table
        /// </summary>
        public DatabaseTable SourceTable { get; }
        /// <summary>
        /// The dest table
        /// </summary>
        public DatabaseTable DestTable { get; }

        /// <summary>
        /// Gets or sets if <see cref="UseEffectTable"/> is <see langword="true"/>, provide the effect table to make result set limit
        /// </summary>
        public DatabaseTable? EffectTable { get; set; }
        /// <summary>
        /// Gets the link for source table and dest table
        /// </summary>
        public IReadOnlyList<ITableFieldLink> NoGroupLinks { get; }
        /// <summary>
        /// Gets the set provide source table group field link
        /// </summary>
        public IReadOnlyList<ITableFieldLink> GroupLinks { get; }
        /// <summary>
        /// Gets all links
        /// </summary>
        public IReadOnlyList<ITableFieldLink> AllLinks { get; }
        /// <summary>
        /// Gets or sets compile query use effect table
        /// </summary>
        public bool UseEffectTable { get; set; }
        /// <summary>
        /// Gets or sets if <see cref="UseView"/> is <see langword="true"/>, provide the view name for query
        /// </summary>
        public string? ViewName { get; set; }
        /// <summary>
        /// Gets or sets compile query use view
        /// </summary>
        public bool UseView { get; set; }

        /// <summary>
        /// Addition where
        /// </summary>
        public string? AdditionWhere { get; set; }

        /// <summary>
        /// Use <see cref="SqlType"/> to wrap input
        /// </summary>
        /// <param name="input">The input if <see langword="null"/> return empty string</param>
        /// <returns>Wrap result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Wrap(string? input)
        {
            if (input == null)
            {
                return string.Empty;
            }
            return SqlType.Wrap(input);
        }
        /// <summary>
        /// Check the link dest not duplicate
        /// </summary>
        /// <param name="name">Duplicate column name</param>
        /// <returns>Dest column is duplicate</returns>
        protected virtual bool IsLinkDestColumnDuplice(out string? name)
        {
            name = AllLinks.GroupBy(x => x.DestColumn.Name).Where(x => x.Skip(1).Any()).Select(x=>x.Key).FirstOrDefault();
            return !string.IsNullOrEmpty(name);
        }
    }
}
