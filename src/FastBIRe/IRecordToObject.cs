using System.Data;

namespace FastBIRe
{
    public interface IRecordToObject<T>
    {
        T? To(IDataRecord record);

        IEnumerable<T?> Enumerable(IDataReader reader);

        IList<T?> ToList(IDataReader reader);
    }
}
