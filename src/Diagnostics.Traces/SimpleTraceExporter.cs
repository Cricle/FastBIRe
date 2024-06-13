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

        public event EventHandler<Exception>? ExceptionRaised;

        public override ExportResult Export(in Batch<T> batch)
        {
            foreach (var item in batch)
            {
                try
                {
                    Handler.Handle(item);
                }
                catch (Exception ex)
                {
                    ExceptionRaised?.Invoke(this, ex);
                }
            }
            return ExportResult.Success;
        }
    }
}
