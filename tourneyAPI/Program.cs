using Routers;
using Services;
using Serilog;
using Helpers;
using Entities;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

//set urls for server
builder.WebHost.UseUrls(AppConstants.ServerURL);

//static init function for logger
AppSetup.SetupLogging();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(ApplicationDbContext.SetupProd()));

builder.Services.AddSignalR();

//scoped services will be destroyed after the function scope that uses them closes 
builder.Services.AddScoped<IPlayerManager, PlayerManager>();
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
                //assign signed in users a server side session
                await AppSetup.HandleUserSession(context, context.Principal?.Identity?.Name ?? string.Empty);
            };
    });


var app = builder.Build();

//PROD
//app.UseHttpsRedirection(); 

app.UseDefaultFiles();
app.UseStaticFiles();

//add route handlers

app.MapIdentityApi<ApplicationUser>();
//extend IdentityApi provided routes
UserRouter.Map(app);
PlayerRouter.Map(app);
GameRouter.Map(app);

app.MapHub<ConnectionHub>(AppConstants.HubURL);

Console.CancelKeyPress += (sender, eventArgs) =>
{
    AppSetup.LogServerStop();
    eventArgs.Cancel = true;
    app.Lifetime.StopApplication();
};

AppSetup.LogServerStart();
app.Run();


public partial class Program { }



