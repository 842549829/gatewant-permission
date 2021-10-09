using GateWayPermissionApi;
using GateWayPermissionApi.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog.Extensions.Logging;
using Ocelot.Authorization;
using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;
var logging = builder.Logging;

// 添加配置文件
configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile("ocelot.json", true, true);

// 添加网关
services.AddOcelot(configuration);

// 添加日志记录器
logging
    .AddConsole()
    .AddNLog("NLog.config");

// 跨域
services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
{
    builder.AllowAnyMethod()
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowCredentials();
}));

//添加认证方式
services.AddIdentityServer4(configuration);

var app = builder.Build();

//异常拦截处理
app.UseExceptionHandler(builder =>
{
    builder.Run(async context =>
    {
        var logger = app.Logger;
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        var ex = context.Features.Get<IExceptionHandlerFeature>();
        if (ex != null)
        {
            logger?.LogError(ex.Error, "网关错误");
        }
        await context.Response.WriteAsync("错误", encoding: Encoding.UTF8);
    });
});

// 跨域
app.UseCors("CorsPolicy");

// 身份认证
app.UseAuthentication();

// 网关应用
app.UseOcelot(new OcelotPipelineConfiguration()
{
    // 权限认证
    AuthorizationMiddleware = GatewayExtension.AuthorizationMiddleware
}).Wait();

app.Run();

#region .net5写法
//public class Program
//{
//    public static void Main(string[] args)
//    {
//        CreateHostBuilder(args).Build().Run();
//    }

//    public static IHostBuilder CreateHostBuilder(string[] args) =>
//        Host.CreateDefaultBuilder(args)
//         .ConfigureAppConfiguration((hostingContext, builder) =>
//         {
//             builder.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
//         })
//        .ConfigureWebHostDefaults(webBuilder =>
//        {
//            webBuilder.UseStartup<Startup>();
//        });
//} 
#endregion