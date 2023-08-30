using Ao.Stock.Mirror;
using Dapper;
using FastBIRe.Project.DynamicTable;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FastBIRe.Project.WebSample.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SchoolController : ControllerBase
    {
        private readonly ProjectSession projectSession;

        public SchoolController(ProjectSession projectSession)
        {
            this.projectSession = projectSession;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] DefaultDynamicTable<DefaultDynamicColumn> table)
        {
            var res = await projectSession.DynamicOperator.UpsetAsync(projectSession.Context, projectSession.Project, table);
            return Ok(res);
        }
        [HttpGet]
        public IActionResult FindClass(string className)
        {
            return Ok(projectSession.Result.Project?.Tables.FirstOrDefault(x => x.Name == className));
        }
        [HttpGet]
        public async Task<IActionResult> DeleteClass(string className)
        {
            var res=await projectSession.DynamicOperator.DropAsync(projectSession.Context,projectSession.Project, className);
            return Ok(res);
        }
        [HttpGet]
        public IActionResult AllClass()
        {
            return Ok(projectSession.Result.Project?.Tables);
        }
        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] WriteDataRequest request)
        {
            var fun = projectSession.FunctionMapper;
            var @class = projectSession.Project.Tables.FirstOrDefault(x => x.Name == request.ClassName);
            if (@class == null)
            {
                return BadRequest();
            }
            var map = projectSession.DynamicOperator.CaseValues(projectSession.Project,request.ClassName,request.Values);
            var sql = $"INSERT INTO `{request.ClassName}`(_time,{string.Join(",", map.Select(x=>x.Key))}) VALUES ({fun.Now()},{string.Join(",", map.Select(x => x.Value))})";
            var datas = await projectSession.Connection.ExecuteAsync(sql);
            return Ok(datas);
        }
        [HttpGet]
        public async Task<IActionResult> AllStudent(string className)
        {
            var datas = await projectSession.Connection.QueryAsync($"SELECT * FROM `{className}`");
            return Ok(datas);
        }
    }
    public class WriteDataRequest
    {
        public string? ClassName { get; set; }

        public Dictionary<string, object>? Values { get; set; }
    }
}