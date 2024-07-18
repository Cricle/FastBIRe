using Diagnostics.Traces.Stores;
using FastBIRe;
using System;
using System.Buffers;
using System.Security.Cryptography;
using ValueBuffer;

namespace Diagnostics.Traces.Parquet
{
    public class ParquetStringStore : StringStoreBase, IDisposable
    {
        public ParquetStringStore(IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult> databaseSelector, string name)
        {
            DatabaseSelector = databaseSelector;
            Name = name;
        }

        public IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult> DatabaseSelector { get; }

        public override string Name{ get; }

        public override int Count()
        {
            return DatabaseSelector.UsingDatabaseResult(static (res) =>
            {
                using (var reader = res.GetReader())
                {
                    var c = 0;
                    var groupCount = reader.Operator.FileMetaData.NumRowGroups;
                    for (int i = 0; i < groupCount; i++)
                    {
                        c += (int)reader.Operator.RowGroup(i).MetaData.NumRows;
                    }
                    return c;
                }
            });
        }

        public override void Dispose()
        {
            DatabaseSelector.Dispose();
        }

        public override unsafe void Insert(BytesStoreValue value)
        {
            CoreInsertMany(new OneEnumerable<BytesStoreValue>(value));
        }

        private void CoreInsertMany(IEnumerator<BytesStoreValue> strings)
        {
            using var dts = new ValueList<DateTime>();
            using var vls = new ValueList<byte[]>();

            while(strings.MoveNext())
            {
                var item = strings.Current;
                dts.Add(item.Time);
                if (item.Offset == 0 && item.Length == item.Value.Length)
                {
                    vls.Add(item.Value);
                }
                else
                {
                    var buffer = new byte[item.Length - item.Offset];
                    Array.Copy(item.Value, item.Offset, buffer, 0, item.Length);
                    vls.Add(buffer);
                }
            }
            DatabaseSelector.UsingDatabaseResult(strings, (res, v) =>
            {
                using (var writer = res.GetWriter())
                {
                    using var group = writer.Operator.AppendRowGroup();
                    if (dts.BufferSlotIndex == 0 && vls.BufferSlotIndex == 0)
                    {
                        var arrdt = dts.DangerousGetArray(0);
                        var arrvl = vls.DangerousGetArray(0);
                        group.NextColumn().LogicalWriter<DateTime>().WriteBatch(arrdt);
                        group.NextColumn().LogicalWriter<byte[]>().WriteBatch(arrvl);
                    }
                    else
                    {
                        using (var dtWriter = group.NextColumn().LogicalWriter<DateTime>())
                        {
                            foreach (var item in dts.DangerousEnumerableArray())
                            {
                                dtWriter.WriteBatch(item.Span);
                            }
                        }
                        using (var vlWriter = group.NextColumn().LogicalWriter<byte[]>())
                        {
                            foreach (var item in vls.DangerousEnumerableArray())
                            {
                                vlWriter.WriteBatch(item.Span);
                            }
                        }
                    }
                }
            });
        }

        public override void InsertMany(IEnumerable<BytesStoreValue> strings)
        {
            using (var enu=strings.GetEnumerator())
            {
                CoreInsertMany(enu);
            }
        }
    }
}
