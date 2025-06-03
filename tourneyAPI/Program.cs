using Routers;
using Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"logs\\{DateOnly.FromDateTime(DateTime.Now).ToString("MMMM dd yyyy")}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

//this should allow both for a singleton factory registration for DB access inside of game and
//scopped access on the individual service level
builder.Services.AddDbContextFactory<AppDBContext>();
builder.Services.AddDbContext<AppDBContext>();


//scoped services will be destroyed after the function scope that uses them closes 
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

//gameService is this applications "application" class
builder.Services.AddSingleton<IGameService, GameService>();

builder.Services.AddSerilog();

var app = builder.Build();

//add route handlers
UserRouter.Map(app);
PlayerRouter.Map(app);
GameRouter.Map(app);

Console.CancelKeyPress += (sender, eventArgs) =>
{
    Console.WriteLine("\nShutting down...");
    eventArgs.Cancel = true;
    app.Lifetime.StopApplication();
};

app.Run();






