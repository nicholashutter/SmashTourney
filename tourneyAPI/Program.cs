using Routers;
using Services;
using Serilog;
using Helpers;
using Microsoft.AspNetCore.Identity;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

//static init function for logger
SetupLogging.Setup();

//static init function for AppDbContext
var dbPath = ApplicationDbContext.Setup();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(dbPath);

});


//scoped services will be destroyed after the function scope that uses them closes 
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IUserManager, UserManager>();

//gameService is this applications "application" singleton
builder.Services.AddSingleton<IGameService, GameService>();

//hand logging pipeline over from asp.net core webapp to serilog
builder.Services.AddSerilog();

//username and pw based auth using ASP net core identity
builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
.AddEntityFrameworkStores<ApplicationDbContext>();


/* TODO implement IEmailSender 
builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
});

builder.Services.AddTransient<IEmailSender, EmailSender>(); */


builder.Services.ConfigureApplicationCookie(options =>
    {
        // options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
       // options.SlidingExpiration = true;
       // options.Cookie.HttpOnly = true;
       // options.Cookie.SameSite = SameSiteMode.Strict;
        options.Events.OnSignedIn = async context =>
            {
                var username = context.Principal.Identity?.Name;

                if (username is not null)
                {
                    using (var scope = context.HttpContext.RequestServices.CreateAsyncScope())
                    {
                        var scopedServices = scope.ServiceProvider;

                        var _gameService = scopedServices.GetRequiredService<IGameService>();
                        var _userRepository = scopedServices.GetRequiredService<IUserManager>();

                        var foundUser = await _userRepository.GetUserByUserNameAsync(username);

                        if (foundUser is not null)
                        {
                            _gameService.CreateUserSession(foundUser);
                        }
                        else
                        {
                            Log.Warning("User has entered application for the first time. No session created.");
                        }
                    }
                }
            };
    });


var app = builder.Build();

//PROD
//app.UseHttpsRedirection(); 

app.UseDefaultFiles();
app.UseStaticFiles();




//add route handlers

//test dev only get request hello world route handler

app.MapIdentityApi<ApplicationUser>();
//extend IdentityApi provided routes
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

public partial class Program { }



