using Diagnostics.Traces.Stores;
using System.IO.MemoryMappedFiles;

namespace Diagnostics.Traces.Mini
{
    public class MiniDatabaseCreatedResult : DatabaseCreatedResultBase
    {
        public MiniDatabaseCreatedResult(string filePath, string key,long capacity) 
            : base(filePath, key)
        {
            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            var mappedFile = MemoryMappedFile.CreateFromFile(filePath, FileMode.OpenOrCreate,null, capacity);
            Serializer = new MemoryMapFileMiniWriteSerializer(mappedFile,capacity);
        }

        private long count;

        public long Count => count;

        public void AddCount(long value = 1)
        {
            count += value;
        }


        public MemoryMapFileMiniWriteSerializer Serializer { get; }

        protected override void OnDisposed()
        {
            Serializer.Dispose();
        }
    }
}
