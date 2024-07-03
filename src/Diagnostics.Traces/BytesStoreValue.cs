using System.Text;

namespace Diagnostics.Traces
{
    public readonly struct BytesStoreValue
    {
        public BytesStoreValue(string str)
            : this(DateTime.Now, str)
        {
        }
        public BytesStoreValue(DateTime time, string str)
        {
            Time = time;
            Value = Encoding.UTF8.GetBytes(str);
            Offset = 0;
            Length = Value.Length;
        }
        public BytesStoreValue(byte[] value)
            : this(DateTime.Now, value, 0, value.Length)
        {
        }
        public BytesStoreValue(DateTime time, byte[] value)
            : this(time, value, 0, value.Length)
        {
        }
        public BytesStoreValue(byte[] value, int offset, int length)
            : this(DateTime.Now, value, offset, length)
        {
        }
        public BytesStoreValue(DateTime time, byte[] value, int offset, int length)
        {
            Time = time;
            Value = value;
            Offset = offset;
            Length = length;
        }

        public DateTime Time { get; }

        public byte[] Value { get; }

        public int Offset { get; }

        public int Length { get; }

        public override string ToString()
        {
            return $"{{{Time.ToString("o")}}} {Encoding.UTF8.GetString(Value, Offset, Length)}}}";
        }

        public static implicit operator BytesStoreValue(string str)
        {
            return new BytesStoreValue(str);
        }
        public static implicit operator BytesStoreValue(byte[] value)
        {
            return new BytesStoreValue(value);
        }
    }
}
