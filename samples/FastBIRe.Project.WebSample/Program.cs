using FastBIRe.Project.Accesstor;
using Microsoft.OpenApi.Models;

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
                var pjId=httpContext.HttpContext!.Request.Headers["projId"].ToString();
                var accesstor = ser.GetRequiredService<ProjectDbServices>();
                var res= accesstor.CreateDbContextAsync(pjId).GetAwaiter().GetResult()!;
                if (res.Project!=null)
                {
                    httpContext.HttpContext.Features.Set(res.Project);
                }
                return res.DbContext!;
            });
            builder.Services.AddSingleton<ProjectDbServices>();
            builder.Services.AddSingleton<IProjectAccesstor<IProjectAccesstContext<string>, string>>(p =>
            {
                return new JsonDirectoryProjectAccesstor("projects", "proj");
            });
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
}