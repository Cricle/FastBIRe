using OpenTelemetry;

namespace Diagnostics.Traces
{
    public class SimpleBatchTraceExporter<T> : BaseExporter<T>
        where T : class
    {
        public SimpleBatchTraceExporter(IBatchInputHandlerSync<T> handler)
        {
            Handler = handler;
        }

        public IBatchInputHandlerSync<T> Handler { get; }

        public event EventHandler<Exception>? ExceptionRaised;

        public override ExportResult Export(in Batch<T> batch)
        {
            try
            {
                Handler.Handle(batch);
                return ExportResult.Success;
            }
            catch (Exception ex)
            {
                ExceptionRaised?.Invoke(this, ex);
                return ExportResult.Failure;
            }
        }
    }
}
