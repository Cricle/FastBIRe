namespace FastBIRe.Project.Models
{
    public record Project<TId>: IProject<TId>
    {
        public TId? Id { get; set; }

        public string? Name { get; set; }

        public Version? Version { get; set; }

        public DateTime CreateTime { get; set; }

        private Dictionary<string, string>? features;

        public Project()
        {
        }

        public Project(TId id, string name, Version version, DateTime createTime)
        {
            Id = id;
            Name = name;
            Version = version;
            CreateTime = createTime;
        }

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
