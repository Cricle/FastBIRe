using FastBIRe.Project.Accesstor;
using FastBIRe.Project.Models;

namespace FastBIRe.Project.Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "projects");
            var asset = new JsonDirectoryProjectAccesstor(path,"proj");
            asset.CreateProjectAsync(new ProjectAccesstContext<string>("hello"), new Project<string>("hello", "hello", Version.Parse("1.0.0"), DateTime.Now))
                .GetAwaiter().GetResult();
            Console.WriteLine(asset.ProjectExistsAsync(new ProjectAccesstContext<string>("hello")).GetAwaiter().GetResult());
            Console.WriteLine(asset.GetProjectAsync(new ProjectAccesstContext<string>("hello")).Result.Id);
        }
    }
}