using System.Diagnostics;

namespace Diagnostics.Traces
{
    public interface IActivityTraceHandler : IInputHandler<Activity>,IInputHandlerSync<Activity>
    {
    }
    public interface IBatchActivityTraceHandler : IBatchInputHandler<Activity>, IBatchInputHandlerSync<Activity>
    {
    }
}
