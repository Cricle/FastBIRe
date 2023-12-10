using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    internal sealed class ReflectionRecordToObject<T> : IRecordToObject<T>
    {
        public static readonly ReflectionRecordToObject<T> Instance = new ReflectionRecordToObject<T>();

        private ReflectionRecordToObject() { }

        public T? To(IDataRecord record)
        {
            return ObjectMapper<T>.Fill(record);
        }

        public IList<T?> ToList(IDataReader reader)
        {
            return ObjectMapper<T?>.FillList(reader);
        }
    }
}
