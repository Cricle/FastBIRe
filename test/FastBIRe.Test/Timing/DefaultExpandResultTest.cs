using FastBIRe.Timing;

namespace FastBIRe.Test.Timing
{
    [TestClass]
    public class DefaultExpandResultTest
    {
        [TestMethod]
        public void New()
        {
            var originName = "origin";
            var name = "name";
            var expFormatter = "formatter";
            var result = new DefaultExpandResult(originName, name, expFormatter);
            Assert.AreEqual(originName, result.OriginName);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(expFormatter, result.ExparessionFormatter);
        }
        [TestMethod]
        public void FormatExpression()
        {
            var result = new DefaultExpandResult("", "", "a{0}b");
            var res = result.FormatExpression("11");
            Assert.AreEqual("a11b", res);
        }
        [TestMethod]
        public void FormatExpression_EmptyExp()
        {
            var result = new DefaultExpandResult("", "", null);
            var res = result.FormatExpression("11");
            Assert.AreEqual(string.Empty, res);
        }
        [TestMethod]
        public void Expression_New()
        {
            var result = DefaultExpandResult.Expression("a",null);
            Assert.AreEqual("a", result.OriginName);
            Assert.AreEqual("a", result.Name);
        }
    }
}
