using Diagnostics.Traces.Stores;
using FastBIRe;
using ParquetSharp;
using ValueBuffer;

namespace Diagnostics.Traces.Parquet
{
    public class ParquetCounterStoreProvider : ICounterStoreProvider, IDisposable
    {
        class DatabaseEntity : IDisposable
        {
            public readonly IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult> Selector;
            public readonly int ColumnCount;

            public DatabaseEntity(IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult> selector, int columnCount)
            {
                Selector = selector;
                ColumnCount = columnCount;
            }

            public void Dispose()
            {
                Selector.Dispose();
            }
        }
        private readonly Dictionary<string, DatabaseEntity> databaseSelector = new Dictionary<string, DatabaseEntity>();

        public ParquetCounterStoreProvider(IUndefinedDatabaseSelectorFactory<ParquetDatabaseCreatedResult, ParquetDatabaseCreatedResultCreateInput> databaseSelectorCreator)
        {
            DatabaseSelectorCreator = databaseSelectorCreator;
        }

        public IUndefinedDatabaseSelectorFactory<ParquetDatabaseCreatedResult, ParquetDatabaseCreatedResultCreateInput> DatabaseSelectorCreator { get; }

        public Task InitializeAsync(string name, IEnumerable<CounterStoreColumn> columns)
        {
            var col = new List<Column>
            {
                new Column<DateTime>("ts")
            };
            foreach (var item in columns)
            {
                col.Add(new Column<double?>(item.Name));
            }
            var selector = DatabaseSelectorCreator.Create(new ParquetDatabaseCreatedResultCreateInput(name, col.ToArray()));
            lock (databaseSelector)
            {
                if (databaseSelector.Remove(name, out var old))
                {
                    old.Dispose();
                }
                databaseSelector.Add(name, new DatabaseEntity(selector, col.Count));
            }
            return Task.CompletedTask;
        }

        public Task InsertAsync(string name, IEnumerable<double?> values)
        {
            return InsertManyAsync(name, new OneEnumerable<IEnumerable<double?>>(values));
        }

        public Task InsertManyAsync(string name, IEnumerable<IEnumerable<double?>> values)
        {
            if (databaseSelector.TryGetValue(name, out var entity))
            {
                var now = DateTime.Now;
                var ts = new ValueList<DateTime>();
                var cols = new ValueList<double?>[entity.ColumnCount - 1];
                try
                {
                    var rowCount = 0;
                    foreach (var row in values)
                    {
                        ts.Add(now);
                        var offset = 0;
                        foreach (var col in row)
                        {
                            cols[offset++].Add(col);
                        }
                        rowCount++;
                    }

                    entity.Selector.UsingDatabaseResult(res =>
                    {
                        using (var group = res.Writer.AppendRowGroup())
                        {
                            group.NextColumn().LogicalWriter<DateTime>().WriteColumn(ts);
                            for (int i = 0; i < cols.Length; i++)
                            {
                                group.NextColumn().LogicalWriter<double?>().WriteColumn(cols[i]);
                            }
                        }
                    });
                    entity.Selector.ReportInserted(rowCount);
                }
                finally
                {
                    ts.Dispose();
                    for (int i = 0; i < cols.Length; i++)
                    {
                        cols[i].Dispose();
                    }
                }
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            lock (databaseSelector)
            {
                foreach (var item in databaseSelector)
                {
                    item.Value.Dispose();
                }
                databaseSelector.Clear();
            }
        }
    }
}
