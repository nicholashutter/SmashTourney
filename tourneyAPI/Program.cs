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

//this should allow both for a singleton factory registration for DB access inside of game and
//scopped access on the individual service level
builder.Services.AddDbContextFactory<AppDBContext>();
builder.Services.AddDbContext<AppDBContext>();


//scoped services will be destroyed after the route handler that uses them closes 
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

//gameService is this applications "application" class
builder.Services.AddSingleton<IGameService, GameService>();

var app = builder.Build();

//add route handlers
UserRouter.Map(app);
PlayerRouter.Map(app);
GameRouter.Map(app);

app.Run();
