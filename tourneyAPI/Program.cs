using Routers;
using Services;
using Serilog;
using Helpers;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Json.Serialization;

// Determines whether a development CORS origin is allowed.
static bool IsDevelopmentOriginAllowed(string? origin)
{
    if (string.IsNullOrWhiteSpace(origin))
    {
        return false;
    }

    Uri parsedOriginUri;
    try
    {
        parsedOriginUri = new Uri(origin, UriKind.Absolute);
    }
    catch (UriFormatException)
    {
        return false;
    }

    return parsedOriginUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || parsedOriginUri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
}


var builder = WebApplication.CreateBuilder(args);

// Configures the server URL bindings.
builder.WebHost.UseUrls(AppConstants.ServerURL);

// Configures structured application logging.
AppSetup.SetupLogging();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(ApplicationDbContext.SetupProd()));

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Registers scoped application services.
builder.Services.AddScoped<IPlayerManager, PlayerManager>();
builder.Services.AddScoped<IUserManager, UserManager>();

// Registers the game orchestrator as a singleton service.
builder.Services.AddSingleton<IGameService, GameService>();

// Connects ASP.NET logging to Serilog.
builder.Services.AddSerilog();

// Enables authorization for authenticated endpoints.
builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Uses a disabled sender while email infrastructure is not enabled.
builder.Services.AddTransient<IEmailSender, DisabledEmailSender>();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>, DisabledEmailSender>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configures CORS policies for development and production environments.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy
                    .SetIsOriginAllowed(IsDevelopmentOriginAllowed)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
            else
            {
                policy.WithOrigins(AppConstants.ClientUrl)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
        });
});

builder.Services.ConfigureApplicationCookie(options =>
    {
        // options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        // options.SlidingExpiration = true;
        // options.Cookie.HttpOnly = true;
        // options.Cookie.SameSite = SameSiteMode.Strict;
        options.Events.OnSignedIn = async context =>
            {
                // Assigns a server-side session record for the signed-in user.
                await AppSetup.HandleUserSession(context, context.Principal?.Identity?.Name ?? string.Empty);
            };
    });


var app = builder.Build();

await AppSetup.ClearDevelopmentGamesForDummyProfileAsync(app.Services, app.Environment, app.Configuration);
await AppSetup.SeedDevelopmentUsersAsync(app.Services, app.Environment, app.Configuration);

// Configures the HTTP request pipeline.
//app.UseHttpsRedirection(); 
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ConnectionHub>(AppConstants.HubURL);
app.UseDefaultFiles();
app.UseStaticFiles();


// Maps identity and feature routes.

app.MapIdentityApi<ApplicationUser>();
// Extends identity API routes with domain-specific endpoints.
UserRouter.Map(app);
PlayerRouter.Map(app);


// Handles graceful shutdown for Ctrl+C.
Console.CancelKeyPress += (sender, eventArgs) =>
{
    AppSetup.LogServerStop();
    eventArgs.Cancel = true;
    app.Lifetime.StopApplication();
};

AppSetup.LogServerStart();
app.Run();


public partial class Program { }



