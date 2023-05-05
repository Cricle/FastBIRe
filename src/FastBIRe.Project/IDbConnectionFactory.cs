using System.Data.Common;

namespace FastBIRe.Project
{
    public interface IDbConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
