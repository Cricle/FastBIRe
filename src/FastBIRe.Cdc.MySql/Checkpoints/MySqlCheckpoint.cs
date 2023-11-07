using FastBIRe.Cdc.Checkpoints;
using MySqlCdc.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.Cdc.MySql.Checkpoints
{
    public class MySqlCheckpoint : ICheckpoint
    {
        public MySqlCheckpoint(long position, string? fileName)
        {
            Position = position;
            FileName = fileName;
        }

        public MySqlCheckpoint(IGtid? gtid, long serviceId)
        {
            Gtid = gtid;
            ServiceId = serviceId;
        }

        public long Position { get; }

        public string? FileName { get; }

        public IGtid? Gtid { get; }

        public long ServiceId { get; }

        public bool IsEmpty => Gtid == null && Position == 0;

        public IGtidState? ToGtidState()
        {
            if (Gtid is MySqlCdc.Providers.MySql.Gtid mgtid)
            {
                return MySqlCdc.Providers.MySql.GtidSet.Parse($"{mgtid.SourceId}:{ServiceId}-{mgtid.TransactionId}");
            }
            else if (Gtid is MySqlCdc.Providers.MariaDb.Gtid magtid)
            {
                return new MySqlCdc.Providers.MariaDb.GtidList { Gtids = { magtid } };
            }
            return null;
        }

        public override string? ToString()
        {
            if (Gtid!=null)
            {
                return ToGtidState()?.ToString();
            }
            return $"{{FileName: {FileName}, Pos: {Position}}}";
        }
        public byte[] ToBytes()
        {
            var buffer = new List<byte>
            {
                //Is gtidMode
                (byte)(Gtid == null ? 0 : 1),
                //Is mysql or mariadb
                (byte)(Gtid is MySqlCdc.Providers.MySql.Gtid?1:0)
            };
            if (Gtid == null)
            {
                buffer.AddRange(BitConverter.GetBytes(Position));
                if (!string.IsNullOrEmpty(FileName))
                {
                    buffer.AddRange(Encoding.UTF8.GetBytes(FileName));
                }
            }
            else if(Gtid is MySqlCdc.Providers.MySql.Gtid mysqlGtid)
            {
                buffer.AddRange(mysqlGtid.SourceId.ToByteArray());
                buffer.AddRange(BitConverter.GetBytes(ServiceId));
                buffer.AddRange(BitConverter.GetBytes(mysqlGtid.TransactionId));
            }
            else if (Gtid is MySqlCdc.Providers.MariaDb.Gtid mariaGtid)
            {
                buffer.AddRange(BitConverter.GetBytes(mariaGtid.DomainId));
                buffer.AddRange(BitConverter.GetBytes(mariaGtid.ServerId));
                buffer.AddRange(BitConverter.GetBytes(mariaGtid.Sequence));
            }
            return buffer.ToArray();
        }
    }
}
