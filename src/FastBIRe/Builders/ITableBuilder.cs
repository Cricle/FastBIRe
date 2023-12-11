using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using System.Data;

namespace FastBIRe.Builders
{
    public interface ITableBuilder : ISqlTableBuilder
    {
        DatabaseTable Table { get; }

        ITableBuilder Config(Action<DatabaseTable> configuration);

        ITableColumnBuilder GetColumnBuilder(string name);
    }
    public static class TableBuilderSetExtensions
    {
        public static string ToCreateTablesSql(this ITableProvider provider)
        {
            var schema = new DatabaseSchema(string.Empty, provider.SqlType);
            foreach (var item in provider)
            {
                schema.AddTable(item);
            }
            var ddl = new DdlGeneratorFactory(provider.SqlType).AllTablesGenerator(schema);
            return ddl.Write();
        }
        public static ITablesProviderBuilder ConfigTable(this ITablesProviderBuilder builder, string name, Action<ITableBuilder> config)
        {
            var tableBuilder = builder.GetTableBuilder(name);
            config(tableBuilder);
            return builder;
        }
        private static bool IsString(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                case DbType.String:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return true;
                default:
                    return false;
            }
        }
        public static ITableBuilder StringColumn(this ITableBuilder builder,
            string name,
            int length,
            string? computedDefinition = null,
            bool nullable = true,
            int? scale = null,
            int? precision = null,
            object? id = null,
            bool isAutoNumber = false,
            bool identityByDefault = false,
            int? identitySeed = null,
            int? identityIncrement = null,
            string? defaultValue = null)
        {
            return Column(builder, name, DbType.String, computedDefinition, nullable, length, scale, precision, id, isAutoNumber, identityByDefault, identitySeed, identityIncrement, defaultValue);
        }
        public static ITableBuilder DateTimeColumn(this ITableBuilder builder,
            string name,
            int? length=null,
            string? computedDefinition = null,
            bool nullable = true,
            int? scale = null,
            int? precision = null,
            object? id = null,
            bool isAutoNumber = false,
            bool identityByDefault = false,
            int? identitySeed = null,
            int? identityIncrement = null,
            string? defaultValue = null)
        {
            return Column(builder, name, DbType.DateTime, computedDefinition, nullable, length, scale, precision, id, isAutoNumber, identityByDefault, identitySeed, identityIncrement, defaultValue);
        }
        public static ITableBuilder Column(this ITableBuilder builder, string name,
            Action<DatabaseColumn> typeHandle,
            string? computedDefinition = null,
            bool nullable = true,
            int? length = null,
            int? scale = null,
            int? precision = null,
            object? id = null,
            bool isAutoNumber = false,
            bool identityByDefault = false,
            int? identitySeed = null,
            int? identityIncrement = null,
            string? defaultValue = null)
        {
            var columnBuilder = builder.GetColumnBuilder(name);
            columnBuilder.Config(col =>
            {
                col.ComputedDefinition = computedDefinition;
                col.Nullable = nullable;
                col.Length = length;
                col.Scale = scale;
                col.Precision = precision;
                col.Id = id;
                typeHandle(col);
                col.Id = null;
                if (isAutoNumber)
                {
                    col.AddIdentity();
                    col.IdentityDefinition.IdentityByDefault = identityByDefault;
                    if (identitySeed != null)
                    {
                        col.IdentityDefinition.IdentitySeed = identitySeed.Value;
                    }
                    if (identityIncrement != null)
                    {
                        col.IdentityDefinition.IdentityIncrement = identityIncrement.Value;
                    }
                }
                col.DefaultValue = defaultValue;
            });
            return builder;
        }

        public static ITableBuilder Column(this ITableBuilder builder, string name,
            string type,
            string? computedDefinition = null,
            bool nullable = true,
            int? length = null,
            int? scale = null,
            int? precision = null,
            object? id = null,
            bool isAutoNumber = false,
            bool identityByDefault = false,
            int? identitySeed = null,
            int? identityIncrement = null,
            string? defaultValue = null)
        {
            return Column(builder, name, col => col.SetType(type), computedDefinition, nullable, length, scale, precision, id, isAutoNumber, identityByDefault, identitySeed, identityIncrement, defaultValue);
        }
        public static ITableBuilder Column(this ITableBuilder builder, string name,
            DbType type,
            string? computedDefinition = null,
            bool nullable = true,
            int? length = null,
            int? scale = null,
            int? precision = null,
            object? id = null,
            bool isAutoNumber = false,
            bool identityByDefault = false,
            int? identitySeed = null,
            int? identityIncrement = null,
            string? defaultValue = null)
        {
            return Column(builder, name, col =>
            {
                var typePars = Array.Empty<object>();
                if (IsString(type))
                {
                    typePars = [length ?? 0];
                }
                else if (type == DbType.Decimal)
                {
                    typePars = [precision ?? 18, scale ?? 2];
                }
                col.SetType(builder.SqlType, type, typePars);
            }, computedDefinition, nullable, length, scale, precision, id, isAutoNumber, identityByDefault, identitySeed, identityIncrement, defaultValue);
        }
        public static ITableBuilder ConfigColumn(this ITableBuilder builder, string name, Action<ITableColumnBuilder> config)
        {
            var columnBuilder = builder.GetColumnBuilder(name);
            config(columnBuilder);
            return builder;
        }
        public static ITableColumnBuilder SetId(this ITableColumnBuilder builder, object? id)
        {
            builder.Column.Id = id;
            return builder;
        }
        public static ITableColumnBuilder SetComputed(this ITableColumnBuilder builder, string? computed)
        {
            builder.Column.ComputedDefinition = computed;
            return builder;
        }

