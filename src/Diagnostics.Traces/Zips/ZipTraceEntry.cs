using System.IO.Compression;

namespace Diagnostics.Traces.Zips
{
    public class ZipTraceEntry : IDisposable
    {
        internal ZipTraceEntry(string filePath)
        {
            FilePath = filePath;
            ZipFile = new ZipArchive(File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite), ZipArchiveMode.Update);
            slim = new SemaphoreSlim(1, 1);
            lastVisitSlimTime=DateTime.Now;
        }

        private DateTime lastVisitSlimTime;
        private SemaphoreSlim slim;

        public SemaphoreSlim Slim
        {
            get
            {
                lastVisitSlimTime = DateTime.Now;
                return slim;
            }
        }

        public DateTime LastVisitSlimTime => lastVisitSlimTime;

        public string FilePath { get; }

        public ZipArchive ZipFile { get; }

        public bool LastVisitIsLarged(TimeSpan time)
        {
            return DateTime.Now - lastVisitSlimTime > time;
        }

        public ZipArchiveEntry GetOrCreate(string entityName)
        {
            var entry = ZipFile.GetEntry(entityName);
            if (entry != null)
            {
                return entry;
            }
            return ZipFile.CreateEntry(entityName);
        }

        public Stream GetOrCreateOpenStream(string entityName)
        {
            return GetOrCreate(entityName).Open();
        }

        public void Dispose()
        {
            ZipFile.Dispose();
            Slim.Dispose();
        }
    }
}
