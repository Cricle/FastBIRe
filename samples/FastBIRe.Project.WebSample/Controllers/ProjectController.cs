using FastBIRe.Project.Accesstor;
using FastBIRe.Project.DynamicTable;
using Microsoft.AspNetCore.Mvc;

namespace FastBIRe.Project.WebSample.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectDbServices dbServices;

        public ProjectController(ProjectDbServices dbServices)
        {
            this.dbServices = dbServices;
        }
        [HttpGet]
        public async Task<IActionResult> AllProject()
        {
            return Ok(await dbServices.ProjectAccesstor.AllProjectsAsync(null));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProject([FromForm] string id)
        {
            return Ok(await dbServices.ProjectAccesstor.DeleteProjectAsync(new ProjectAccesstContext<string>(id)));
        }
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromForm] string name)
        {
            var id = Guid.NewGuid().ToString("N");
            var ok = await dbServices.ProjectAccesstor.CreateProjectAsync(new ProjectAccesstContext<string>(id),
                new SchoolProject(id, name, new Version(1, 0), DateTime.Now, new List<DefaultDynamicTable<DefaultDynamicColumn>>(0)));
            return Ok(id);
        }
    }
}