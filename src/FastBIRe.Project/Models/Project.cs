namespace FastBIRe.Project.Models
{
    public record Project<TId>(TId Id, string Name, Version Version, DateTime CreateTime) : IProject<TId>
    {
        private Dictionary<string, string>? features;

        public Dictionary<string, string> Features
        {
            get
            {
                if (features == null)
                {
                    features = new Dictionary<string, string>();
                }
                return features;
            }
            set
            {
                features = value;
            }
        }
    }
}
