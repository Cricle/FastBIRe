using Microsoft.Diagnostics.Runtime;

namespace Diagnostics.Helpers.Models
{
    public struct ThreadStackFrame
    {
        public ThreadStackFrame(ulong stackPointer, ulong instructionPointer, string stackTrace)
        {
            StackPointer = stackPointer;
            InstructionPointer = instructionPointer;
            StackTrace = stackTrace;
        }

        public ulong StackPointer { get; set; }

        public ulong InstructionPointer { get; set; }

        public string StackTrace { get; set; }

        public override string ToString()
        {
            return string.Format("{0:x12} {1:x12} {2}", StackPointer, InstructionPointer, StackTrace);
        }
        public static ThreadStackFrame Create(ClrStackFrame frame)
        {
            return new ThreadStackFrame(frame.StackPointer, frame.InstructionPointer, frame.ToString() ?? string.Empty);
        }
    }
}
