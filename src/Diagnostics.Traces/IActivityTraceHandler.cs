using Diagnostics.Generator.Core;
using System.Diagnostics;

namespace Diagnostics.Traces
{
    public interface IActivityTraceHandler : IOpetatorHandler<Activity>,IInputHandlerSync<Activity>
    {
    }
    public interface IBatchActivityTraceHandler : IBatchInputHandler<Activity>, IBatchInputHandlerSync<Activity>
    {
    }
}
