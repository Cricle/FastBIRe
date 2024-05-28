using System.IO.Compression;

namespace FastBIRe.Store
{
    public class ZipDataStore : SyncDataStore, IDisposable
    {
        public static ZipDataStore FromFile(string nameSpace, string path, FileShare fileShare = FileShare.Read)
        {
            var dir = new DirectoryInfo(Path.GetDirectoryName(path)!);
            if (!dir.Exists)
            {
                dir.Create();
            }
            var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, fileShare);
            var zip = new ZipArchive(fs, ZipArchiveMode.Update);
            return new ZipDataStore(nameSpace, zip);
        }

        public ZipDataStore(string nameSpace, ZipArchive zipArchive)
            : base(nameSpace)
        {
            ZipArchive = zipArchive;
        }

        public ZipArchive ZipArchive { get; }

        public override void Clear()
        {
            foreach (var item in ZipArchive.Entries)
            {
                item.Delete();
            }
        }
        public override bool Exists(string key)
        {
            return ZipArchive.Entries.Any(x => x.Name == key);
        }

        public override Stream? Get(string key)
        {
            return ZipArchive.Entries.FirstOrDefault(x => x.Name == key)?.Open();
        }

        public override bool Remove(string key)
        {
            var entity = ZipArchive.Entries.FirstOrDefault(x => x.Name == key);
            if (entity != null)
            {
                entity.Delete();
                return true;
            }
            return false;
        }

        public override void Set(string key, Stream value)
        {
            var entity = ZipArchive.Entries.FirstOrDefault(x => x.Name == key);
            entity ??= ZipArchive.CreateEntry(key);
            using (var stream = entity.Open())
            {
                value.CopyTo(stream);
            }
        }
        public override async Task SetAsync(string key, Stream value, CancellationToken token = default)
        {
            var entity = ZipArchive.Entries.FirstOrDefault(x => x.Name == key);
            entity ??= ZipArchive.CreateEntry(key);
            using (var stream = entity.Open())
            {
                await value.CopyToAsync(stream, 81920, token);
            }
        }

        public void Dispose()
        {
            ZipArchive.Dispose();
        }
    }
}
