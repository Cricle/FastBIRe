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
            else if (isMysqlGtid)
            {
                var uuid = new Uuid(data.AsSpan(2,16).ToArray());
                var serviceId = BitConverter.ToInt64(data, 2 + 16);
                var transId = BitConverter.ToInt64(data, 2 + 16+sizeof(long));
                return new MySqlCheckpoint(new MySqlCdc.Providers.MySql.Gtid(uuid, transId), serviceId);
            }
            else
            {
                var domainId = BitConverter.ToInt64(data, 2 + 16);
                var serviceId = BitConverter.ToInt64(data, 2 + 16*2);
                var sequence = BitConverter.ToInt64(data, 2 + 16*3);
                return new MySqlCheckpoint(new MySqlCdc.Providers.MariaDb.Gtid(domainId, serviceId, sequence), 0);
            }
        }
    }
}
