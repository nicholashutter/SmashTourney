using Routers;
using Services;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Entities;


//setup logger system using serlog library
//typical implementation for writing to file and console
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    //WriteTo.File provides a path inside the logs folder filename is today's date
    .WriteTo.File($"logs\\{DateOnly.FromDateTime(DateTime.Now).ToString("MMMM dd yyyy")}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

//this should allow both for a singleton factory registration for DB access inside of game and
//scopped access on the individual service level
builder.Services.AddDbContextFactory<ApplicationDbContext>();
builder.Services.AddDbContext<ApplicationDbContext>();


//scoped services will be destroyed after the function scope that uses them closes 
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

//gameService is this applications "application" class
builder.Services.AddSingleton<IGameService, GameService>();

//hand logging pipeline over from asp.net core webapp to serilog
builder.Services.AddSerilog();

builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
.AddEntityFrameworkStores<ApplicationDbContext>();

/* 
PROD
This option will tell the browser not to allow cros origin requests
This will heavily restrict the api's ability to accept requests
from anywhere that isn't the host domain
var cookiePolicyOptions = new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
}; 
*/

var app = builder.Build();


//PROD
//app.UseHttpsRedirection(); 

//this will let us host our front end solution
//app.UseStaticFiles(); 






//UserRouter may be removed
//UserRouter.Map(app);

//add route handlers
app.MapIdentityApi<IdentityUser>();
PlayerRouter.Map(app);
GameRouter.Map(app);

Console.CancelKeyPress += (sender, eventArgs) =>
{
    Console.WriteLine("\nShutting down...");
    eventArgs.Cancel = true;
    app.Lifetime.StopApplication();
};

app.Run();






