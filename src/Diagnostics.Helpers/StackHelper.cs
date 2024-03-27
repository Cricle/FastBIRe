using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;

namespace Diagnostics.Helpers
{
    public static class StackHelper
    {
        //https://github.com/microsoft/clrmd/blob/main/src/Samples/ClrStack/ClrStack.cs
        public static StackSnapshotCollection GetStackSnapshots()
        {
            var dt = DataTarget.AttachToProcess(PlatformHelper.CurrentProcessId, false);
            return GetStackSnapshots(dt);
        }
        public static StackSnapshotCollection GetStackSnapshots(int processId)
        {
            var dt = DataTarget.AttachToProcess(processId, false);
            return GetStackSnapshots(dt);
        }
        public static StackSnapshotCollection GetStackSnapshots(DataTarget dataTarget)
        {
            var isTarget64Bit = dataTarget.DataReader.PointerSize == 8;
            if (PlatformHelper.Is64Bit != isTarget64Bit)
            {
                throw new Exception(string.Format("Architecture mismatch:  Process is {0} but target is {1}", PlatformHelper.Is64Bit ? "64 bit" : "32 bit", isTarget64Bit ? "64 bit" : "32 bit"));
            }
            var stacks = new List<StackSnapshot>();
            foreach (var version in dataTarget.ClrVersions)
            {
                stacks.Add(new StackSnapshot(version));
            }
            return new StackSnapshotCollection(dataTarget, stacks);
        }
    }
}