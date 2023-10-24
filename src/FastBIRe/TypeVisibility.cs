namespace FastBIRe
{
    internal static class TypeVisibility<T>
    {
        static TypeVisibility()
        {
            IsBool = typeof(T) == typeof(bool) || typeof(T) == typeof(bool?);
            IsByte = typeof(T) == typeof(byte) || typeof(T) == typeof(byte?);
            IsSByte = typeof(T) == typeof(sbyte) || typeof(T) == typeof(sbyte?);
            IsShort = typeof(T) == typeof(short) || typeof(T) == typeof(short?);
            IsUShort = typeof(T) == typeof(ushort) || typeof(T) == typeof(ushort?);
            IsChar = typeof(T) == typeof(char) || typeof(T) == typeof(char?);
            IsShort = typeof(T) == typeof(short) || typeof(T) == typeof(short?);
            IsInt = typeof(T) == typeof(int) || typeof(T) == typeof(int?);
            IsUInt = typeof(T) == typeof(uint) || typeof(T) == typeof(uint?);
            IsLong = typeof(T) == typeof(long) || typeof(T) == typeof(long?);
            IsULong = typeof(T) == typeof(ulong) || typeof(T) == typeof(ulong?);
            IsFloat = typeof(T) == typeof(float) || typeof(T) == typeof(float?);
            IsDouble = typeof(T) == typeof(double) || typeof(T) == typeof(double?);
            IsGuid = typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?);
            IsDateTime = typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?);
            IsDecimal = typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?);
            IsString = typeof(T) == typeof(string);
        }

        public static readonly bool IsBool;
        public static readonly bool IsByte;
        public static readonly bool IsSByte;
        public static readonly bool IsChar;
        public static readonly bool IsShort;
        public static readonly bool IsUShort;
        public static readonly bool IsInt;
        public static readonly bool IsUInt;
        public static readonly bool IsULong;
        public static readonly bool IsLong;
        public static readonly bool IsDouble;
        public static readonly bool IsFloat;
        public static readonly bool IsString;
        public static readonly bool IsGuid;
        public static readonly bool IsDateTime;
        public static readonly bool IsDecimal;
    }
}
