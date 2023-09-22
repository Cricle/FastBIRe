namespace FastBIRe.Timing
{
    public enum TimeTypes
    {
        None = 0,
        Second = 1,
        Minute = Second << 1,
        Hour = Second << 2,
        Day = Second << 3,
        Week = Second << 4,
        Month = Second << 5,
        Quarter = Second << 6,
        Year = Second << 7,
    }
}
