using System.Runtime.InteropServices;

namespace Diagnostics.Traces.Serialization
{
    [StructLayout(LayoutKind.Sequential, Size = 4096)]
    public struct TraceHeader<TMode>
    {
        public TMode Mode;
    }
}
