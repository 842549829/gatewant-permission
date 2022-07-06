var builder = WebApplication.CreateBuilder(args);
var startup = new Startup(builder.Configuration, builder.Logging);
startup.ConfigureServices(builder.Services);
var app = builder.Build();
startup.Configure(app);