namespace FastBIRe.Mig
{
    public class VTable: VObject
    {
        public string Name { get; set; }

        public List<VColumn> Columns { get; set; }
    }
}