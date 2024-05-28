using System;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public interface IIntervalExecuter : IDisposable
    {
        bool IsRunning { get; }

        TimeSpan Interval { get; }

        Func<Task> Executer { get; }

        event EventHandler<Exception>? ExceptionRaised;
    }
}
