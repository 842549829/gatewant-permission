//#region .net5 写法
//using GateWayPermissionApi.Extensions;
//using Microsoft.AspNetCore.Diagnostics;
//using NLog.Extensions.Logging;
//using Ocelot.DependencyInjection;
//using Ocelot.Middleware;
//using System.Text;

//namespace GateWayPermissionApi
//{
//    public class Startup
//    {
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        private IConfiguration Configuration { get; }

//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.AddIdentityServer4(Configuration);

//            services.AddOcelot();

//            services.AddLogging(builder => { builder.AddNLog("NLog.config"); });

//            // 跨域
//            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
//            {
//                builder.AllowAnyMethod()
//                    .SetIsOriginAllowed(_ => true)
//                    .AllowAnyHeader()
//                    .AllowCredentials();
//            }));
//        }

//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
//        {
//            // 处理WebSockets Token
//            app.Use((context, next) =>
//            {
//                if (context.Request.Query.TryGetValue("access_token", out var token)
//                    && context.Request.Path.StartsWithSegments("/signalr-hubs"))
//                    context.Request.Headers.Add("Authorization", $"Bearer {token}");
//                return next.Invoke();
//            });


//            //异常处理
//            app.UseExceptionHandler(builder =>
//            {
//                builder.Run(async context =>
//                {
//                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//                    context.Response.ContentType = "application/json";
//                    var ex = context.Features.Get<IExceptionHandlerFeature>();
//                    if (ex != null)
//                    {
//                        logger?.LogError(ex.Error, "网关错误");
//                    }
//                    await context.Response.WriteAsync("错误", encoding: Encoding.UTF8);
//                });
//            });

//            // 鉴权
//            app.UseAuthentication();

//            // 跨域
//            app.UseCors("CorsPolicy");

//            // WebSockets请求
//            app.UseWebSockets();

//            // http请求
//            app.UseHttpsRedirection()
//                .UseOcelot().Wait();
//        }
//    }
//}

//#endregion