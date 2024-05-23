using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.DuckDB
{
    internal static class ArrayHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Array<T>(int size)
        {
#if NETSTANDARD2_0
            return new T[size];
#else
            return GC.AllocateUninitializedArray<T>(size);
#endif
        }
    }
}
