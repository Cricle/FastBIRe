using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace Diagnostics.Generator.Core
{
    public class ExtremumCalculator<T> : SynchronousCalculator<T>
        where T : struct
    {
        private T? value = default;

        public ExtremumTypes Extremum { get; }

        public ExtremumCalculator(ExtremumTypes extremum, IComparer<T> comparer = null)
        {
            Comparer = comparer ?? Comparer<T>.Default;
            Extremum = extremum;
        }

        public IComparer<T> Comparer { get; }

        public override T GetValue()
        {
            Thread.MemoryBarrier();
            return value ?? default;
        }

        protected override Task OnProcessAsync(T value, CancellationToken token)
        {
            if (this.value == null)
            {
                this.value = value;
            }
            else
            {
                var compareResult = Comparer.Compare(value, this.value.Value);
                if ((Extremum == ExtremumTypes.Max && compareResult > 0) ||
                    (Extremum == ExtremumTypes.Min && compareResult < 0))
                {
                    this.value = value;
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
            RaiseUpdated(new SynchronousCalculatorResult<T>(value, this.value.Value));
            return Task.CompletedTask;
        }
    }
}
