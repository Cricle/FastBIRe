namespace FastBIRe.Farm
{
    public readonly record struct CursorRow
    {
        public CursorRow(string name, ulong point)
        {
            Name = name;
            Point = point;
        }

        public string Name { get; }

        public ulong Point { get; }
    }
}
