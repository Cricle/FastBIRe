using OpenTelemetry;

namespace Diagnostics.Traces
{
    public class BufferTraceExporer<T> : BaseExporter<T>
       where T : class
    {
        protected readonly BufferOperator<T> bufferOperator;

        public BufferTraceExporer(IInputHandler<T> handler)
            : this(handler, new BufferOperator<T>(handler))
        {

        }
        public BufferTraceExporer(IInputHandler<T> handler, BufferOperator<T> bufferOperator)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this.bufferOperator = bufferOperator;
        }

        public IInputHandler<T> Handler { get; }

        public override ExportResult Export(in Batch<T> batch)
        {
            foreach (var item in batch)
            {
                bufferOperator.Add(item);
            }
            return ExportResult.Success;
        }

        protected override void Dispose(bool disposing)
        {
            bufferOperator.Dispose();
        }
    }
}
