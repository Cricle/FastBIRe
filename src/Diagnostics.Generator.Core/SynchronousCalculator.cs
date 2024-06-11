using System;

namespace Diagnostics.Generator.Core
{
    public abstract class SynchronousCalculator<T> : SynchronousExecuter<T>
    {
        public event EventHandler<SynchronousCalculatorResult<T>>? Updated;

        public abstract T GetValue();

        protected void RaiseUpdated(in SynchronousCalculatorResult<T> result)
        {
            Updated?.Invoke(this, result);
        }
    }
}
