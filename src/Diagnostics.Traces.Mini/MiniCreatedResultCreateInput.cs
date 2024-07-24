namespace Diagnostics.Traces.Mini
{
    public readonly struct MiniCreatedResultCreateInput
    {
        public MiniCreatedResultCreateInput(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
