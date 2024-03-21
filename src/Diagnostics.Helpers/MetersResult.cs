#pragma warning disable CA1416
namespace Tracker
{
    public readonly struct MetersResult
    {
        public MetersResult(MetersIdentity identity, string result)
        {
            Identity = identity;
            Result = result;
        }

        public MetersIdentity Identity { get; }

        public string Result { get; }
    }
}
