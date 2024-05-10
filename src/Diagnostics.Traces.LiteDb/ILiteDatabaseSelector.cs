using LiteDB;

namespace Diagnostics.Traces.LiteDb
{
    public interface ILiteDatabaseSelector<TIdentity>
    {
        ILiteDatabase GetLiteDatabase(TraceTypes type);
    }
}
