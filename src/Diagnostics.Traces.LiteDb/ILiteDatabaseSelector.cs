namespace Diagnostics.Traces.LiteDb
{
    public interface ILiteDatabaseSelector
    {
        void UsingDatabaseResult(TraceTypes type,Action<LiteDatabaseCreatedResult> @using);

        void ReportInserted(TraceTypes type, int count);
    }
}
