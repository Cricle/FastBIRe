using System.IO.Compression;

namespace Diagnostics.Traces
{
    public static class StringStoreInsertExtensions
    {
        public static void Insert(this IBytesStore stringStore, string value)
        {
            stringStore.Insert(new BytesStoreValue(value));
        }
        public static Task InsertAsync(this IBytesStore stringStore, string value)
        {
            return stringStore.InsertAsync(new BytesStoreValue(value));
        }

        public static void InsertGzip(this IBytesStore stringStore, byte[] value, int offset, int length, CompressionLevel level = CompressionLevel.Optimal)
        {
            using (var gzipResult = GzipHelper.Compress(value, offset, length, level))
            {
                stringStore.Insert(new BytesStoreValue(gzipResult.Result,0,gzipResult.Count));
            }
        }
        public static async Task InsertGzipAsync(this IBytesStore stringStore, byte[] value, int offset, int length, CompressionLevel level = CompressionLevel.Optimal)
        {
            using (var gzipResult = GzipHelper.Compress(value, offset, length, level))
            {
                await stringStore.InsertAsync(new BytesStoreValue(gzipResult.Result,0,gzipResult.Count));
            }
        }
        public static void InsertGzip(this IBytesStore stringStore, byte[] value, CompressionLevel level = CompressionLevel.Optimal)
        {
            using (var gzipResult = GzipHelper.Compress(value, 0, value.Length, level))
            {
                stringStore.Insert(new BytesStoreValue(gzipResult.Result,0,gzipResult.Count));
            }
        }
        public static async Task InsertGzipAsync(this IBytesStore stringStore, byte[] value, CompressionLevel level = CompressionLevel.Optimal)
        {
            using (var gzipResult = GzipHelper.Compress(value, 0, value.Length, level))
            {
                await stringStore.InsertAsync(new BytesStoreValue(gzipResult.Result,0,gzipResult.Count));
            }
        }
        public static void InsertGzip(this IBytesStore stringStore, string value, CompressionLevel level = CompressionLevel.Optimal)
        {
            using (var gzipResult = GzipHelper.Compress(value, level:level))
            {
                stringStore.Insert(new BytesStoreValue(gzipResult.Result, 0, gzipResult.Count));

            }
        }
        public static async Task InsertGzipAsync(this IBytesStore stringStore, string value, CompressionLevel level = CompressionLevel.Optimal)
        {
            using (var gzipResult = GzipHelper.Compress(value, level: level))
            {
                await stringStore.InsertAsync(new BytesStoreValue(gzipResult.Result,0,gzipResult.Count));
            }
        }

        public static void InsertMany(this IBytesStore stringStore, IEnumerable<string> strings)
        {
            var now = DateTime.Now;
            stringStore.InsertMany(strings.Select(x => new BytesStoreValue(now, x)));
        }
        public static Task InsertManyAsync(this IBytesStore stringStore, IEnumerable<string> strings)
        {
            var now = DateTime.Now;
            return stringStore.InsertManyAsync(strings.Select(x => new BytesStoreValue(now, x)));
        }
    }
}
