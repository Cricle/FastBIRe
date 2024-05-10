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

        public override ExportResult Export(in Batch<T> batch)
        {
            Handler.Handle(batch);
            return ExportResult.Success;
        }
    }
}
