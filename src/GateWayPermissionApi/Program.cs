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

// ��������ļ�
configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile("ocelot.json", true, true);

// �������
services.AddOcelot(configuration);

// �����־��¼��
logging
    .AddConsole()
    .AddNLog("NLog.config");

// ����
services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
{
    builder.AllowAnyMethod()
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowCredentials();
}));

//�����֤��ʽ
services.AddIdentityServer4(configuration);

var app = builder.Build();

//�쳣���ش���
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
            logger?.LogError(ex.Error, "���ش���");
        }
        await context.Response.WriteAsync("����", encoding: Encoding.UTF8);
    });
});

// ����
app.UseCors("CorsPolicy");

// �����֤
app.UseAuthentication();

// ����Ӧ��
app.UseOcelot(new OcelotPipelineConfiguration()
{
    // Ȩ����֤
    AuthorizationMiddleware = GatewayExtension.AuthorizationMiddleware
}).Wait();

app.Run();

#region .net5д��
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