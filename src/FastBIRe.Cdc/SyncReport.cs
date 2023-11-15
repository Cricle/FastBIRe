using System;

namespace FastBIRe.Cdc
{
    public readonly struct SyncReport
    {
        public SyncReport(SyncStages stage, TimeSpan? time)
        {
            Stage = stage;
            Time = time;
        }

        public SyncStages Stage { get; }

        public TimeSpan? Time { get; }
    }
}
