using FastBIRe.Project.Accesstor;
using FastBIRe.Project.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastBIRe.Project.Sample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "projects");
            var asset = new JsonDirectoryProjectAccesstor(path,"proj");
            await asset.CreateProjectAsync(new ProjectAccesstContext<string>("hello"), new Project<string>("hello", "hello", Version.Parse("1.0.0"), DateTime.Now));
            var ser = new ProjectDbServices(asset, new DbContextOptionsBuilder().Options);
            var ctx = await ser.CreateDbContextAsync("hello");
            var sser=new SchoolService(ctx);
            await sser.AddClass(new Class {  Name="hello" });
        }
    }
    public class SchoolService
    {
        public SchoolService(SchoolDbContext context)
        {
            Context = context;
        }

        public SchoolDbContext Context { get; }

        public Task AddClass(Class @class)
        {
            Context.Classs.Add(@class);
            return Context.SaveChangesAsync();
        }
        public Task<List<Class>> AllClass()
        {
            return Context.Classs.AsNoTracking().ToListAsync();
        }
        public Task AddStudents(IEnumerable<Student> students)
        {
            Context.Students.AddRange(students);
            return Context.SaveChangesAsync();
        }
        public Task<List<Student>> AllStudent(int classId)
        {
            return Context.Students.AsNoTracking().Where(x => x.ClassId == classId).ToListAsync();
        }
    }
    public class ProjectDbServices
    {
        public ProjectDbServices(IProjectAccesstor<IProjectAccesstContext<string>, string> projectAccesstor, DbContextOptions baseOption)
        {
            ProjectAccesstor = projectAccesstor;
            BaseOption = baseOption;

            projectFirst = new ConcurrentDictionary<string, bool>();
        }
        private readonly ConcurrentDictionary<string,bool> projectFirst;

        public IProjectAccesstor<IProjectAccesstContext<string>, string> ProjectAccesstor { get; }

        public DbContextOptions BaseOption { get; }

        public async Task<SchoolDbContext?> CreateDbContextAsync(string id,CancellationToken token=default)
        {
            var project = await ProjectAccesstor.GetProjectAsync(new ProjectAccesstContext<string>(id), token);
            if (project!=null)
            {
                var builder = new DbContextOptionsBuilder(BaseOption);
                builder.UseSqlite($"Data source={project.Id}");
                var ctx = new SchoolDbContext(builder.Options);
                if (projectFirst.TryAdd(id,false))
                {
                    await ctx.Database.EnsureCreatedAsync();
                    await ctx.Database.MigrateAsync();
                }
                return ctx;
            }
            return null;
        }
    }
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
    public class Student
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey(nameof(Class))]
        public virtual int ClassId { get; set; }

        public virtual Class Class { get; set; }
    }

    public class Class
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Student> Students { get; set; }
    }
}