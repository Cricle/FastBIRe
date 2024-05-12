using LiteDB;

namespace Diagnostics.Traces.LiteDb
{
    public class DelegateDatabaseSelector : ILiteDatabaseSelector
    {

        public Func<TraceTypes, Action<LiteDatabaseCreatedResult>> Getter { get; }

        public DelegateDatabaseSelector(Func<TraceTypes, Action<LiteDatabaseCreatedResult>> getter)
        {
            Getter = getter;
        }

        public void ReportInserted(TraceTypes type, int count)
        {
        }

        public void UsingDatabaseResult(TraceTypes type, Action<LiteDatabaseCreatedResult> @using)
        {
            throw new NotImplementedException();
        }
    }
}
