using Diagnostics.Traces.Serialization;
using Diagnostics.Traces.Stores;
using FastBIRe;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Diagnostics.Traces.Mini
{
    public class MiniCounterStoreProvider : ICounterStoreProvider, IDisposable
    {
        class DatabaseEntity : IDisposable
        {
            public readonly IUndefinedDatabaseSelector<MiniDatabaseCreatedResult> Selector;
            public readonly int ColumnCount;

            public DatabaseEntity(IUndefinedDatabaseSelector<MiniDatabaseCreatedResult> selector, int columnCount)
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

        public MiniCounterStoreProvider(IUndefinedDatabaseSelectorFactory<MiniDatabaseCreatedResult, MiniCreatedResultCreateInput> databaseSelectorCreator)
        {
            DatabaseSelectorCreator = databaseSelectorCreator;
        }

        public IUndefinedDatabaseSelectorFactory<MiniDatabaseCreatedResult, MiniCreatedResultCreateInput> DatabaseSelectorCreator { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task InitializeAsync(string name, IEnumerable<CounterStoreColumn> columns)
        {
            var selector = DatabaseSelectorCreator.Create(new MiniCreatedResultCreateInput(name));
            lock (databaseSelector)
            {
                if (databaseSelector.TryGetValue(name,out var old))
                {
                    old.Dispose();
                    databaseSelector.Remove(name);
                }
                //Init head
                selector.UnsafeUsingDatabaseResult(columns,static (res,col) =>
                {
                    var count = col.Count();
                    var head = new TraceCounterHeader { FieldCount = count };
                    res.Serializer.Write(head);
                });
                databaseSelector.Add(name, new DatabaseEntity(selector, columns.Count()));
            }
            return Task.CompletedTask;
        }

        public Task InsertAsync(string name, IEnumerable<double?> values)
        {
            return InsertManyAsync(name, new OneEnumerable<IEnumerable<double?>>(values));
        }

        public unsafe Task InsertManyAsync(string name, IEnumerable<IEnumerable<double?>> values)
        {
            if (databaseSelector.TryGetValue(name, out var selector))
            {
                selector.Selector.UsingDatabaseResult(values, (res, val) =>
                {
                    var now = DateTime.Now;

                    foreach (var row in val)
                    {
                        using (var bufferWriter = new ArrayPoolBufferWriter<byte>())
                        {
                            foreach (var item in row)
                            {
                                var buffer = bufferWriter.GetSpan(sizeof(double));
                                if (item == null)
                                {
                                    Unsafe.Write(Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), double.NaN);
                                }
                                else
                                {
                                    Unsafe.Write(Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), item.Value);
                                }
                                bufferWriter.Advance(sizeof(double));
                            }

                            var writted = bufferWriter.WrittenSpan;
                            var header = MiniCounterHeader.Create(now, writted);
                            res.Serializer.Write(header);
                            res.Serializer.Write(writted);
                        }
                    }

                });
            }
            return Task.CompletedTask;
        }
    }
}
