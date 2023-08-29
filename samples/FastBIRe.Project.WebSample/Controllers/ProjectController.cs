using FastBIRe.Project.Accesstor;
using FastBIRe.Project.Models;
using Microsoft.AspNetCore.Mvc;

namespace FastBIRe.Project.WebSample.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectAccesstor<IProjectAccesstContext<string>, string> projectAccesstor;

        public ProjectController(IProjectAccesstor<IProjectAccesstContext<string>, string> projectAccesstor)
        {
            this.projectAccesstor = projectAccesstor;
        }
        [HttpGet]
        public async Task<IActionResult> AllProject()
        {
            return Ok(await projectAccesstor.AllProjectsAsync(null));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProject([FromForm] string id)
        {
            return Ok(await projectAccesstor.DeleteProjectAsync(new ProjectAccesstContext<string>(id)));
        }
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromForm] string name)
        {
            var id = Guid.NewGuid().ToString("N");
            var ok = await projectAccesstor.CreateProjectAsync(new ProjectAccesstContext<string>(id),
                new Project<string>(id, name, new Version(1, 0), DateTime.Now));
            return Ok(id);
        }
    }
}