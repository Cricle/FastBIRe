using FastBIRe.Cdc.Checkpoints;
using System.Text;

namespace FastBIRe.Cdc.NpgSql.Checkpoints
{
    public class PgSqlCheckpoint : ICheckpoint
    {
        public PgSqlCheckpoint(string? currentLsn, string? fileName)
        {
            CurrentLsn = currentLsn;
            FileName = fileName;
        }

        public string? CurrentLsn { get; }

        public string? FileName { get; }

        public bool IsEmpty => CurrentLsn != null && FileName != null;

        public override string ToString()
        {
            return $"{{FileName: {FileName}, Lsn: {CurrentLsn}}}";
        }
        public byte[] ToBytes()
        {
            var buffer = new List<byte>();
            if (string.IsNullOrEmpty(CurrentLsn))
            {
                buffer.AddRange(BitConverter.GetBytes(0));
            }
            else
            {
                buffer.AddRange(BitConverter.GetBytes(CurrentLsn!.Length));
                buffer.AddRange(Encoding.UTF8.GetBytes(CurrentLsn));
            }
            if (string.IsNullOrEmpty(FileName))
            {
                buffer.AddRange(BitConverter.GetBytes(0));
            }
            else
            {
                buffer.AddRange(BitConverter.GetBytes(FileName!.Length));
                buffer.AddRange(Encoding.UTF8.GetBytes(FileName));
            }
            return buffer.ToArray();
        }
        public static PgSqlCheckpoint FromBytes(byte[] bytes)
        {
            string? lsn = null;
            string? fileName=null;

            var lsnLength = BitConverter.ToInt32(bytes, 0);
            if (lsnLength != 0)
            {
                lsn = Encoding.UTF8.GetString(bytes, sizeof(int), lsnLength);
            }
            lsnLength = BitConverter.ToInt32(bytes, sizeof(int) + lsnLength);
            if (lsnLength != 0)
            {
                var offset = lsnLength + sizeof(int) * 2;
                fileName = Encoding.UTF8.GetString(bytes, offset, bytes.Length - offset);
            }
            return new PgSqlCheckpoint(lsn, fileName);
        }
    }
}
