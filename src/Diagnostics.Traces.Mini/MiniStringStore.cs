﻿using Diagnostics.Traces.Serialization;
using Diagnostics.Traces.Stores;
using FastBIRe;

namespace Diagnostics.Traces.Mini
{
    public class MiniStringStore : StringStoreBase
    {
        public MiniStringStore(IUndefinedDatabaseSelector<MiniDatabaseCreatedResult> databaseSelector, string name)
        {
            DatabaseSelector = databaseSelector ?? throw new ArgumentNullException(nameof(databaseSelector));
            Name = name;
        }

        public override string Name { get; }

        public IUndefinedDatabaseSelector<MiniDatabaseCreatedResult> DatabaseSelector { get; }

        public override int Count()
        {
            throw new NotSupportedException();
        }

        public override void Dispose()
        {
            DatabaseSelector.Dispose();
        }

        public override void Insert(BytesStoreValue value)
        {
            InsertMany(new OneEnumerable<BytesStoreValue>(value));
        }

        public override void InsertMany(IEnumerable<BytesStoreValue> strings)
        {
            DatabaseSelector.UsingDatabaseResult(strings, static (res, val) =>
            {
                foreach (var item in val)
                {
                    var sp = item.Value.AsSpan(item.Offset, item.Length);
                    var compressMode = item.Length > 1024 ? TraceCompressMode.Zstd : TraceCompressMode.None;
                    if (compressMode == TraceCompressMode.None)
                    {
                        var head = MiniSerializeString.Create(sp, compressMode);
                        res.Serializer.Write(head);
                        res.Serializer.Write(item.Time);
                        res.Serializer.Write(sp);
                    }
                    else
                    {
                        using (var zstdRes = ZstdHelper.WriteZstd(sp))
                        {
                            var head = MiniSerializeString.Create(zstdRes.Span, compressMode);
                            res.Serializer.Write(head);
                            res.Serializer.Write(item.Time);
                            res.Serializer.Write(zstdRes.Span);
                        }
                    }
                }
            });
        }
    }
}