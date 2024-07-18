namespace Diagnostics.Traces.Stores
{
    public abstract class DatabaseCreatedResultBase : IDatabaseCreatedResult
    {
        protected DatabaseCreatedResultBase(string? filePath, string key)
        {
            Root = new object();
            FilePath = filePath;
            Key = key;
        }
        private int disposedCount;

        public object Root { get; }

        public string? FilePath { get; }

        public string Key { get; }

        public SaveLogModes SaveLogModes { get; set; } = SaveLogModes.All;

        public SaveExceptionModes SaveExceptionModes { get; set; } = SaveExceptionModes.All;

        public SaveActivityModes SaveActivityModes { get; set; } = SaveActivityModes.All;

        public void Dispose()
        {
            if (Interlocked.Increment(ref disposedCount) == 1)
            {
                OnDisposed();
            }
        }
        protected virtual void OnDisposed()
        {

        }
    }
}
