using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.MySql.Checkpoints;
using MySqlCdc;
using System;
using System.Collections.Generic;

namespace FastBIRe.Cdc.MySql
{
    public class MySqlGetCdcListenerOptions : GetCdcListenerOptions
    {
        public MySqlGetCdcListenerOptions(ICheckpoint? checkpoint, Action<ReplicaOptions> replicaOptionsAction)
            : base(checkpoint)
        {
            ReplicaOptionsAction = opt =>
            {
                if (checkpoint is MySqlCheckpoint cp)
                {
                    if (cp.GtidState != null)
                    {
                        opt.Binlog = BinlogOptions.FromGtid(cp.GtidState);
                    }
                }
                replicaOptionsAction(opt);
            };

        }

        public Action<ReplicaOptions> ReplicaOptionsAction { get; }
    }
}
