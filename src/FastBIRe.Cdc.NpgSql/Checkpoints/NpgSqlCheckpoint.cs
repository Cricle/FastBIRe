using FastBIRe.Cdc.Checkpoints;
using System.Text;

namespace FastBIRe.Cdc.NpgSql.Checkpoints
{
    public class NpgSqlCheckpoint : ICheckpoint
    {
        public NpgSqlCheckpoint(string currentLsn, string fileName)
        {
            CurrentLsn = currentLsn ?? throw new ArgumentNullException(nameof(currentLsn));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        public string CurrentLsn { get; }

        public string FileName { get; }

        public byte[] ToBytes()
        {
            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(CurrentLsn.Length));
            buffer.AddRange(Encoding.UTF8.GetBytes(CurrentLsn));
            buffer.AddRange(BitConverter.GetBytes(FileName.Length));
            buffer.AddRange(Encoding.UTF8.GetBytes(FileName));
            return buffer.ToArray();
        }
        public static NpgSqlCheckpoint FromBytes(byte[] bytes)
        {
            var lsnLength = BitConverter.ToInt32(bytes,0);
            var lsn = Encoding.UTF8.GetString(bytes, sizeof(int), lsnLength);
            lsnLength = BitConverter.ToInt32(bytes, sizeof(int) + lsnLength);
            var offset = lsnLength + sizeof(int) * 2;
            var fn = Encoding.UTF8.GetString(bytes, offset, bytes.Length - offset);
            return new NpgSqlCheckpoint(lsn, fn);
        }
    }
}
