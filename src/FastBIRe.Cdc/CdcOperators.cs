using System;

namespace FastBIRe.Cdc
{
    [Flags]
    public enum CdcOperators
    {
        None = 0,
        EnableDatabaseCdc = 1,
        EnableTableCdc = EnableDatabaseCdc << 1,
        DisableDatabaseCdc = EnableDatabaseCdc << 2,
        DisableTableCdc = EnableDatabaseCdc << 3,
        CheckDatabaseSupportCdc = EnableDatabaseCdc << 4,
        CheckDatabaseEnableCdc = EnableDatabaseCdc << 5,
        CheckTableEnableCdc = EnableDatabaseCdc << 6,
        GetLastCheckPoint= EnableDatabaseCdc<<7,
        All = EnableDatabaseCdc | EnableTableCdc | DisableDatabaseCdc | DisableTableCdc | CheckDatabaseSupportCdc | CheckDatabaseEnableCdc | CheckTableEnableCdc| GetLastCheckPoint,
        WithoutEnableDisable = All & ~EnableDatabaseCdc & ~EnableTableCdc & ~DisableDatabaseCdc & ~DisableTableCdc
    }
}
