namespace FastBIRe.Cdc.MongoDB
{
    public class MongoVariables : DbVariables
    {
        public const string MemberStateKey = "memgbers.stateStr";
        public const string MemberIdKey = "memgbers._id";
        public const string ConfigsvrKey = "configsvr";

        public string? MemberState => GetOrDefault(MemberStateKey);

        public string? MemberId=> GetOrDefault(MemberIdKey);
    }
}
