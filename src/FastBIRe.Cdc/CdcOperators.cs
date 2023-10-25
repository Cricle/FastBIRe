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
        All = EnableDatabaseCdc | EnableTableCdc | DisableDatabaseCdc | DisableTableCdc | CheckDatabaseSupportCdc | CheckDatabaseEnableCdc | CheckTableEnableCdc,
        WithoutEnableDisable=All & ~EnableDatabaseCdc & ~EnableTableCdc & ~DisableDatabaseCdc & ~DisableTableCdc
    }
}
