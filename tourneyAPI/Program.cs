using Routers;
using Services;


var builder = WebApplication.CreateBuilder(args);
var logFilePath = $"logs/{DateTime.UtcNow}.txt";


//Create a StreamWriter to write logs to a text file
using (StreamWriter logFileWriter = new StreamWriter(logFilePath, append: true))
{
    //Create an ILoggerFactory
    ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
        //Add console output
        builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                    });

        //Add a custom log provider to write logs to text files
        builder.AddProvider(new ApplicationLoggerProvider(logFileWriter));
    });
}
builder.Services.AddDbContext<AppDBContext>();

//this should allow both for a singleton factory registration for DB access inside of game and
//scopped access on the individual service level
builder.Services.AddDbContextFactory<AppDBContext>();
//scoped services will be destroyed after the function scope that uses them closes 
builder.Services.AddScoped<ILogger, ApplicationLogger>();
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

Console.CancelKeyPress += (sender, eventArgs) =>
{
    Console.WriteLine("\nShutting down...");
    eventArgs.Cancel = true;
    app.Lifetime.StopApplication();
};

app.Run();






