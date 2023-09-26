using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using FastBIRe.Naming;
using FastBIRe.Querying;

namespace FastBIRe.AAMode
{
    public class EffectTableCreateAAModelHelper : IModeHelper<EffectTableCreateAAModelRequest>
    {
        public static readonly INameGenerator DefaultEffectNameGenerator = new RegexNameGenerator("{0}_effect");

        public static readonly EffectTableCreateAAModelHelper Default = new EffectTableCreateAAModelHelper(DefaultEffectNameGenerator, DefaultEffectTableKeyNameGenerator.Instance, StringComparison.Ordinal);

        public EffectTableCreateAAModelHelper(INameGenerator effectNameGenerator, INameGenerator effectTableKeyNameGenerator, StringComparison columnComparision)
        {
            EffectNameGenerator = effectNameGenerator;
            EffectTableKeyNameGenerator = effectTableKeyNameGenerator;
            ColumnComparision = columnComparision;
        }

        public INameGenerator EffectNameGenerator { get; }

        public INameGenerator EffectTableKeyNameGenerator { get; }

        public StringComparison ColumnComparision { get; } = StringComparison.Ordinal;

        public void Apply(DatabaseReader reader, EffectTableCreateAAModelRequest request)
        {
            //Check table exists
            var effectTableName = EffectNameGenerator.Create(new[] { request.AggregationTable.Name });

            var effectTable = reader.Table(effectTableName);
            if (effectTable != null)
            {
                //The table exists, check name and db types
                var isChanged = IsEffectTableChanged(reader, request, effectTable);
                if (!isChanged)
                {
                    //Effect table no changed, nothing to do
                    return;
                }
                //Drop the old table
                var dropTableSql = new TableHelper(reader.SqlType!.Value).CreateDropTable(effectTableName);
                request.Scripts.Add(dropTableSql);
            }
            //Create the effect table
            var ddl = new DdlGeneratorFactory(reader.SqlType!.Value);
            effectTable = new DatabaseTable
            {
                Name = effectTableName
            };
            //Add all columns
            foreach (var item in request.SettingItems)
            {
                effectTable.AddColumn(item.EffectColumn);
                //All column was index
                var idx = new DatabaseIndex
                {
                    Name = "IX_" + item.EffectColumn.Name,
                    Columns =
                    {
                        item.EffectColumn
                    }
                };
                effectTable.AddIndex(idx);
            }
            var createSql = ddl.TableGenerator(effectTable).Write();
            request.Scripts.Add(createSql);
        }
        protected virtual bool IsEffectTableChanged(DatabaseReader reader, EffectTableCreateAAModelRequest request, DatabaseTable table)
        {
            //Is the field count equals
            if (table.Columns.Count != request.SettingItems.Count)
            {
                return true;
            }
            foreach (var item in request.SettingItems)
            {
                //Is all the name exists
                var remoteColumn = table.Columns.Find(x => x.Name.Equals(item.EffectColumn.Name, ColumnComparision));
                if (remoteColumn == null)
                {
                    return true;
                }
                //Is column type equals
                if (!string.Equals(remoteColumn.DbDataType, item.EffectColumn.DbDataType, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                //Nullable 
                if (item.EffectColumn.Nullable != remoteColumn.Nullable)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
