using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.AAMode
{
    public class EffectTableCreateAAModelRequest : AAModeRequest
    {
        public EffectTableCreateAAModelRequest(DatabaseTable archiveTable, DatabaseTable aggregationTable, IReadOnlyList<EffectTableSettingItem> settingItems)
            : base(archiveTable, aggregationTable)
        {
            SettingItems = settingItems;
        }

        public IReadOnlyList<EffectTableSettingItem> SettingItems { get; }

        public static EffectTableCreateAAModelRequest From(DatabaseReader reader, string archiveTableName, string aggregationTableName, IEnumerable<string> sourceColumnNames)
        {
            var archiveTable = reader.Table(archiveTableName);
            var aggregationTable = reader.Table(aggregationTableName);
            return From(archiveTable, aggregationTable, sourceColumnNames);
        }
        public static EffectTableCreateAAModelRequest From(DatabaseTable archiveTable, DatabaseTable aggregationTable, IEnumerable<string> sourceColumnNames)
        {
            var settingItems = new List<EffectTableSettingItem>();
            if (!sourceColumnNames.Any())
            {
                throw new ArgumentException($"{nameof(sourceColumnNames)} at less one");
            }
            foreach (var item in sourceColumnNames)
            {
                var column = archiveTable.Columns.Find(x => x.Name == item);
                if (column == null)
                {
                    throw new ArgumentException($"Column {item} not found on table {archiveTable.Name}");
                }
                settingItems.Add(new EffectTableSettingItem(column));
            }
            return new EffectTableCreateAAModelRequest(archiveTable, aggregationTable, settingItems);
        }
    }
}
