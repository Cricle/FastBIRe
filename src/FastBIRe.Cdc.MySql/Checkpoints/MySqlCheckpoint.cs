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

        public MySqlCheckpoint(IGtidState? gtid)
        {
            GtidState = gtid;
        }

        public long Position { get; }

        public string? FileName { get; }

        public IGtidState? GtidState { get; }

        public bool IsEmpty => GtidState == null && Position == 0;

        public static MySqlCheckpoint Parse(string gtid,bool isMariaDB)
        {
            if (isMariaDB)
            {
                var mariaDBId = MySqlCdc.Providers.MariaDb.GtidList.Parse(gtid);
                return new MySqlCheckpoint(mariaDBId);
            }
            var mysqlDBId = MySqlCdc.Providers.MySql.GtidSet.Parse(gtid);
            return new MySqlCheckpoint(mysqlDBId);
        }

        public IGtidState? ToGtidState()
        {
            if (GtidState is MySqlCdc.Providers.MySql.Gtid mgtid)
            {
                return MySqlCdc.Providers.MySql.GtidSet.Parse($"{mgtid.SourceId}:1-{mgtid.TransactionId}");
            }
            else if (GtidState is MySqlCdc.Providers.MariaDb.Gtid magtid)
            {
                return new MySqlCdc.Providers.MariaDb.GtidList { Gtids = { magtid } };
            }
            return null;
        }

        public override string? ToString()
        {
            if (GtidState != null)
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
                (byte)(GtidState == null ? 0 : 1),
                //Is mysql or mariadb
                (byte)(GtidState is MySqlCdc.Providers.MySql.GtidSet?1:0)
            };
            if (GtidState == null)
            {
                buffer.AddRange(BitConverter.GetBytes(Position));
                if (!string.IsNullOrEmpty(FileName))
                {
                    buffer.AddRange(Encoding.UTF8.GetBytes(FileName));
                }
            }
            else
            {
                buffer.AddRange(Encoding.UTF8.GetBytes(GtidState.ToString()!));
            }
            return buffer.ToArray();
        }
    }
}
