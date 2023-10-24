using FastBIRe.Annotations;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public static class RecordToObjectManager<T>
    {
        private static IRecordToObject<T> recordToObject = null!;

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
            var type = typeof(T);
            if (type.IsPrimitive || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) || type == typeof(string) || type == typeof(decimal))
            {
                recordToObject = PrimitiveRecordToObject<T>.Instance;
            }
            else
            {
                var attr = type.GetCustomAttribute<RecordToAttribute>();
                if (attr != null)
                {
                    recordToObject = (IRecordToObject<T>)Activator.CreateInstance(attr.RecordToObjectType);
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
