using Ao.Stock.Mirror;
using Dapper;
using FastBIRe.Project.Accesstor;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text.Json;

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
        public async Task<IActionResult> CreateClass([FromBody] ClassTable table)
        {
            using (var mig = await projectSession.ProjectDbServices.CreateTableFactoryAsync(projectSession.Context))
            {
                mig.Service.Logger = s => Console.WriteLine(s);
                var builder = mig!.Service.GetColumnBuilder();
                var defines = new List<TableColumnDefine>();
                foreach (var item in table.Columns)
                {
                    switch (item.Type)
                    {
                        case FastDataType.Text:
                            defines.Add(builder.Column(item.Name, builder.Type(DbType.String, 64),destNullable:item.Nullable, id: item.Id));
                            break;
                        case FastDataType.Number:
                            defines.Add(builder.Column(item.Name, builder.Type(DbType.Decimal,builder.Scale,builder.Precision), destNullable: item.Nullable, id: item.Id));
                            break;
                        case FastDataType.DateTime:
                            defines.Add(builder.Column(item.Name, builder.Type(DbType.DateTime), destNullable: item.Nullable, id: item.Id));
                            break;
                        default:
                            break;
                    }
                }
                var colsMerge = mig.TableIniter.WithColumns(builder, defines);
                var result = await mig.MigrateToSqlAsync(table.Name, colsMerge, null);
                await result.ExecuteAsync();
                var @class = projectSession.Result.Project.Classes.FirstOrDefault(x => x.Name == table.Name);
                if (@class==null)
                {
                    projectSession.Result.Project.Classes.Add(table);
                }
                else
                {
                    @class.Columns.Clear();
                    @class.Columns.AddRange(table.Columns);
                }
                var res = await projectSession.ProjectAccesstor.UpdateProjectAsync(projectSession.Context, projectSession.Result.Project);
                return Ok(res);
            }
        }
        [HttpGet]
        public IActionResult FindClass(string className)
        {
            return Ok(projectSession.Result.Project?.Classes.FirstOrDefault(x=>x.Name==className));
        }
        [HttpGet]
        public async Task<IActionResult> DeleteClass(string className)
        {
            var @class = projectSession.Result.Project.Classes.FirstOrDefault(x => x.Name == className);
            if (@class!=null)
            {
                var sql = $"Drop table `{className}`";
                await projectSession.Result.Connection.ExecuteNonQueryAsync(sql);
                projectSession.Result.Project.Classes.Remove(@class);
                var tb = await projectSession.ProjectAccesstor.UpdateProjectAsync(projectSession.Context,projectSession.Result.Project);
                return Ok(true);
            }
            return Ok(false);
        }
        [HttpGet]
        public IActionResult AllClass()
        {
            return Ok(projectSession.Result.Project?.Classes);
        }
        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] WriteDataRequest request)
        {
            var fun = projectSession.FunctionMapper;
            var @class = projectSession.Project.Classes.FirstOrDefault(x => x.Name == request.ClassName);
            if (@class==null)
            {
                return BadRequest();
            }
            var cols = new List<string>();
            var args = new List<string>();
            if (request.Values != null)
            {
                foreach (var item in request.Values)
                {
                    var col = @class.Columns.FirstOrDefault(x => x.Name == item.Key);
                    if (col != null)
                    {
                        cols.Add(item.Key);
                        switch (col.Type)
                        {
                            case FastDataType.DateTime:
                            case FastDataType.Text:
                                args.Add($"'{item.Value}'");
                                break;
                            case FastDataType.Number:
                                args.Add(item.Value.ToString());
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            var sql = $"INSERT INTO `{request.ClassName}`(_time,{string.Join(",", cols)}) VALUES ({fun.Now()},{string.Join(",",args)})";
            var datas = await projectSession.Connection.ExecuteAsync(sql);
            return Ok(datas);
        }
        [HttpGet]
        public async Task<IActionResult> AllStudent(string className)
        {
            var datas =await projectSession.Connection.QueryAsync($"SELECT * FROM `{className}`");
            return Ok(datas);
        }
    }
    public class WriteDataRequest
    {
        public string? ClassName { get; set; }

        public Dictionary<string,object>? Values { get; set; }
    }
}