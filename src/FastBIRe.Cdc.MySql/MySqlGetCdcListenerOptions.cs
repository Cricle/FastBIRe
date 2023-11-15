using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.MySql.Checkpoints;
using MySqlCdc;
using System;
using System.Collections.Generic;

namespace FastBIRe.Cdc.MySql
{
    public class MySqlGetCdcListenerOptions : GetCdcListenerOptions
    {
        public MySqlGetCdcListenerOptions(IReadOnlyList<string>? tableNames, ICheckpoint? checkpoint, Action<ReplicaOptions> replicaOptionsAction)
            : base(tableNames, checkpoint)
        {
            ReplicaOptionsAction = opt =>
            {
                if (checkpoint is MySqlCheckpoint cp)
                {
                    var state = cp.ToGtidState();
                    if (state != null)
                    {
                        opt.Binlog = BinlogOptions.FromGtid(state);
                    }
                }
                replicaOptionsAction(opt);
            };

        }

        public Action<ReplicaOptions> ReplicaOptionsAction { get; }
    }
}
