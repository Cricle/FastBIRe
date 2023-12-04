using System.Diagnostics.Tracing;
using System.Text;

namespace FastBIRe.Internals.Etw
{
    internal abstract unsafe class EtwEventSource : EventSource
    {
        protected static string ToString( IEnumerable<string>? str)
        {
            if (str==null||!str.Any())
            {
                return string.Empty;
            }
            var s = new StringBuilder();
            foreach (var item in str)
            {
                s.Append(item);
            }
            return s.ToString();
        }
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
