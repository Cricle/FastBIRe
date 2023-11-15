using FastBIRe.Cdc.Checkpoints;
using MySqlCdc.Events;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public MySqlCheckpoint(IGtid? gtid)
        {
            Gtid = gtid;
        }

        public long Position { get; }

        public string? FileName { get; }

        public IGtid? Gtid { get; }

        public bool IsEmpty => Gtid == null && Position == 0;

        public static MySqlCheckpoint Parse(string gtid,bool isMariaDB)
        {
            if (isMariaDB)
            {
                var mariaDBId = MySqlCdc.Providers.MariaDb.GtidList.Parse(gtid).Gtids[0];
                return new MySqlCheckpoint(mariaDBId);
            }
            var mysqlDBId = MySqlCdc.Providers.MySql.GtidSet.Parse(gtid);
            var set = mysqlDBId.UuidSets.First().Value;
            return new MySqlCheckpoint(new MySqlCdc.Providers.MySql.Gtid(
                set.SourceId,
                set.Intervals.Last().End));
        }

        public IGtidState? ToGtidState()
        {
            if (Gtid is MySqlCdc.Providers.MySql.Gtid mgtid)
            {
                return MySqlCdc.Providers.MySql.GtidSet.Parse($"{mgtid.SourceId}:{mgtid.TransactionId}-{mgtid.TransactionId}");
            }
            else if (Gtid is MySqlCdc.Providers.MariaDb.Gtid magtid)
            {
                return new MySqlCdc.Providers.MariaDb.GtidList { Gtids = { magtid } };
            }
            return null;
        }

        public override string? ToString()
        {
            if (Gtid != null)
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
            else if (Gtid is MySqlCdc.Providers.MySql.Gtid mysqlGtid)
            {
                buffer.AddRange(mysqlGtid.SourceId.ToByteArray());
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
