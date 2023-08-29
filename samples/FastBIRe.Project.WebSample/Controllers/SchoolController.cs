using Dapper;
using FastBIRe.Project.Accesstor;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FastBIRe.Project.WebSample.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SchoolController : ControllerBase
    {
        private readonly ProjectSession  projectSession;

        public SchoolController(ProjectSession projectSession)
        {
            this.projectSession = projectSession;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass(string className)
        {
            using (var mig = await projectSession.ProjectDbServices.CreateTableFactoryAsync(projectSession.Context))
            {
                mig.Service.Logger = s => Console.WriteLine(s);
                var builder = mig!.Service.GetColumnBuilder();
                var colsMerge = mig.TableIniter.WithColumns(builder,new TableColumnDefine[]
                 {
                    builder.Column("Name",builder.Type(DbType.AnsiStringFixedLength,64)),
                    builder.Column("Age",builder.Type(DbType.Int16))
                 });
                var result = await mig.MigrateToSqlAsync(className, colsMerge, null);
                await result.ExecuteAsync();
                return Ok(className);
            }
        }
        [HttpGet]
        public async Task<IActionResult> AllClass()
        {
            using (var mig = await projectSession.ProjectDbServices.CreateTableFactoryAsync(projectSession.Context))
            {
                var tables = mig.Service.Reader.TableList();
                return Ok(tables);
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateStudent(string className,string name,int arg)
        {
            var fun = projectSession.FunctionMapper;
            var datas = await projectSession.Connection.ExecuteAsync($"INSERT INTO `{className}`(_time,Name,Age) VALUES ({fun.Now()},'{name}',{arg})");
            return Ok(datas);
        }
        [HttpGet]
        public async Task<IActionResult> AllStudent(string className)
        {
            var datas =await projectSession.Connection.QueryAsync($"SELECT * FROM `{className}`");
            return Ok(datas);
        }
    }
}