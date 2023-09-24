namespace FastBIRe.Timing
{
    public interface ITimeExpandHelper
    {
        IEnumerable<string> Create(string name, TimeTypes type);
    }
}