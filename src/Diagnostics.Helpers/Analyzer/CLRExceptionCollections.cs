using Diagnostics.Helpers.Analyzer.Output;
using Microsoft.Diagnostics.Runtime;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace Diagnostics.Helpers.Analyzer
{
    public class CLRExceptionCollections:List<ClrObject>
    {
        public CLRExceptionCollections(HeapWithFilters heapWithFilters)
        {
            HeapWithFilters = heapWithFilters;
        }
        public CLRExceptionCollections(HeapWithFilters heapWithFilters,IEnumerable<ClrObject> objects)
            : base(objects) 
        {
            HeapWithFilters = heapWithFilters;
        }

        public HeapWithFilters HeapWithFilters { get; }

        public override string ToString()
        {
            var s = new StringBuilder();
            PrintExceptions(new StringWriter(s), this);
            return s.ToString();
        }

        public static void PrintExceptions(TextWriter writer,IEnumerable<ClrObject> exceptionObjects)
        {
            Table output = new(writer, ColumnKind.Pointer, ColumnKind.Pointer, ColumnKind.TypeName);
            output.WriteHeader("Address", "MethodTable", "Message", "Name");

            int totalExceptions = 0;
            foreach (ClrObject exceptionObject in exceptionObjects)
            {
                totalExceptions++;

                ClrException clrException = exceptionObject.AsException()!;
                output.WriteRow(exceptionObject.Address, exceptionObject.Type!.MethodTable, exceptionObject.Type!.Name!);

                writer.Write("        Message: ");
                writer.WriteLine(clrException.Message ?? "<null>");

                ImmutableArray<ClrStackFrame> stackTrace = clrException.StackTrace;
                if (stackTrace.Length > 0)
                {
                    writer.Write("        StackFrame: ");
                    writer.WriteLine(stackTrace[0].ToString());
                }
            }

            writer.WriteLine();
            writer.WriteLine($"    Total: {totalExceptions} objects");
        }

        public static HeapWithFilters CreateHeapWithFilters(ClrHeap clrHeap, Generation? generation)
        {
            HeapWithFilters filteredHeap = new(clrHeap);
            if (generation != null)
            {

                filteredHeap.Generation = generation;
            }
            return filteredHeap;
        }
    }
}
