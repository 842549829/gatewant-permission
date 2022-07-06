namespace GateWayPermissionApi
{
    public class Startup
    {
        public Startup(ConfigurationManager configuration, ILoggingBuilder logging)
        {
            Configuration = configuration;
            Logging = logging;
        }

        private ConfigurationManager Configuration { get; }

        private ILoggingBuilder Logging { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // 添加配置文件
            Configuration
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("ocelot.json", true, true);

            // 添加网关
            services.AddOcelot(Configuration);

            // 添加日志记录器
            Logging
                .AddConsole()
                .AddNLog("NLog.config");

            // 跨域
            services.AddCors(options => options.AddPolicy("CorsPolicy", corsPolicyBuilder =>
            {
                corsPolicyBuilder.AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));

            //添加认证方式
            services.AddIdentityServer4(Configuration);
        }

        public void Configure(WebApplication app)
        {
            // 处理WebSockets Token
            app.Use((context, next) =>
            {
                if (context.Request.Query.TryGetValue("access_token", out var token)
                    && context.Request.Path.StartsWithSegments("/signal-hubs"))
                    context.Request.Headers.Add("Authorization", $"Bearer {token}");
                return next.Invoke();
            });

            // WebSockets请求
            app.UseWebSockets();

            //异常拦截处理
            app.UseExceptionHandler(applicationBuilder =>
            {
                applicationBuilder.Run(async context =>
                {
                    var logger = app.Logger;
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        logger.LogError(ex.Error, "网关错误");
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
                // 认证
                AuthenticationMiddleware = GatewayExtension.AuthorizationMiddleware,

                // 权限认证
                AuthorizationMiddleware = GatewayExtension.AuthorizationMiddleware
            }).Wait();

            app.Run();
        }
    }
}