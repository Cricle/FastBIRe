using FastBIRe.Timing;

namespace FastBIRe.Test.Timing
{
    [TestClass]
    public class TimeNameMapperTest
    {
        [TestMethod]
        [DataRow(TimeTypes.Second, TimeNameMapper.Second)]
        [DataRow(TimeTypes.Minute, TimeNameMapper.Minute)]
        [DataRow(TimeTypes.Hour, TimeNameMapper.Hour)]
        [DataRow(TimeTypes.Day, TimeNameMapper.Day)]
        [DataRow(TimeTypes.Week, TimeNameMapper.Week)]
        [DataRow(TimeTypes.Month, TimeNameMapper.Month)]
        [DataRow(TimeTypes.Quarter, TimeNameMapper.Quarter)]
        [DataRow(TimeTypes.Year, TimeNameMapper.Year)]
        public void ToName(TimeTypes timeType, string act)
        {
            Assert.AreEqual(act, TimeNameMapper.Instance.ToName(timeType));
        }
        [TestMethod]
        public void ToName_Unknow()
        {
            Assert.AreEqual(string.Empty, TimeNameMapper.Instance.ToName((TimeTypes)999));
        }
    }
}
