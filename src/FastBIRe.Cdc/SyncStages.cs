namespace FastBIRe.Cdc
{
    public enum SyncStages
    {
        SyncingStruct,
        SyncedStruct,
        FetchingCheckpoint,
        FetchedCheckpoint,
        DeletingTargetDatas,
        DeletedTargetDatas,
        SyncingData,
        SyncedData
    }
}
