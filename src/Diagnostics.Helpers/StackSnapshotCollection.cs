﻿using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Diagnostics.Helpers
{
    public record class StackSnapshotCollection : IDisposable
    {
        public StackSnapshotCollection(DataTarget dataTarget, IReadOnlyList<StackSnapshot> stacks)
        {
            DataTarget = dataTarget;
            Stacks = stacks;
        }

        public DataTarget DataTarget { get; }

        public IReadOnlyList<StackSnapshot> Stacks { get; }

        public void Dispose()
        {
            DataTarget.Dispose();
        }
        public override string ToString()
        {
            var s = new StringBuilder();
            foreach (var item in Stacks)
            {
                s.AppendLine(item.ToString());
            }
            return s.ToString();
        }
    }
}