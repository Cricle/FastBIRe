using ParquetSharp;

namespace Diagnostics.Traces.Parquet
{
    public class ParquetDatabaseCreatedResultCreateInput
    {
        public ParquetDatabaseCreatedResultCreateInput(string name, Column[] columns)
        {
            Name = name;
            Columns = columns;
        }

        public string Name { get; }

        public Column[] Columns { get; }
    }
}
