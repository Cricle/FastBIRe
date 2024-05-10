using LiteDB;

namespace Diagnostics.Traces.LiteDb
{
    public class DelegateDatabaseSelector<TIdentity> : ILiteDatabaseSelector<TIdentity>
    {
        public DelegateDatabaseSelector(Func<TraceTypes, ILiteDatabase> databaseFactory)
        {
            DatabaseFactory = databaseFactory;
        }
        public Func<TraceTypes, ILiteDatabase> DatabaseFactory { get; }

        public ILiteDatabase GetLiteDatabase(TraceTypes type)
        {
            return DatabaseFactory(type);
        }
    }
}
