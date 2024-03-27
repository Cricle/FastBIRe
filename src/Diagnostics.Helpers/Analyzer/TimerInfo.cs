#nullable disable
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Diagnostics.Helpers.Analyzer
{
    public class TimerInfo
    {
        public ulong TimerQueueTimerAddress { get; set; }
        public uint DueTime { get; set; }
        public uint Period { get; set; }
        public bool Cancelled { get; set; }
        public ulong StateAddress { get; set; }
        public string StateTypeName { get; set; }
        public string MethodName { get; set; }
        public bool? IsShort { get; set; }
    }
}
#nullable restore