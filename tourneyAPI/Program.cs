using Routers;
using Serilog;
using Services;

var loggerConfig = new LoggerConfiguration()
.MinimumLevel.Information()
.Enrich.FromLogContext()
.WriteTo.Console()
.CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();


builder.Logging.ClearProviders();
builder.Logging.AddSerilog(loggerConfig);

builder.Services.AddDbContext<AppDBContext>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddSingleton<IUserService, UserService>(); 


var app = builder.Build();

UserRouter.Map(app);
PlayerRouter.Map(app);
GameRouter.Map(app);

app.Run();
