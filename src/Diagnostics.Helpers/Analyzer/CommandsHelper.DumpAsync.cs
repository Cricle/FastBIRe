using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Diagnostics.Helpers.Analyzer
{

    public static partial class CommandsHelper
    {
        /// <summary>Indent width.</summary>
        private const int TabWidth = 2;

        public static Dictionary<ClrObject, AsyncObject> DumpAsync(ClrRuntime runtime, 
            TextWriter? writer=null, 
            bool summarize = false,
            bool coalesceStacks = false, 
            bool includeCompleted = false,
            bool includeTasks=false,
            string? nameSubstring=null,
            ulong? objectAddress=null,
            ulong? methodTableAddress=null,
            bool  displayFields=false,
            CancellationToken token=default)
        {
            ClrHeap heap = runtime.Heap;
            if (!heap.CanWalkHeap)
            {
                throw new Exception("Unable to examine the heap.");
            }

            ClrType? taskType = runtime.BaseClassLibrary.GetTypeByName("System.Threading.Tasks.Task");
            if (taskType is null)
            {
                throw new Exception("Unable to find required type.");
            }

            ClrStaticField? taskCompletionSentinelType = taskType.GetStaticFieldByName("s_taskCompletionSentinel");

            ClrObject taskCompletionSentinel = default;

            if (taskCompletionSentinelType is not null)
            {
                Debug.Assert(taskCompletionSentinelType.IsObjectReference);
                taskCompletionSentinel = taskCompletionSentinelType.ReadObject(runtime.BaseClassLibrary.AppDomain);
            }

            // Enumerate the heap, gathering up all relevant async-related objects.
            Dictionary<ClrObject, AsyncObject> objects = CollectObjects();
            if (writer==null)
            {
                return objects;
            }
            // Render the data according to the options specified.
            if (summarize)
            {
                RenderStats();
            }
            else if (coalesceStacks)
            {
                RenderCoalescedStacks();
            }
            else
            {
                RenderStacks();
            }
            return objects;

            // <summary>Group frames and summarize how many of each occurred.</summary>
            void RenderStats()
            {
                // Enumerate all of the "frames", and create a mapping from a rendering of that
                // frame to its associated type and how many times that frame occurs.
                Dictionary<string, (ClrType Type, int Count)> typeCounts = new();
                foreach (KeyValuePair<ClrObject, AsyncObject> pair in objects)
                {
                    ClrObject obj = pair.Key;
                    if (obj.Type is null)
                    {
                        continue;
                    }

                    string description = Describe(obj);

                    if (!typeCounts.TryGetValue(description, out (ClrType Type, int Count) value))
                    {
                        value = (obj.Type, 0);
                    }

                    value.Count++;
                    typeCounts[description] = value;
                }

                // Render one line per frame.
                if (writer != null)
                {
                    writer.WriteLine($"{"MT",-16} {"Count",-8} Type");

                    foreach (KeyValuePair<string, (ClrType Type, int Count)> entry in typeCounts.OrderByDescending(e => e.Value.Count))
                    {
                        WriteAddress(entry.Value.Type.MethodTable,writer);
                        writer.WriteLine($" {entry.Value.Count,-8:N0} {entry.Key}");
                    }
                }
            }

            // <summary>Group stacks at each frame in order to render a tree of coalesced stacks.</summary>
            void RenderCoalescedStacks()
            {
                // Find all stacks to include.
                List<ClrObject> startingList = new();
                foreach (KeyValuePair<ClrObject, AsyncObject> entry in objects)
                {
                    token.ThrowIfCancellationRequested();

                    AsyncObject obj = entry.Value;
                    if (obj.TopLevel && ShouldIncludeStack(obj))
                    {
                        startingList.Add(entry.Key);
                    }
                }

                // If we found any, render them.
                if (startingList.Count > 0)
                {
                    RenderLevel(startingList, 0);
                }

                // <summary>Renders the next level of frames for coalesced stacks.</summary>
                void RenderLevel(List<ClrObject> frames, int depth)
                {
                    token.ThrowIfCancellationRequested();
                    List<ClrObject> nextLevel = new();

                    // Grouping function.  We want to treat all objects that render the same as the same entity.
                    // For async state machines, we include the await state, both because we want it to render
                    // and because we want to see state machines at different positions as part of different groups.
                    Func<ClrObject, string> groupBy = o => {
                        string description = Describe(o);
                        if (objects.TryGetValue(o, out AsyncObject asyncObject) && asyncObject.IsStateMachine)
                        {
                            description = $"({asyncObject.AwaitState}) {description}";
                        }
                        return description;
                    };

                    // Group all of the frames, rendering each group as a single line with a count.
                    // Then recur for each.
                    int stackId = 1;
                    foreach (IGrouping<string, ClrObject> group in frames.GroupBy(groupBy).OrderByDescending(g => g.Count()))
                    {
                        int count = group.Count();
                        Debug.Assert(count > 0);

                        // For top-level frames, write out a header.
                        if (depth == 0)
                        {
                            writer?.WriteLine($"STACKS {stackId++}");
                        }

                        // Write out the count and frame.
                        writer?.Write($"{Tabs(depth)}[{count}] ");
                        WriteAddress(group.First().Type?.MethodTable ?? 0,writer);
                        writer?.WriteLine($" {group.Key}");

                        // Gather up all of the next level of frames.
                        nextLevel.Clear();
                        foreach (ClrObject next in group)
                        {
                            if (objects.TryGetValue(next, out AsyncObject asyncObject))
                            {
                                // Note that the merging of multiple continuations can lead to numbers increasing at a particular
                                // level of the coalesced stacks.  It's not clear there's a better answer.
                                nextLevel.AddRange(asyncObject.Continuations);
                            }
                        }

                        // If we found any, recur.
                        if (nextLevel.Count != 0)
                        {
                            RenderLevel(nextLevel, depth + 1);
                        }

                        if (depth == 0)
                        {
                            writer?.WriteLine();
                        }
                    }
                }
            }

            // <summary>Render each stack of frames.</summary>
            void RenderStacks()
            {
                Stack<(AsyncObject AsyncObject, int Depth)> stack = new();

                // Find every top-level object (ones that nothing else has as a continuation) and output
                // a stack starting from each.
                int stackId = 1;
                foreach (KeyValuePair<ClrObject, AsyncObject> entry in objects)
                {
                    token.ThrowIfCancellationRequested();
                    AsyncObject top = entry.Value;
                    if (!top.TopLevel || !ShouldIncludeStack(top))
                    {
                        continue;
                    }

                    int depth = 0;

                    writer?.WriteLine($"STACK {stackId++}");

                    // If the top-level frame is an async method that's paused at an await, it must be waiting on
                    // something.  Try to synthesize a frame to represent that thing, just to provide a little more information.
                    if (top.IsStateMachine && top.AwaitState >= 0 && !IsCompleted(top.TaskStateFlags) &&
                        top.StateMachine is IClrValue stateMachine &&
                        stateMachine.Type is not null)
                    {
                        // Short of parsing the method's IL, we don't have a perfect way to know which awaiter field
                        // corresponds to the current await state, as awaiter fields are shared across all awaits that
                        // use the same awaiter type.  We instead employ a heuristic.  If the await state is 0, the
                        // associated field will be the first one (<>u__1); even if other awaits share it, it's fine
                        // to use.  Similarly, if there's only one awaiter field, we know that must be the one being
                        // used.  In all other situations, we can't know which of the multiple awaiter fields maps
                        // to the await state, so we instead employ a heuristic of looking for one that's non-zero.
                        // The C# compiler zero's out awaiter fields when it's done with them, so if we find an awaiter
                        // field with any non-zero bytes, it must be the one in use.  This can have false negatives,
                        // as it's perfectly valid for an awaiter to be all zero bytes, but it's better than nothing.

                        // if the name is null, we have to assume it's an awaiter

                        Func<IClrInstanceField, bool> hasOneAwaiterField = static f => {
                            return f.Name is null
                                || f.Name.StartsWith("<>u__", StringComparison.Ordinal);
                        };

                        if ((top.AwaitState == 0)
                            || stateMachine.Type.Fields.Count(hasOneAwaiterField) == 1)
                        {
                            if (stateMachine.Type.GetFieldByName("<>u__1") is ClrInstanceField field &&
                                TrySynthesizeAwaiterFrame(field))
                            {
                                depth++;
                            }
                        }
                        else
                        {
                            foreach (ClrInstanceField field in stateMachine.Type.Fields)
                            {
                                // Look for awaiter fields.  This is the naming convention employed by the C# compiler.
                                if (field.Name?.StartsWith("<>u__") == true)
                                {
                                    if (field.IsObjectReference)
                                    {
                                        if (stateMachine.ReadObjectField(field.Name) is ClrObject { IsNull: false } awaiter)
                                        {
                                            if (TrySynthesizeAwaiterFrame(field))
                                            {
                                                depth++;
                                            }
                                            break;
                                        }
                                    }
                                    else if (field.IsValueType &&
                                        stateMachine.ReadValueTypeField(field.Name) is ClrValueType { IsValid: true } awaiter &&
                                        awaiter.Type is not null)
                                    {
                                        byte[] awaiterBytes = new byte[awaiter.Type.StaticSize - (runtime.DataTarget!.DataReader.PointerSize * 2)];
                                        if (runtime.DataTarget!.DataReader.Read(awaiter.Address, awaiterBytes) == awaiterBytes.Length && !AllZero(awaiterBytes))
                                        {
                                            if (TrySynthesizeAwaiterFrame(field))
                                            {
                                                depth++;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // <summary>Writes out a frame for the specified awaiter field, if possible.</summary>
                        bool TrySynthesizeAwaiterFrame(ClrInstanceField field)
                        {
                            if (field?.Name is string name)
                            {
                                if (field.IsObjectReference)
                                {
                                    IClrValue awaiter = stateMachine.ReadObjectField(name);
                                    if (awaiter.Type is not null)
                                    {
                                        writer?.Write("<< Awaiting: ");
                                        WriteAddress(awaiter.Address, writer);
                                        writer?.Write(" ");
                                        WriteAddress(awaiter.Type.MethodTable, writer);
                                        writer?.Write(awaiter.Type.Name);
                                        writer?.WriteLine(" >>");
                                        return true;
                                    }
                                }
                                else if (field.IsValueType)
                                {
                                    IClrValue awaiter = stateMachine.ReadValueTypeField(name);
                                    if (awaiter.Type is not null)
                                    {
                                        writer?.Write("<< Awaiting: ");
                                        WriteAddress(awaiter.Address, writer);
                                        writer?.Write(" ");
                                        WriteAddress(awaiter.Type.MethodTable, writer);
                                        writer?.Write($" {awaiter.Type.Name}");
                                        writer?.WriteLine(" >>");
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }
                    }

                    // Push the root node onto the stack to start the iteration.  Then as long as there are nodes left
                    // on the stack, pop the next, render it, and push any continuations it may have back onto the stack.
                    Debug.Assert(stack.Count == 0);
                    stack.Push((top, depth));
                    while (stack.Count > 0)
                    {
                        (AsyncObject frame, depth) = stack.Pop();

                        writer?.Write($"{Tabs(depth)}");
                        WriteAddress(frame.Object.Address, writer);
                        writer?.Write(" ");
                        WriteAddress(frame.Object.Type?.MethodTable ?? 0, writer);
                        writer?.Write($" {(frame.IsStateMachine ? $"({frame.AwaitState})" : $"({DescribeTaskFlags(frame.TaskStateFlags)})")} {Describe(frame.Object)}");
                        WriteAddress(frame.NativeCode,writer);
                        writer?.WriteLine();

                        if (displayFields)
                        {
                            RenderFields(frame.StateMachine ?? frame.Object, depth + 4); // +4 for extra indent for fields
                        }

                        foreach (ClrObject continuation in frame.Continuations)
                        {
                            if (objects.TryGetValue(continuation, out AsyncObject asyncContinuation))
                            {
                                stack.Push((asyncContinuation, depth + 1));
                            }
                            else
                            {
                                string state = TryGetTaskStateFlags(continuation, out int flags) ? DescribeTaskFlags(flags) : "";
                                writer?.Write($"{Tabs(depth + 1)}");
                                WriteAddress(continuation.Address,writer);
                                writer?.Write(" ");
                                WriteAddress(continuation.Type?.MethodTable ?? 0, writer);
                                writer?.WriteLine($" ({state}) {Describe(continuation)}");
                            }
                        }
                    }

                    writer?.WriteLine();
                }
            }

            // <summary>Determine whether the stack rooted in this object should be rendered.</summary>
            bool ShouldIncludeStack(AsyncObject obj)
            {
                // We want to render the stack for this object once we find any node that should be
                // included based on the criteria specified as arguments _and_ if the include tasks
                // options wasn't specified, once we find any node that's an async state machine.
                // That way, we scope the output down to just stacks that contain something the
                // user is interested in seeing.
                bool sawShouldInclude = false;
                bool sawStateMachine = includeTasks;

                Stack<AsyncObject> stack = new();
                stack.Push(obj);
                while (stack.Count > 0)
                {
                    obj = stack.Pop();
                    sawShouldInclude |= obj.IncludeInOutput;
                    sawStateMachine |= obj.IsStateMachine;

                    if (sawShouldInclude && sawStateMachine)
                    {
                        return true;
                    }

                    foreach (ClrObject continuation in obj.Continuations)
                    {
                        if (objects.TryGetValue(continuation, out AsyncObject asyncContinuation))
                        {
                            stack.Push(asyncContinuation);
                        }
                    }
                }

                return false;
            }

            // <summary>Outputs a line of information for each instance field on the object.</summary>
            void RenderFields(IClrValue? obj, int depth)
            {
                if (obj?.Type is not null)
                {
                    string depthTab = new(' ', depth * TabWidth);

                    writer?.WriteLine($"{depthTab}{"Address",16} {"MT",16} {"Type",-32} {"Value",16} Name");
                    foreach (ClrInstanceField field in obj.Type.Fields)
                    {
                        if (field.Type is not null)
                        {
                            writer?.Write($"{depthTab}");
                            if (field.IsObjectReference)
                            {
                                ClrObject objRef = field.ReadObject(obj.Address, obj.Type.IsValueType);
                                WriteAddress(objRef.Address, writer);
                            }
                            else
                            {
                                WriteAddress(field.GetAddress(obj.Address, obj.Type.IsValueType),writer);
                            }
                            writer?.Write(" ");
                            WriteAddress(field.Type.MethodTable,writer);
                            writer?.WriteLine($" {Truncate(field.Type.Name, 32),-32} {Truncate(GetDisplay(obj, field).ToString(), 16),16} {field.Name}");
                        }
                    }
                }
            }

            // <summary>Gets a printable description for the specified object.</summary>
            string Describe(ClrObject obj)
            {
                string description = string.Empty;
                if (obj.Type?.Name is not null)
                {
                    // Default the description to the type name.
                    description = obj.Type.Name;

                    if (IsStateMachineBox(obj.Type))
                    {
                        // Remove the boilerplate box type from the name.
                        int pos = description.IndexOf("StateMachineBox<", StringComparison.Ordinal);
                        if (pos >= 0)
                        {
                            ReadOnlySpan<char> slice = description.AsSpan(pos + "StateMachineBox<".Length);
                            slice = slice.Slice(0, slice.Length - 1); // remove trailing >
                            description = slice.ToString();
                        }
                    }
                    else if (TryGetValidObjectField(obj, "m_action", out ClrObject taskDelegate))
                    {
                        // If we can figure out what the task's delegate points to, append the method signature.
                        if (TryGetMethodFromDelegate(runtime, taskDelegate, out ClrMethod? method))
                        {
                            description = $"{description} {{{method!.Signature}}}";
                        }
                    }
                    else if (obj.Address != 0 && taskCompletionSentinel.Address == obj.Address)
                    {
                        description = "TaskCompletionSentinel";
                    }
                }
                return description;
            }

            // <summary>Determines whether the specified object is of interest to the user based on their criteria provided as command arguments.</summary>
            bool IncludeInOutput(ClrObject obj)
            {
                if (objectAddress is ulong addr && obj.Address != addr)
                {
                    return false;
                }

                if (obj.Type is not null)
                {
                    if (methodTableAddress is ulong mt && obj.Type.MethodTable != mt)
                    {
                        return false;
                    }

                    if (nameSubstring is not null && obj.Type.Name is not null && !obj.Type.Name.Contains(nameSubstring))
                    {
                        return false;
                    }
                }

                return true;
            }

            // <summary>Finds all of the relevant async-related objects on the heap.</summary>
            Dictionary<ClrObject, AsyncObject> CollectObjects()
            {
                Dictionary<ClrObject, AsyncObject> found = new();

                // Enumerate the heap, looking for all relevant objects.
                foreach (ClrObject obj in heap.EnumerateObjects())
                {
                    token.ThrowIfCancellationRequested();

                    if (!obj.IsValid || obj.Type is null)
                    {
                        Trace.TraceError($"(Skipping invalid object {obj})");
                        continue;
                    }

                    // Skip objects too small to be state machines or tasks, simply to help with performance.
                    if (obj.Size <= 24)
                    {
                        continue;
                    }

                    // We only care about task-related objects (all boxes are tasks).
                    if (!IsTask(obj.Type))
                    {
                        continue;
                    }

                    // This is currently working around an issue that result in enumerating segments multiple times in 6.0 runtimes
                    // up to 6.0.5. The PR that fixes it is https://github.com/dotnet/runtime/pull/67995, but we have this here for back compat.
                    if (found.ContainsKey(obj))
                    {
                        continue;
                    }

                    // If we're only going to render a summary (which only considers objects individually and not
                    // as part of chains) and if this object shouldn't be included, we don't need to do anything more.
                    if (summarize &&
                        (!IncludeInOutput(obj) || (!includeTasks && !IsStateMachineBox(obj.Type))))
                    {
                        continue;
                    }

                    // If we couldn't get state flags for the task, something's wrong; skip it.
                    if (!TryGetTaskStateFlags(obj, out int taskStateFlags))
                    {
                        continue;
                    }

                    // If we're supposed to ignore already completed tasks and this one is completed, skip it.
                    if (!includeCompleted && IsCompleted(taskStateFlags))
                    {
                        continue;
                    }

                    // Gather up the necessary data for the object and store it.
                    AsyncObject result = new()
                    {
                        Object = obj,
                        IsStateMachine = IsStateMachineBox(obj.Type),
                        IncludeInOutput = IncludeInOutput(obj),
                        TaskStateFlags = taskStateFlags,
                    };

                    if (result.IsStateMachine && TryGetStateMachine(obj, out result.StateMachine))
                    {
                        bool gotState = TryRead(result.StateMachine!, "<>1__state", out result.AwaitState);
                        Debug.Assert(gotState);

                        if (result.StateMachine?.Type is ClrType stateMachineType)
                        {
                            foreach (ClrMethod method in stateMachineType.Methods)
                            {
                                if (method.NativeCode != ulong.MaxValue)
                                {
                                    result.NativeCode = method.NativeCode;
                                    if (method.Name == "MoveNext")
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (TryGetContinuation(obj, out ClrObject continuation))
                    {
                        AddContinuation(continuation, result.Continuations);
                    }

                    found.Add(obj, result);
                }

                // Mark off objects that are referenced by others and thus aren't top level
                foreach (KeyValuePair<ClrObject, AsyncObject> entry in found)
                {
                    foreach (ClrObject continuation in entry.Value.Continuations)
                    {
                        if (found.TryGetValue(continuation, out AsyncObject asyncContinuation))
                        {
                            asyncContinuation.TopLevel = false;
                        }
                    }
                }

                return found;
            }

            // <summary>Adds the continuation into the list of continuations.</summary>
            // <remarks>
            // If the continuation is actually a List{object}, enumerate the list to add
            // each of the individual continuations to the continuations list.
            // </remarks>
            void AddContinuation(ClrObject continuation, List<ClrObject> continuations)
            {
                if (continuation.Type is not null)
                {
                    if (continuation.Type.Name is not null &&
                        continuation.Type.Name.StartsWith("System.Collections.Generic.List<", StringComparison.Ordinal))
                    {
                        if (continuation.Type.GetFieldByName("_items") is ClrInstanceField itemsField)
                        {
                            ClrObject itemsObj = itemsField.ReadObject(continuation.Address, interior: false);
                            if (!itemsObj.IsNull)
                            {
                                ClrArray items = itemsObj.AsArray();
                                if (items.Rank == 1)
                                {
                                    for (int i = 0; i < items.Length; i++)
                                    {
                                        if (items.GetObjectValue(i) is ClrObject { IsValid: true } c)
                                        {
                                            continuations.Add(ResolveContinuation(c));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        continuations.Add(continuation);
                    }
                }
            }

            // <summary>Tries to get the object contents of a Task's continuations field</summary>
            bool TryGetContinuation(ClrObject obj, out ClrObject continuation)
            {
                if (obj.Type is not null &&
                    obj.Type.GetFieldByName("m_continuationObject") is ClrInstanceField continuationObjectField &&
                    continuationObjectField.ReadObject(obj.Address, interior: false) is ClrObject { IsValid: true } continuationObject)
                {
                    continuation = ResolveContinuation(continuationObject);
                    return true;
                }

                continuation = default;
                return false;
            }

            // <summary>Analyzes a continuation object to try to follow to the actual continuation target.</summary>
            ClrObject ResolveContinuation(ClrObject continuation)
            {
                ClrObject tmp;

                // If the continuation is an async method box, there's nothing more to resolve.
                if (IsTask(continuation.Type) && IsStateMachineBox(continuation.Type))
                {
                    return continuation;
                }

                // If it's a standard task continuation, get its task field.
                if (TryGetValidObjectField(continuation, "m_task", out tmp))
                {
                    return tmp;
                }

                // If it's storing an action wrapper, try to follow to that action's target.
                if (TryGetValidObjectField(continuation, "m_action", out tmp))
                {
                    continuation = tmp;
                }

                // If we now have an Action, try to follow through to the delegate's target.
                if (TryGetValidObjectField(continuation, "_target", out tmp))
                {
                    continuation = tmp;

                    // In some cases, the delegate's target might be a ContinuationWrapper, in which case we want to unwrap that as well.
                    if (continuation.Type?.Name == "System.Runtime.CompilerServices.AsyncMethodBuilderCore+ContinuationWrapper" &&
                        TryGetValidObjectField(continuation, "_continuation", out tmp))
                    {
                        continuation = tmp;
                        if (TryGetValidObjectField(continuation, "_target", out tmp))
                        {
                            continuation = tmp;
                        }
                    }
                }

                // Use whatever we ended with.
                return continuation;
            }

            // <summary>Determines if a type is or is derived from Task.</summary>
            bool IsTask(ClrType? type)
            {
                while (type is not null)
                {
                    if (type.MetadataToken == taskType.MetadataToken &&
                        type.Module == taskType.Module)
                    {
                        return true;
                    }

                    type = type.BaseType;
                }

                return false;
            }
        }        
        

        /// <summary>Writes out an object address.  If DML is supported, this will be linked.</summary>
        /// <param name="addr">The object address.</param>
        private static void WriteAddress(ulong addr, TextWriter? writer)
        {
            if (writer != null)
            {
                switch (IntPtr.Size)
                {
                    case 4:
                        writer.Write($"{addr,16:x8}");
                        break;

                    case 5:
                        writer.Write($"{addr:x16}");
                        break;
                }
            }
        }

        /// <summary>Gets whether the specified type is an AsyncStateMachineBox{T}.</summary>
        private static bool IsStateMachineBox(ClrType? type)
        {
            // Ideally we would compare the metadata token and module for the generic template for the type,
            // but that information isn't fully available via ClrMd, nor can it currently find DebugFinalizableAsyncStateMachineBox
            // due to various limitations.  So we're left with string comparison.
            const string Prefix = "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<";
            return
                type?.Name is string name &&
                name.StartsWith(Prefix, StringComparison.Ordinal) &&
                name.IndexOf("AsyncStateMachineBox", Prefix.Length, StringComparison.Ordinal) >= 0;
        }

        /// <summary>Tries to get the compiler-generated state machine instance from a state machine box.</summary>
        private static bool TryGetStateMachine(ClrObject obj, out IClrValue? stateMachine)
        {
            // AsyncStateMachineBox<T> has a StateMachine field storing the compiler-generated instance.
            if (obj.Type?.GetFieldByName("StateMachine") is ClrInstanceField field)
            {
                if (field.IsValueType)
                {
                    if (obj.ReadValueTypeField("StateMachine") is ClrValueType { IsValid: true } t)
                    {
                        stateMachine = t;
                        return true;
                    }
                }
                else if (field.ReadObject(obj.Address, interior: false) is ClrObject { IsValid: true } t)
                {
                    stateMachine = t;
                    return true;
                }
            }

            stateMachine = null;
            return false;
        }

        /// <summary>Extract from the specified field of the specified object something that can be ToString'd.</summary>
        private static object GetDisplay(IClrValue obj, ClrInstanceField field)
        {
            if (field.Name is string fieldName)
            {
                switch (field.ElementType)
                {
                    case ClrElementType.Boolean:
                        return obj.ReadField<bool>(fieldName) ? "true" : "false";

                    case ClrElementType.Char:
                        char c = obj.ReadField<char>(fieldName);
                        return c >= 32 && c < 127 ? $"'{c}'" : $"'\\u{(int)c:X4}'";

                    case ClrElementType.Int8:
                        return obj.ReadField<sbyte>(fieldName);

                    case ClrElementType.UInt8:
                        return obj.ReadField<byte>(fieldName);

                    case ClrElementType.Int16:
                        return obj.ReadField<short>(fieldName);

                    case ClrElementType.UInt16:
                        return obj.ReadField<ushort>(fieldName);

                    case ClrElementType.Int32:
                        return obj.ReadField<int>(fieldName);

                    case ClrElementType.UInt32:
                        return obj.ReadField<uint>(fieldName);

                    case ClrElementType.Int64:
                        return obj.ReadField<long>(fieldName);

                    case ClrElementType.UInt64:
                        return obj.ReadField<ulong>(fieldName);

                    case ClrElementType.Float:
                        return obj.ReadField<float>(fieldName);

                    case ClrElementType.Double:
                        return obj.ReadField<double>(fieldName);

                    case ClrElementType.String:
                        return $"\"{obj.ReadStringField(fieldName)}\"";

                    case ClrElementType.Pointer:
                    case ClrElementType.NativeInt:
                    case ClrElementType.NativeUInt:
                    case ClrElementType.FunctionPointer:
                        return obj.ReadField<ulong>(fieldName).ToString(IntPtr.Size == 8 ? "x16" : "x8");

                    case ClrElementType.SZArray:
                        IClrValue arrayObj = obj.ReadObjectField(fieldName);
                        if (!arrayObj.IsNull)
                        {
                            IClrArray arrayObjAsArray = arrayObj.AsArray();
                            return $"{arrayObj.Type?.ComponentType?.ToString() ?? "unknown"}[{arrayObjAsArray.Length}]";
                        }
                        return "null";

                    case ClrElementType.Struct:
                        return field.GetAddress(obj.Address).ToString(IntPtr.Size == 8 ? "x16" : "x8");

                    case ClrElementType.Array:
                    case ClrElementType.Object:
                    case ClrElementType.Class:
                        IClrValue classObj = obj.ReadObjectField(fieldName);
                        return classObj.IsNull ? "null" : classObj.Address.ToString(IntPtr.Size == 8 ? "x16" : "x8");

                    case ClrElementType.Var:
                        return "(var)";

                    case ClrElementType.GenericInstantiation:
                        return "(generic instantiation)";

                    case ClrElementType.MVar:
                        return "(mvar)";

                    case ClrElementType.Void:
                        return "(void)";
                }
            }

            return "(unknown)";
        }

        /// <summary>Tries to get a ClrMethod for the method wrapped by a delegate object.</summary>
        private static bool TryGetMethodFromDelegate(ClrRuntime runtime, ClrObject delegateObject, out ClrMethod? method)
        {
            ClrInstanceField? methodPtrField = delegateObject.Type?.GetFieldByName("_methodPtr");
            ClrInstanceField? methodPtrAuxField = delegateObject.Type?.GetFieldByName("_methodPtrAux");

            if (methodPtrField is not null && methodPtrAuxField is not null)
            {
                ulong methodPtr = methodPtrField.Read<UIntPtr>(delegateObject.Address, interior: false).ToUInt64();
                if (methodPtr != 0)
                {
                    method = runtime.GetMethodByInstructionPointer(methodPtr);
                    if (method is null)
                    {
                        methodPtr = methodPtrAuxField.Read<UIntPtr>(delegateObject.Address, interior: false).ToUInt64();
                        if (methodPtr != 0)
                        {
                            method = runtime.GetMethodByInstructionPointer(methodPtr);
                        }
                    }

                    return method is not null;
                }
            }

            method = null;
            return false;
        }

        /// <summary>Creates an indenting string.</summary>
        /// <param name="count">The number of tabs.</param>
        private static string Tabs(int count) => new(' ', count * TabWidth);

        /// <summary>Shortens a string to a maximum length by eliding part of the string with ...</summary>
        private static string? Truncate(string? value, int maxLength)
        {
            if (value is not null && value.Length > maxLength)
            {
                value = $"...{value.Substring(value.Length - maxLength + 3)}";
            }

            return value;
        }

        /// <summary>Tries to get the state flags from a task.</summary>
        private static bool TryGetTaskStateFlags(ClrObject obj, out int flags)
        {
            if (obj.Type?.GetFieldByName("m_stateFlags") is ClrInstanceField field)
            {
                flags = field.Read<int>(obj.Address, interior: false);
                return true;
            }

            flags = 0;
            return false;
        }

        /// <summary>Tries to read the specified value from the field of an entity.</summary>
        private static bool TryRead<T>(IClrValue entity, string fieldName, out T result) where T : unmanaged
        {
            if (entity.Type?.GetFieldByName(fieldName) is not null)
            {
                result = entity.ReadField<T>(fieldName);
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>Tries to read an object from a field of another object.</summary>
        private static bool TryGetValidObjectField(ClrObject obj, string fieldName, out ClrObject result)
        {
            if (obj.Type?.GetFieldByName(fieldName) is ClrInstanceField field &&
                field.ReadObject(obj.Address, interior: false) is { IsValid: true } validObject)
            {
                result = validObject;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>Gets whether a task has completed, based on its state flags.</summary>
        private static bool IsCompleted(int taskStateFlags)
        {
            const int TASK_STATE_COMPLETED_MASK = 0x1600000;
            return (taskStateFlags & TASK_STATE_COMPLETED_MASK) != 0;
        }

        /// <summary>Determines whether a span contains all zeros.</summary>
        private static bool AllZero(ReadOnlySpan<byte> bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>Gets a string representing interesting aspects of the specified task state flags.</summary>
        /// <remarks>
        /// The goal of this method isn't to detail every flag value (there are a lot).
        /// Rather, we only want to render flags that are likely to be valuable, e.g.
        /// we don't render WaitingForActivation, as that's the expected state for any
        /// task that's showing up in a stack.
        /// </remarks>
        private static string DescribeTaskFlags(int stateFlags)
        {
            if (stateFlags != 0)
            {
                StringBuilder? sb = null;
                void Append(string s)
                {
                    sb ??= new StringBuilder();
                    if (sb.Length != 0)
                    {
                        sb.Append('|');
                    }

                    sb.Append(s);
                }

                if ((stateFlags & 0x10000) != 0) { Append("Started"); }
                if ((stateFlags & 0x20000) != 0) { Append("DelegateInvoked"); }
                if ((stateFlags & 0x40000) != 0) { Append("Disposed"); }
                if ((stateFlags & 0x80000) != 0) { Append("ExceptionObservedByParent"); }
                if ((stateFlags & 0x100000) != 0) { Append("CancellationAcknowledged"); }
                if ((stateFlags & 0x200000) != 0) { Append("Faulted"); }
                if ((stateFlags & 0x400000) != 0) { Append("Canceled"); }
                if ((stateFlags & 0x800000) != 0) { Append("WaitingOnChildren"); }
                if ((stateFlags & 0x1000000) != 0) { Append("RanToCompletion"); }
                if ((stateFlags & 0x4000000) != 0) { Append("CompletionReserved"); }

                if (sb is not null)
                {
                    return sb.ToString();
                }
            }

            return " ";
        }

    }
}
