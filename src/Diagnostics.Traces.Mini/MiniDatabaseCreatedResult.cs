using Diagnostics.Traces.Stores;

namespace Diagnostics.Traces.Mini
{
    public class MiniDatabaseCreatedResult : DatabaseCreatedResultBase
    {
        public MiniDatabaseCreatedResult(string filePath, string key,long capacity,bool autoCapacity) 
            : base(filePath, key)
        {
            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            Serializer = new MemoryMapFileMiniWriteSerializer(filePath, capacity, autoCapacity);
            Serializer.WriteHead(new TraceHeader { Count = TraceHeader.UnknowCount });
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
