using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastBIRe.Project.WebSample.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SchoolController : ControllerBase
    {
        private readonly SchoolDbContext dbContext;

        public SchoolController(SchoolDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass(string name)
        {
            dbContext.Classs.Add(new Class
            {
                Name= name
            });
            return Ok(await dbContext.SaveChangesAsync());
        }
        [HttpGet]
        public async Task<IActionResult> AllClass()
        {
            return Ok(await dbContext.Classs.AsNoTracking().ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> CreateStudent(int classId,string name)
        {
            dbContext.Students.Add(new Student
            {
                ClassId= classId,
                Name = name
            });
            return Ok(await dbContext.SaveChangesAsync());
        }
        [HttpGet]
        public async Task<IActionResult> AllStudent()
        {
            return Ok(await dbContext.Students.AsNoTracking().ToListAsync());
        }
    }
}