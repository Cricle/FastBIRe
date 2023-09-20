using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace FastBIRe
{
    public static class MD5Helper
    {
        private static readonly MD5 instance = MD5.Create();

        public unsafe static string ComputeHash(string str)
        {
            var strLen = str.Length;
            var cs = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(str.AsSpan()));
            var byteCount = Encoding.UTF8.GetByteCount(cs, strLen);
            var bytes = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                var bytesReceived = Encoding.UTF8.GetBytes(cs, strLen, (byte*)Unsafe.AsPointer(ref bytes[0]), byteCount);
                Debug.Assert(bytesReceived == byteCount);
                return Convert.ToBase64String(instance.ComputeHash(bytes, 0, bytesReceived));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }
    }
}
