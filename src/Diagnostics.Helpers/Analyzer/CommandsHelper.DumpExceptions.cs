using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Diagnostics.Helpers.Analyzer
{
    public static partial class CommandsHelper
    {
        public static CLRExceptionCollections DumpExceptions(ClrRuntime runtime, Generation? generation = null, bool live = false, bool dead = false,Func<IEnumerable<ClrObject>,IEnumerable<ClrObject>>? filter=null, CancellationToken token = default)
        {
            var filteredHeap = CLRExceptionCollections.CreateHeapWithFilters(runtime.Heap, generation);

            IEnumerable<ClrObject> exceptionObjects =
                filteredHeap.EnumerateFilteredObjects(token)
                    .Where(obj => obj.IsException);

            LiveObjectService? liveObjectService = null;

            if (live)
            {
                liveObjectService ??= new LiveObjectService(runtime, token);
                exceptionObjects = exceptionObjects.Where(obj => liveObjectService.IsLive(obj));
            }

            if (dead)
            {
                liveObjectService ??= new LiveObjectService(runtime, token);
                exceptionObjects = exceptionObjects.Where(obj => !liveObjectService.IsLive(obj));
            }

            if (filter!=null)
            {
                exceptionObjects = filter(exceptionObjects);
            }
            return new CLRExceptionCollections(filteredHeap, exceptionObjects);
        }
    }
}
