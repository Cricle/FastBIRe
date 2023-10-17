using FastBIRe.Cdc.Checkpoints;
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

        public long Position { get; }

        public string? FileName { get; }
        public override string ToString()
        {
            return $"{{FileName: {FileName}, Pos: {Position}}}";
        }
        public byte[] ToBytes()
        {
            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(Position));
            if (!string.IsNullOrEmpty(FileName))
            {
                buffer.AddRange(Encoding.UTF8.GetBytes(FileName));
            }
            return buffer.ToArray();
        }
    }
}
