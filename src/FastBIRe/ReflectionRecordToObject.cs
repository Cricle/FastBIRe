using System.Data;

namespace FastBIRe
{
    internal sealed class ReflectionRecordToObject<T> : IRecordToObject<T>
    {
        public static readonly ReflectionRecordToObject<T> Instance = new ReflectionRecordToObject<T>();

        private ReflectionRecordToObject() { }

        public IEnumerable<T?> Enumerable(IDataReader reader)
        {
            while (reader.Read())
            {
                yield return To(reader);
            }
        }

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
