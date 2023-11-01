using System.Data;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public class PrimitiveRecordToObject<T> : IRecordToObject<T>
    {
        private static readonly Type type = typeof(T);

        public static readonly PrimitiveRecordToObject<T> Instance = new PrimitiveRecordToObject<T>();

        private PrimitiveRecordToObject() { }

        public T? To(IDataRecord record)
        {
            T? result;
            if (record.IsDBNull(0))
            {
                result = default;
            }
            else if (TypeVisibility<T>.IsBool)
            {
                var dt = record.GetBoolean(0);
                result = Unsafe.As<bool, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsByte)
            {
                var dt = record.GetByte(0);
                result = Unsafe.As<byte, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsSByte)
            {
                var dt = (sbyte)record.GetByte(0);
                result = Unsafe.As<sbyte, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsChar)
            {
                var dt = (char)record.GetInt16(0);
                result = Unsafe.As<char, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsShort)
            {
                var dt = record.GetInt16(0);
                result = Unsafe.As<short, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsUShort)
            {
                var dt = (ushort)record.GetInt16(0);
                result = Unsafe.As<ushort, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsInt)
            {
                var dt = record.GetInt32(0);
                result = Unsafe.As<int, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsUInt)
            {
                var dt = (uint)record.GetInt32(0);
                result = Unsafe.As<uint, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsLong)
            {
                var dt = record.GetInt64(0);
                result = Unsafe.As<long, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsULong)
            {
                var dt = (ulong)record.GetInt64(0);
                result = Unsafe.As<ulong, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsFloat)
            {
                var dt = record.GetFloat(0);
                result = Unsafe.As<float, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsDouble)
            {
                var dt = record.GetDouble(0);
                result = Unsafe.As<double, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsDecimal)
            {
                var dt = record.GetDecimal(0);
                result = Unsafe.As<decimal, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsDateTime)
            {
                var dt = record.GetDateTime(0);
                result = (T)(object)dt;
            }
            else if (TypeVisibility<T>.IsString)
            {
                var dt = record.GetString(0);
                result = Unsafe.As<string, T>(ref dt);
            }
            else if (TypeVisibility<T>.IsGuid)
            {
                var dt = record.GetGuid(0);
                result = Unsafe.As<Guid, T>(ref dt);
            }
            else
            {
                var res = record.GetValue(0);
                result = (T)Convert.ChangeType(res, type);
            }
            return result;
        }

        public IList<T?> ToList(IDataReader reader)
        {
            var res = new List<T?>();
            while (reader.Read())
            {
                res.Add(To(reader));
            }
            return res;
        }
    }
}
