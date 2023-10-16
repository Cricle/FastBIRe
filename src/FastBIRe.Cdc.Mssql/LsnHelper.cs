using System.Numerics;

namespace FastBIRe.Cdc.Mssql
{
    public static class LsnHelper
    {
        public static string LsnToString(byte[] value)
        {
            return $"0x{BitConverter.ToString(value).Replace("-",string.Empty)}";
        }
        public static BigInteger LsnToBitInteger(byte[] value)
        {
            var cp=new byte[value.Length];
            Array.Copy(value, cp, value.Length);
            Array.Reverse(cp);
            return new BigInteger(cp);
        }
    }
}
