namespace Diagnostics.Traces.Mini
{
    public enum MiniReadResultTypes
    {
        Succeed=0,
        CanNotReadHeader=1,
        CanNotReadBody=2,
        HashError=3
    }
}
