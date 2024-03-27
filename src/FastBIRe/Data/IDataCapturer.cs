using System.Data;

namespace FastBIRe.Data
{
    public interface IDataCapturer
    {
        void Capture(IDataReader reader);
    }

}
