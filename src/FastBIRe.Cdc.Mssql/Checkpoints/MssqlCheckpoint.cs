using FastBIRe.Cdc.Checkpoints;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FastBIRe.Cdc.Mssql.Checkpoints
{
    public class MssqlCheckpoint : ICheckpoint
    {
        public MssqlCheckpoint(byte[] lsn)
        {
            Lsn = lsn;
        }

        public byte[] Lsn { get; }

        public BigInteger LsnInteger => LsnHelper.LsnToBitInteger(Lsn);

        public string Hex => LsnInteger.ToString("X");

        public byte[] ToBytes()
        {
            return Lsn;
        }
    }
}
