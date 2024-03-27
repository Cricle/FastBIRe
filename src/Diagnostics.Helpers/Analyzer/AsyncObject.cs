using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;
using System.Collections.Generic;

namespace Diagnostics.Helpers.Analyzer
{
    public sealed class AsyncObject
    {
        /// <summary>The actual object on the heap.</summary>
        public ClrObject Object;
        /// <summary>true if <see cref="Object"/> is an AsyncStateMachineBox.</summary>
        public bool IsStateMachine;
        /// <summary>A compiler-generated state machine extracted from the object, if one exists.</summary>
        public IClrValue? StateMachine;
        /// <summary>The state of the state machine, if the object contains a state machine.</summary>
        public int AwaitState;
        /// <summary>The <see cref="Object"/>'s Task state flags, if it's a task.</summary>
        public int TaskStateFlags;
        /// <summary>Whether this object meets the user-specified criteria for inclusion.</summary>
        public bool IncludeInOutput;
        /// <summary>true if this is a top-level instance that nothing else continues to.</summary>
        /// <remarks>This starts off as true and then is flipped to false when we find a continuation to this object.</remarks>
        public bool TopLevel { get; set; } = true;
        /// <summary>The address of the native code for a method on the object (typically MoveNext for a state machine).</summary>
        public ulong NativeCode;
        /// <summary>This object's continuations.</summary>
        public readonly List<ClrObject> Continuations = new();
    }
}
