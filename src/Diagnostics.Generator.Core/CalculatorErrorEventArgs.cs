using System;

namespace Diagnostics.Generator.Core
{
    public readonly struct CalculatorErrorEventArgs<T>
    {
        public CalculatorErrorEventArgs(T value, Exception exception)
        {
            Value = value;
            Exception = exception;
        }

        public T Value { get; }

        public Exception Exception { get; }
    }

}
