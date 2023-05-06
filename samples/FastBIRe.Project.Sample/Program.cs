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
}