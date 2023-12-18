using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace FastBIRe.Internals.Etw
{
    internal abstract unsafe class EtwEventSource : EventSource
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteString(EventData* data, string? str)
        {
            if (str == null)
            {
                str = string.Empty;
            }
            fixed (char* string1Bytes = str)
            {
                data->DataPointer = (nint)string1Bytes;
                data->Size = ((str.Length + 1) * 2);
            }
        }
    }
}
