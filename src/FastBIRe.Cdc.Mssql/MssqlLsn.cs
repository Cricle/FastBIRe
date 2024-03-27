using System.Numerics;

namespace FastBIRe.Cdc.Mssql
{
    public readonly struct MssqlLsn : IEquatable<MssqlLsn>
    {
        public static readonly MssqlLsn Empty = new MssqlLsn(new byte[10]);

        public static MssqlLsn Create(byte[]? data)
        {
            if (data == null)
            {
                return Empty;
            }
            return new MssqlLsn(data);
        }

        public MssqlLsn(byte[] lsn)
        {
            Lsn = lsn ?? throw new ArgumentNullException(nameof(lsn));
            if (lsn.Length != 10)
            {
                throw new ArgumentException("The lsn size must 10");
            }
        }

        public byte[] Lsn { get; }

        public BigInteger LsnBigInteger => LsnHelper.LsnToBitInteger(Lsn);

        public string LsnString => LsnHelper.LsnToString(Lsn);

        public override bool Equals(object? obj)
        {
            if (obj is MssqlLsn lsn)
            {
                return Equals(lsn);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hs = new HashCode();
            for (int i = 0; i < Lsn.Length; i++)
            {
                hs.Add(Lsn[i]);
            }
            return hs.ToHashCode();
        }
        public override string ToString()
        {
            return LsnString;
        }

        public bool Equals(MssqlLsn other)
        {
            return other.Lsn.SequenceEqual(Lsn);
        }

        public static bool operator ==(MssqlLsn left, MssqlLsn right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(MssqlLsn left, MssqlLsn right)
        {
            return !left.Equals(right);
        }
        public static bool operator >(MssqlLsn left, MssqlLsn right)
        {
            return left.LsnBigInteger > right.LsnBigInteger;
        }
        public static bool operator <(MssqlLsn left, MssqlLsn right)
        {
            return left.LsnBigInteger < right.LsnBigInteger;
        }
        public static bool operator >=(MssqlLsn left, MssqlLsn right)
        {
            return left.LsnBigInteger >= right.LsnBigInteger;
        }
        public static bool operator <=(MssqlLsn left, MssqlLsn right)
        {
            return left.LsnBigInteger <= right.LsnBigInteger;
        }
        public static implicit operator byte[](MssqlLsn left)
        {
            return left.Lsn;
        }
        public static implicit operator BigInteger(MssqlLsn left)
        {
            return left.LsnBigInteger;
        }
        public static implicit operator string(MssqlLsn left)
        {
            return left.LsnString;
        }
    }
}
