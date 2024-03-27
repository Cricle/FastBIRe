using System.Data;

namespace FastBIRe.Data
{
    public class DataCapturerGroup : List<IDataCapturer>, IDataCapturer
    {
        public void Capture(IDataReader reader)
        {
            foreach (var item in this)
            {
                item.Capture(reader);
            }
        }
    }

}
