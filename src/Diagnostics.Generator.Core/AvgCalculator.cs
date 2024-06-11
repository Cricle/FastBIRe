using System.Threading.Tasks;
using System.Threading;
using System.Numerics;

namespace Diagnostics.Generator.Core
{
    public class AvgCalculator : SynchronousCalculator<double>
    {
        private ulong count = 0;
        private double lastAvg = 0;

        public override double GetValue()
        {
            return Volatile.Read(ref lastAvg);
        }

        protected override Task OnProcessAsync(double value, CancellationToken token)
        {
            count++;
            if (count == 1)
            {
                lastAvg = value;
            }
            else
            {
                lastAvg = lastAvg + (value - lastAvg) / count;
            }
            RaiseUpdated(new SynchronousCalculatorResult<double>(value, lastAvg));
            return Task.CompletedTask;
        }
    }
#if NET8_0_OR_GREATER
    public class SumCalculator<T> : SynchronousCalculator<T>
        where T : IAdditionOperators<T, T, T>
    {
        private T? sum = default;

        public override T GetValue()
        {
            Thread.MemoryBarrier();
            return sum!;
        }

        protected override Task OnProcessAsync(T value, CancellationToken token)
        {
#nullable disable
            sum += value;
#nullable enable
            RaiseUpdated(new SynchronousCalculatorResult<T>(value, sum));
            return Task.CompletedTask;
        }
    }
#endif
}
