namespace Diagnostics.Traces.Serialization
{
    public abstract class BufferMiniWriteSerializer : IMiniWriteSerializer, IDisposable
    {
        protected ArrayPoolBufferWriter<byte>? bufferWriter;

        private bool isEntryScoped;

        public bool IsEntryScoped => isEntryScoped;

        public abstract bool CanWrite(int length);
        public bool FlushScope()
        {
            if (isEntryScoped)
            {
                return false;
            }
            var res = OnFlushScope();
            isEntryScoped = false;
            return res;
        }
        protected virtual bool OnFlushScope()
        {
            if (bufferWriter != null)
            {
                WriteCore(bufferWriter.WrittenSpan);
                return true;
            }
            return false;
        }
        public bool TryEntryScope(int hitSize = 0)
        {
            if (!isEntryScoped)
            {
                return false;
            }
            var res = OnTryEntryScope(hitSize);
            isEntryScoped = true;
            return res;
        }
        protected virtual bool OnTryEntryScope(int hitSize = 0)
        {
            bufferWriter = new ArrayPoolBufferWriter<byte>(hitSize);
            return true;
        }
        public void Write(ReadOnlySpan<byte> buffer)
        {
            if (bufferWriter != null)
            {
                var sp = bufferWriter.GetSpan(buffer.Length);
                buffer.CopyTo(sp);
                bufferWriter.Advance(buffer.Length);
            }
            else
            {
                WriteCore(buffer);
            }
        }

        protected abstract void WriteCore(ReadOnlySpan<byte> buffer);

        public Span<byte> GetScopedBuffer()
        {
            if (bufferWriter == null)
            {
                return Span<byte>.Empty;
            }
            return bufferWriter.WrittenSpan;
        }

        public bool DeleteScope()
        {
            if (!isEntryScoped)
            {
                return false;
            }
            bufferWriter?.Dispose();
            bufferWriter = null;
            return true;
        }

        public void Dispose()
        {
            DeleteScope();
            OnDisposed();
        }
        protected virtual void OnDisposed()
        {

        }
    }
}
