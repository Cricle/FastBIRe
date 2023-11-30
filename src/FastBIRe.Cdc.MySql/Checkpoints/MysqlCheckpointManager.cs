using FastBIRe.Cdc.Checkpoints;
using MySqlCdc.Providers.MySql;
using System;
using System.Text;

namespace FastBIRe.Cdc.MySql.Checkpoints
{
    public class MysqlCheckpointManager : ICheckPointManager
    {
        public static readonly MysqlCheckpointManager Instance = new MysqlCheckpointManager();

        ICheckpoint ICheckPointManager.CreateCheckpoint(byte[] data)
        {
            return CreateCheckpoint(data);
        }

        public MySqlCheckpoint CreateCheckpoint(byte[] data)
        {
            var isGtid = data[0] == 1;
            var isMysqlGtid = data[1] == 1;
            if (!isGtid)
            {
                var pos = BitConverter.ToInt64(data, 2);
                string? fn = null;
                if (data.Length != sizeof(long))
                {
                    fn = Encoding.UTF8.GetString(data, sizeof(long) + 2, (int)(pos - sizeof(long)));
                }
                return new MySqlCheckpoint(pos, fn);
            }
            else
            {
                var str = Encoding.UTF8.GetString(data, 2, data.Length - 2);
                if (isMysqlGtid)
                {
                    return new MySqlCheckpoint(MySqlCdc.Providers.MySql.GtidSet.Parse(str));
                }
                else
                {
                    return new MySqlCheckpoint(MySqlCdc.Providers.MariaDb.GtidList.Parse(str));
                }
            }
        }
    }
}
