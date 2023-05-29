using System.Security.Cryptography;
using System.Text;

namespace FastBIRe
{
    public static class MD5Helper
    {
        private static readonly MD5 instance = MD5.Create();

        public static string ComputeHash(string input)
        {
            var buffer = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(instance.ComputeHash(buffer));
        }
    }
}
