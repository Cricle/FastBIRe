using FastBIRe.Cdc.Checkpoints;
using NpgsqlTypes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FastBIRe.Cdc.NpgSql.Checkpoints
{
    public class PgSqlCheckpoint : ICheckpoint
    {
        private static readonly int NpgsqlLogSequenceNumberSize = Marshal.SizeOf<NpgsqlLogSequenceNumber>();

        public PgSqlCheckpoint(NpgsqlLogSequenceNumber? sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }

        public NpgsqlLogSequenceNumber? SequenceNumber { get; }

        public bool IsEmpty => SequenceNumber == null;

        public override string? ToString()
        {
            return SequenceNumber?.ToString();
        }
        public unsafe byte[] ToBytes()
        {
            if (SequenceNumber != null)
            {
                var data = new byte[NpgsqlLogSequenceNumberSize];
                Unsafe.Write(Unsafe.AsPointer(ref MemoryMarshal.GetReference(data.AsSpan())), SequenceNumber.Value);
            }
            return Array.Empty<byte>();
        }
        public static unsafe PgSqlCheckpoint FromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return new PgSqlCheckpoint(null);
            }
            var number = Unsafe.Read<NpgsqlLogSequenceNumber>(Unsafe.AsPointer(ref bytes[0]));
            return new PgSqlCheckpoint(number);
        }
    }
}
