using DatabaseSchemaReader.DataSchema;
using FastBIRe.Project.Accesstor;
using Microsoft.Data.Sqlite;
using Microsoft.OpenApi.Models;
using System.Data.Common;

namespace FastBIRe.Project.WebSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {
                opt.AddSecurityDefinition("projId", new OpenApiSecurityScheme
                {
                    Name = "projId",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "projId"
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference =new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id ="projId"
                        }
                    },Array.Empty<string>()
                }});
            });
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped(ser =>
            {
                var httpContext = ser.GetRequiredService<IHttpContextAccessor>();
                var pjId = httpContext.HttpContext!.Request.Headers["projId"].ToString();
                var accesstor = ser.GetRequiredService<ProjectDbServices>();
                var ctx = new ProjectAccesstContext<string>(pjId);
                var res = accesstor.CreateDbContextAsync(ctx).GetAwaiter().GetResult();
                if (res?.Connection == null)
                {
                    throw new InvalidOperationException("Project id not found");
                }
                return new ProjectSession(ctx, res, accesstor,
                    ser.GetRequiredService<IStringToDbConnectionFactory>(),
                    ser.GetRequiredService<IProjectAccesstor<IProjectAccesstContext<string>, SchoolProject, string>>());
            });
            builder.Services.AddDataSchema(s => $"{s.Id}_data", s => s.Id);
            builder.Services.AddSingleton(p =>
            {
                return new ProjectDbServices(
                    p.GetRequiredService<IProjectAccesstor<IProjectAccesstContext<string>, SchoolProject, string>>(),
                    p.GetRequiredService<IDataSchema<IProjectAccesstContext<string>>>(),
                    p.GetRequiredService<IStringToDbConnectionFactory>(),
                    string.Empty);
            });
            builder.Services.AddStringToDbConnectionFactory(
                 SqlType.SQLite,
                s => new SqliteConnection(s),
                (s, db) => new SqliteConnection($"Data source=projects/{db}{(string.IsNullOrWhiteSpace(s) ? string.Empty : "," + s)}"));
            builder.Services.AddJsonDirectoryProjectAccesstor<SchoolProject, string>("projects", "pj");
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.MapControllers();

            app.Run();
        }
    }
    public class ProjectSession : IDisposable
    {
        public ProjectSession(IProjectAccesstContext<string> context,
            ProjectCreateWithDbContextResult<SchoolProject, string> result,
            ProjectDbServices projectDbServices,
            IStringToDbConnectionFactory stringToDbConnectionFactory,
            IProjectAccesstor<IProjectAccesstContext<string>, SchoolProject, string> projectAccesstor)
        {
            Context = context;
            Result = result;
            ProjectDbServices = projectDbServices;
            StringToDbConnectionFactory = stringToDbConnectionFactory;
            ProjectAccesstor = projectAccesstor;
        }

        public IProjectAccesstContext<string> Context { get; }

        public ProjectCreateWithDbContextResult<SchoolProject, string> Result { get; }

        public ProjectDbServices ProjectDbServices { get; }

        public IStringToDbConnectionFactory StringToDbConnectionFactory { get; }

        public SqlType SqlType => StringToDbConnectionFactory.SqlType;

        public DbConnection Connection => Result.Connection;

        public FunctionMapper FunctionMapper => new FunctionMapper(SqlType);

        public SchoolProject? Project => Result.Project;

        public IProjectAccesstor<IProjectAccesstContext<string>, SchoolProject, string> ProjectAccesstor { get; }

        public SchoolDynamicOperator DynamicOperator => new SchoolDynamicOperator(ProjectDbServices.CreateTableFactory(Result, TableIniter.Instance), ProjectAccesstor);

        public void Dispose()
        {
            Result.Dispose();
        }
    }
}