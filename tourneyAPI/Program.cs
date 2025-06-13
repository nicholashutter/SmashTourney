using Routers;
using Services;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


//setup logger system using serlog library
//typical implementation for writing to file and console
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    //WriteTo.File provides a path inside the logs folder filename is today's date
    .WriteTo.File($"logs/{DateOnly.FromDateTime(DateTime.Now).ToString("MMMM dd yyyy")}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

var dbPath = ApplicationDbContext.SetupDb();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(dbPath);

});


//scoped services will be destroyed after the function scope that uses them closes 
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

//gameService is this applications "application" class
builder.Services.AddSingleton<IGameService, GameService>();

//hand logging pipeline over from asp.net core webapp to serilog
builder.Services.AddSerilog();

builder.Services.AddAuthorization();


builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
.AddEntityFrameworkStores<ApplicationDbContext>();

/* TODO implement IEmailSender 
builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
});

builder.Services.AddTransient<IEmailSender, EmailSender>(); */

/* 
PROD
var cookiePolicyOptions = new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
}; 
*/

var app = builder.Build();

//PROD
//app.UseHttpsRedirection(); 

app.UseDefaultFiles();
app.UseStaticFiles();

//UserRouter may be removed
//UserRouter.Map(app);

//add route handlers

//test dev only get request hello world route handler

app.MapIdentityApi<ApplicationUser>();
PlayerRouter.Map(app);
GameRouter.Map(app);

Console.CancelKeyPress += (sender, eventArgs) =>
{
    Console.WriteLine("\nShutting down...");
    eventArgs.Cancel = true;
    app.Lifetime.StopApplication();
};

app.Run();






