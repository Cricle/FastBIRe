namespace Diagnostics.Traces
{
    public readonly struct BufferOperatorExceptionEventArgs<T>
    {
        public BufferOperatorExceptionEventArgs(T? input, Exception exception)
        {
            Input = input;
            Exception = exception;
        }

        public T? Input { get; }

        public Exception Exception { get; }
    }
}
