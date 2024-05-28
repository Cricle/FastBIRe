using OpenTelemetry;

namespace Diagnostics.Traces
{
    public class SimpleTraceExporter<T> : BaseExporter<T>
        where T:class
    {
        public SimpleTraceExporter(IInputHandlerSync<T> handler)
        {
            Handler = handler;
        }

        public IInputHandlerSync<T> Handler { get; }

        public override ExportResult Export(in Batch<T> batch)
        {
            foreach (var item in batch)
            {
                Handler.Handle(item);
            }
            return ExportResult.Success;
        }
    }
}
