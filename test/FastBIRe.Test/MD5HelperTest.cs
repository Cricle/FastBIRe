using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FastBIRe.Test
{
    [TestClass]
    public class MD5HelperTest
    {
        [TestMethod]
        public void MD5Compute()
        {
            var str = "abcmd5test";
            using (var md5 = MD5.Create())
            {
                var exp = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(str))); ;
                var act = MD5Helper.ComputeHash(str);
                Assert.AreEqual(exp, act);
            }
        }
    }
}
