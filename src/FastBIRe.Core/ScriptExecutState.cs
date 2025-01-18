namespace FastBIRe
{
    public enum ScriptExecutState
    {
        Begin,
        CreatedCommand,
        LoaedCommand,
        Executed,
        ExecutedBatch,
        CreatedBatch,
        LoadBatchItem,
        Exception,
        Skip,
        StartReading,
        EndReading,
        BeginTransaction,
        CommitedTransaction,
        RollbackedTransaction
    }
}
