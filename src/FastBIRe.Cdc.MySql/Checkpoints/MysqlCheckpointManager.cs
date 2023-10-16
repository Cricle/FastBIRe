using FastBIRe.Cdc.Checkpoints;
using System;
using System.Text;

namespace FastBIRe.Cdc.MySql.Checkpoints
{
    public class MysqlCheckpointManager : ICheckPointManager
    {
        public static readonly MysqlCheckpointManager Instance = new MysqlCheckpointManager();

        public ICheckpoint CreateCheckpoint(byte[] data)
        {
            var pos=BitConverter.ToInt64(data);
            var fn = Encoding.UTF8.GetString(data, sizeof(long), (int)(pos - sizeof(long)));
            return new MySqlCheckpoint(pos, fn);
        }
    }
}
