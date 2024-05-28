using Microsoft.Diagnostics.Runtime;
using System.Text;

namespace Diagnostics.Helpers
{
    public record class StackSnapshot
    {
        public StackSnapshot(ClrInfo clrInfo)
        {
            ClrInfo = clrInfo;
        }

        public ClrInfo ClrInfo { get; }

        public void GetThreadString(StringBuilder builder, ClrRuntime runtime, bool withDos)
        {
            foreach (var item in runtime.Threads)
            {
                if (item.IsAlive)
                {
                    item.GetThreadString(builder, runtime, withDos);
                    builder.AppendLine();
                }
            }
        }
        public override string ToString()
        {
            using (var runtime = ClrInfo.CreateRuntime())
            {
                var s = new StringBuilder();
                s.AppendFormat("CLR: {0}, Thread Count: {1}", ClrInfo, runtime.Threads.Length);
                s.AppendLine();
                GetThreadString(s, runtime, true);
                return s.ToString();
            }
        }
    }
}