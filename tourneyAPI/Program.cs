using Routers;
using Serilog;
using Services;

var builder = WebApplication.CreateBuilder(args);

var loggerConfig = new LoggerConfiguration()
.ReadFrom.Configuration(builder.Configuration)
.Enrich.FromLogContext()
.CreateLogger();




builder.Logging.ClearProviders();
builder.Logging.AddSerilog(loggerConfig);

builder.Services.AddDbContext<AppDBContext>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Application Logger");

UserRouter.Map(app, logger);
PlayerRouter.Map(app, logger);


app.Run();
