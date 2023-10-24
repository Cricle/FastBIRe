using System.Data;

namespace FastBIRe
{
    public interface IRecordToObject<T>
    {
        T? To(IDataRecord record);

        IList<T?> ToList(IDataReader reader);
    }
}
