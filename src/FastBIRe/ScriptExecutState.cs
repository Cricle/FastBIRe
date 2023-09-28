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
        BatchException,
        Skip,
        StartReading,
        EndReading
    }
}
