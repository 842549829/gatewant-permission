namespace GateWayPermissionApi.Extensions
{
    public static class GatewayExtension
    {
        public static IServiceCollection AddIdentityServer4(this IServiceCollection services, IConfiguration configuration)
        {
            //配置consul注册地址
            services.Configure<IdentityServerOptions>(configuration.GetSection("IdentityServer"));
            var config = configuration.GetSection("IdentityServer").Get<IdentityServerOptions>();
            var identityBuilder = services.AddAuthentication(config.IdentityScheme);
            foreach (var resource in config.Resources)
            {
                identityBuilder.AddIdentityServerAuthentication(resource.Key, options =>
                {
                    options.Authority = $"http://{config.IP}:{config.Port}";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = resource.Name;
                    options.SupportedTokens = SupportedTokens.Both;
                });
            }
            return services;
        }

        public static async Task AuthorizationMiddleware(HttpContext httpContext, Func<Task> next)
        {
            var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("OcelotConfiguration");
            var downstreamRoute = httpContext.Items.DownstreamRoute();
            if (httpContext.Request.Method.ToUpper() != "OPTIONS" && downstreamRoute.IsAuthenticated && !string.IsNullOrWhiteSpace(downstreamRoute.AuthenticationOptions.AuthenticationProviderKey))
            {
                logger.LogInformation($"{httpContext.Request.Path} is an authenticated route. AuthorizationMiddleware checking if client is authenticated");
                var result = await httpContext.AuthenticateAsync(downstreamRoute.AuthenticationOptions.AuthenticationProviderKey);
                if (result.Principal != null)
                {
                    httpContext.User = result.Principal;
                }
                if (httpContext.User.Identity is
                    {
                        IsAuthenticated: true
                    })
                {
                    //TODO 权限拦截处理
                    if (httpContext.Request.Path == "/api")
                    {
                        var error = new UnauthorizedError(
                       $"Request for authorized route {httpContext.Request.Path} by {httpContext.User.Identity.Name} was unauthorized");
                        httpContext.Items.SetError(error);
                    }
                    else
                    {
                        logger.LogInformation($"Client has been authenticated for {httpContext.Request.Path}");
                        await next.Invoke();
                    }
                }
                else
                {
                    var error = new UnauthenticatedError($"Request for authenticated route {httpContext.Request.Path} by {httpContext.User.Identity?.Name} was unauthenticated");
                    logger.LogWarning($"Client has NOT been authenticated for {httpContext.Request.Path} and pipeline error set. {error}");
                    httpContext.Items.SetError(error);
                }
            }
            else
            {
                logger.LogInformation($"No authentication needed for {httpContext.Request.Path}");

                await next.Invoke();
            }
        }
    }
}