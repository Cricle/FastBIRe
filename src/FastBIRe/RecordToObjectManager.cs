using FastBIRe.Annotations;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public static class RecordToObjectManager<T>
    {
        private static readonly bool IsPrimitiveOrNullable = typeof(T).IsPrimitive || (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>)) || typeof(T) == typeof(string) || typeof(T) == typeof(decimal);

        private static readonly bool IsArray = typeof(T).IsArray||
            (typeof(T).IsGenericType&& typeof(T).GetGenericTypeDefinition()==typeof(IList<>)) || 
            (typeof(T).IsGenericType&& typeof(T).GetInterfaces().Any(x=>x.IsGenericType&&x.GetGenericTypeDefinition()==typeof(IList<>)));

        internal static IRecordToObject<T> recordToObject = null!;

        public static IRecordToObject<T> RecordToObject => recordToObject;

        public static void SetRecordToObject(IRecordToObject<T> recordToObject)
        {
            RecordToObjectManager<T>.recordToObject = recordToObject ?? throw new ArgumentNullException(nameof(recordToObject));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? To(IDataRecord record)
        {
            return recordToObject.To(record);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<T?> ToList(IDataReader reader)
        {
            return recordToObject.ToList(reader);
        }

        public static void Reset()
        {
            if (IsPrimitiveOrNullable)
            {
                recordToObject = PrimitiveRecordToObject<T>.Instance;
            }
            else
            {
                var attr = typeof(T).GetCustomAttribute<RecordToAttribute>();
                if (attr != null)
                {
                    recordToObject = (IRecordToObject<T>)Activator.CreateInstance(attr.RecordToObjectType)!;
                }
                else
                {
                    recordToObject = ReflectionRecordToObject<T>.Instance;
                }
            }
            Debug.Assert(recordToObject != null);
        }

        static RecordToObjectManager()
        {
            Reset();
        }
    }
}
