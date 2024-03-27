namespace Diagnostics.Generator.Core
{
    public readonly struct SynchronousCalculatorResult<T>
    {
        public SynchronousCalculatorResult(T input, T result)
        {
            Input = input;
            Result = result;
        }

        public T Input { get; }

        public T Result { get; }
    }
}
