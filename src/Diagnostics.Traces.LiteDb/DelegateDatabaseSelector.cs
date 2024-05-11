using LiteDB;

namespace Diagnostics.Traces.LiteDb
{
    public class DelegateDatabaseSelector : ILiteDatabaseSelector
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

        public void ReportInserted(TraceTypes type, int count)
        {
        }
    }
}
