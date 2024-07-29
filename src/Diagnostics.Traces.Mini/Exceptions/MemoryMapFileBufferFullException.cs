namespace Diagnostics.Traces.Mini.Exceptions
{
    public class MemoryMapFileBufferFullException : Exception
    {
        internal MemoryMapFileBufferFullException(string? message,long capacity,long written,long needs) 
            : base(message)
        {
            Capacity = capacity;
            Written = written;
            Needs = needs;
        }

        public long Capacity { get;}

        public long Written { get; }
        
        public long Needs { get; }
    }
}
