#nullable disable
namespace Diagnostics.Helpers.Analyzer
{
    public class ThreadPoolItem
    {
        public ThreadRoot Type { get; set; }
        public ulong Address { get; set; }
        public string MethodName { get; set; }
    }
}
#nullable restore