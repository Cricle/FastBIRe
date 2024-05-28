using System.Data;

namespace FastBIRe.Data
{
    public class FieldDataCapture : IDataCapturer
    {
        public FieldDataCapture(string fieldName, Action<object> captured)
        {
            FieldName = fieldName;
            Captured = captured;
        }

        public string FieldName { get; }

        public Action<object> Captured { get; }

        public bool NotFoundSkip { get; set; }

        public void Capture(IDataReader reader)
        {
            var index = reader.GetOrdinal(FieldName);
            if (index == -1 && !NotFoundSkip)
            {
                throw new ArgumentException($"The field {FieldName} not found!");
            }
            Captured(reader[index]);
        }
    }

}