        public static ITableColumnBuilder SetDefaultValue(this ITableColumnBuilder builder, string defaultValue)
        {
            builder.Column.DefaultValue = defaultValue;
            return builder;
        }
        public static ITableColumnBuilder SetVarChar(this ITableColumnBuilder builder, int length)
        {
            builder.Column.SetType(builder.SqlType, DbType.String, length);
            return builder;
        }
        public static ITableColumnBuilder SetDataType<T>(this ITableColumnBuilder builder, DbType dbType, params string[] args)
        {
            builder.Column.SetType(builder.SqlType, dbType, args);
            return builder;
        }
        public static ITableColumnBuilder SetDataType(this ITableColumnBuilder builder, DbType dbType, params string[] args)
        {
            builder.Column.SetType(builder.SqlType, dbType, args);
            return builder;
        }
        public static ITableColumnBuilder SetDataType(this ITableColumnBuilder builder, string dataType)
        {
            builder.Column.SetType(dataType);
            return builder;
        }
        public static ITableBuilder AddIndex(this ITableBuilder builder, string column, bool orderDesc = false, bool isUnique = false, string? indexType = null, string? name = null, Action<DatabaseIndex>? configurate = null)
        {
            return AddIndex(builder, new[] { column }, new[] { orderDesc }, isUnique, indexType, name, configurate);
        }
        public static ITableBuilder UnsetIndexByName(this ITableBuilder builder, string name)
        {
            var idx = builder.Table.Indexes.Find(x => x.Name == name);
            if (idx != null)
            {
                idx.Columns.ForEach(x => x.IsIndexed = false);
                builder.Table.Indexes.Remove(idx);
            }
            return builder;
        }
        public static ITableBuilder UnsetIndexByColumn(this ITableBuilder builder, string column, bool singleField = true)
        {
            var col = builder.Table.FindColumn(column);
            if (col == null)
            {
                Throws.ThrowFieldNotFound(column, builder.Table.Name);
            }
            col!.IsIndexed = false;
            var indexs = builder.Table.Indexes.Where(x =>
            {
                if (singleField && x.Columns.Count != 1)
                {
                    return false;
                }
                return x.Columns.Contains(col);
            }).ToList();
            foreach (var item in indexs)
            {
                item.Columns.ForEach(x => x.IsIndexed = false);
                builder.Table.Indexes.Remove(item);
            }
            return builder;
        }
        public static ITableBuilder AddIndex(this ITableBuilder builder, IEnumerable<string> columns, IEnumerable<bool>? orderDescs = null, bool isUnique = false,string? indexType=null, string? name = null, Action<DatabaseIndex>? configurate = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = $"IX_{builder.Table.Name}_{string.Join(",", columns)}";
            }
            var constraint = new DatabaseIndex
            {
                Name = name!,
                IsUnique = isUnique,
                IndexType = indexType,
                TableName=builder.Table.Name,
            };
            if (builder.SqlType== SqlType.MySql)
            {
                constraint.IndexType = "BTREE";
            }
            if (orderDescs != null)
            {
                constraint.ColumnOrderDescs.AddRange(orderDescs);
            }
            foreach (var item in columns)
            {
                var col = builder.Table.FindColumn(item);
                if (col == null)
                {
                    Throws.ThrowFieldNotFound(item, builder.Table.Name);
                }
                constraint.Columns.Add(col);
            }
            configurate?.Invoke(constraint);
            builder.Table.AddIndex(constraint);
            return builder;
        }
        public static ITableBuilder UnSetPrimaryKey(this ITableBuilder builder)
        {
            builder.Table.PrimaryKey = null;
            builder.Table.Columns.ForEach(c => c.IsPrimaryKey = false);
            return builder;
        }
        public static ITableBuilder SetPrimaryKey(this ITableBuilder builder, string column, string? name = null)
        {
            return SetPrimaryKey(builder, new[] { column }, name);
        }
        public static ITableBuilder SetPrimaryKey(this ITableBuilder builder, IEnumerable<string> columns, string? name = null)
        {
            return SetPrimaryKey(builder, c =>
            {
                foreach (var item in columns)
                {
                    var col = builder.Table.FindColumn(item);
                    if (item == null)
                    {
                        Throws.ThrowFieldNotFound(item, builder.Table.Name);
                    }
                    col.Nullable = false;
                    c.AddColumn(col);
                }
            }, name);
        }
        public static ITableBuilder SetPrimaryKey(this ITableBuilder builder, Action<DatabaseConstraint>? configurate = null, string? name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = $"PK_{builder.Table.Name}";
            }
            var constraint = new DatabaseConstraint
            {
                Name = name!,
                ConstraintType = ConstraintType.PrimaryKey,
                TableName = builder.Table.Name,
            };
            configurate?.Invoke(constraint);
            builder.Table.AddConstraint(constraint);
            return builder;
        }
    }
}
