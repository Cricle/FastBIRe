using Microsoft.EntityFrameworkCore;

namespace FastBIRe.Project.WebSample
{
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext(DbContextOptions options) : base(options)
        {
        }

        protected SchoolDbContext()
        {
        }

        public DbSet<Student> Students => Set<Student>();

        public DbSet<Class> Classs => Set<Class>();
    }
}