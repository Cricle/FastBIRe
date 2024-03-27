namespace FastBIRe.Timing
{
    public interface ITimeExpandHelper
    {
        IEnumerable<TimeExpandResult> Create(string name, TimeTypes type);
    }
}