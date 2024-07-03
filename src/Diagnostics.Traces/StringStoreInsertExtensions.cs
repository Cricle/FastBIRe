using System.IO.Compression;
using System.Text;

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

        private static byte[] GetGzipBuffer(byte[] value, int offset, int length, CompressionLevel level)
        {
            using (var mem = new MemoryStream())
            using (var gzip = new GZipStream(mem, level))
            {
                gzip.Write(value, offset, length);
                gzip.Flush();
                return mem.ToArray();
            }
        }

        public static void InsertGzip(this IBytesStore stringStore, byte[] value,int offset,int length, CompressionLevel level = CompressionLevel.Optimal)
        {
            stringStore.Insert(new BytesStoreValue(GetGzipBuffer(value,offset,length, level)));
        }
        public static Task InsertGzipAsync(this IBytesStore stringStore, byte[] value, int offset, int length, CompressionLevel level = CompressionLevel.Optimal)
        {
            return stringStore.InsertAsync(new BytesStoreValue(GetGzipBuffer(value,offset,length, level)));
        }
        public static void InsertGzip(this IBytesStore stringStore, byte[] value, CompressionLevel level = CompressionLevel.Optimal)
        {
            stringStore.Insert(new BytesStoreValue(GetGzipBuffer(value, 0, value.Length, level)));
        }
        public static Task InsertGzipAsync(this IBytesStore stringStore, byte[] value, CompressionLevel level = CompressionLevel.Optimal)
        {
            return stringStore.InsertAsync(new BytesStoreValue(GetGzipBuffer(value, 0, value.Length, level)));
        }
        public static void InsertGzip(this IBytesStore stringStore, string value, CompressionLevel level = CompressionLevel.Optimal)
        {
            InsertGzip(stringStore, Encoding.UTF8.GetBytes(value), level);
        }
        public static Task InsertGzipAsync(this IBytesStore stringStore, string value, CompressionLevel level = CompressionLevel.Optimal)
        {
            return InsertGzipAsync(stringStore, Encoding.UTF8.GetBytes(value), level);
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
