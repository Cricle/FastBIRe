using Diagnostics.Traces.Serialization;
using Diagnostics.Traces.Stores;
using FastBIRe;
using System;
using System.Buffers;
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
                if (databaseSelector.TryGetValue(name, out var old))
                {
                    old.Dispose();
                    databaseSelector.Remove(name);
                }
                //Init head
                selector.UnsafeUsingDatabaseResult(columns, static (res, col) =>
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
                var count = selector.Selector.UsingDatabaseResult(values, (res, val) =>
                {
                    var now = DateTime.Now;
                    var c = 0;
                    foreach (var row in val)
                    {
                        var size = sizeof(double) * selector.ColumnCount;
                        var sharedBuffer = ArrayPool<byte>.Shared.Rent(size);
                        try
                        {
                            byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(sharedBuffer.AsSpan()));
                            var offset = 0;
                            foreach (var item in row)
                            {
                                if (item == null)
                                {
                                    Unsafe.Write(ptr+ offset* sizeof(double), double.NaN);
                                }
                                else
                                {
                                    Unsafe.Write(ptr + offset * sizeof(double), item.Value);
                                }
                            }
                            var writted = sharedBuffer.AsSpan(0, size);

                            var header = MiniCounterHeader.Create(now, writted);
                            res.Serializer.Write(header);
                            res.Serializer.Write(writted);
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(sharedBuffer);
                        }
                        c++;
                    }
                    return c;
                });
                selector.Selector.ReportInserted(count);
            }
            return Task.CompletedTask;
        }
    }
}
