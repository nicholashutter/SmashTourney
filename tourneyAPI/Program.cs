using Routers;
using Services;
using Serilog;
using Helpers;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

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

builder.Services.AddSignalR();

// Registers scoped application services.
builder.Services.AddScoped<IPlayerManager, PlayerManager>();
builder.Services.AddScoped<IUserManager, UserManager>();

// Registers the game orchestrator as a singleton service.
builder.Services.AddSingleton<IGameService, GameService>();

// Retires finished and abandoned games on a timer rather than from a GET.
builder.Services.AddHostedService<StaleGameSweeper>();

// Connects ASP.NET logging to Serilog.
builder.Services.AddSerilog();

// Enables authorization for authenticated endpoints.
builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>(identityOptions =>
{
    // An address has to be proved before it can sign in.
    //
    // Registration is open to anybody on the internet, so without this an
    // account is just a claim: somebody can sign up as another person's address,
    // and nothing ever tests whether the mailbox is theirs. It is only
    // enforceable now that confirmation email actually gets sent.
    identityOptions.SignIn.RequireConfirmedEmail = true;

    // Two accounts on one address would make "reset my password" ambiguous.
    identityOptions.User.RequireUniqueEmail = true;

    // Lockout is the only thing standing between a public login form and an
    // unlimited guessing rate. The window is short enough that a real person who
    // mistyped is not locked out of their own party for long.
    identityOptions.Lockout.MaxFailedAccessAttempts = 5;
    identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    identityOptions.Lockout.AllowedForNewUsers = true;

    // Length carries far more weight than character-class rules, which mostly
    // teach people to end passwords with "1!". Twelve characters, and the
    // classes left in place because removing them would weaken existing accounts
    // nobody is going to re-enrol.
    identityOptions.Password.RequiredLength = 12;
    identityOptions.Password.RequireDigit = true;
    identityOptions.Password.RequireLowercase = true;
    identityOptions.Password.RequireUppercase = true;
    identityOptions.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));

// Delivery is chosen from configuration, not from the environment name.
//
// A developer with a relay configured should get real mail, and a production
// deploy that forgot to configure one must not look like it is sending. The
// fallback logs the link and says plainly that nothing was posted, which is the
// distinction the previous no-op sender erased.
var emailOptions = builder.Configuration
    .GetSection(EmailOptions.SectionName)
    .Get<EmailOptions>() ?? new EmailOptions();

if (emailOptions.IsDeliveryConfigured())
{
    builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
    builder.Services.AddTransient<Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>, SmtpEmailSender>();
}
else
{
    builder.Services.AddTransient<IEmailSender, LoggedEmailSender>();
    builder.Services.AddTransient<Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>, LoggedEmailSender>();
}

// Hardens the authentication cookie.
builder.Services.ConfigureApplicationCookie(cookieOptions =>
{
    cookieOptions.Cookie.HttpOnly = true;

    // Secure always, except where the transport is plain HTTP by design: a
    // developer on http://localhost, and the in-memory test host. Forcing Secure
    // in those cases means the client discards the cookie on arrival and nothing
    // can sign in — which looks exactly like broken authentication rather than a
    // cookie policy.
    // Any *Testing environment counts, so a test host that needs its own settings
    // file does not silently lose its cookies by picking a different name.
    var transportMayBePlainHttp = builder.Environment.IsDevelopment()
        || builder.Environment.EnvironmentName.EndsWith("Testing", StringComparison.OrdinalIgnoreCase);

    cookieOptions.Cookie.SecurePolicy = transportMayBePlainHttp
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;

    // Lax rather than Strict: the confirmation link arrives from a mail client,
    // and Strict would drop the cookie on that first cross-site navigation.
    // In production the API serves the built client from wwwroot, so this is a
    // same-site cookie anyway.
    cookieOptions.Cookie.SameSite = SameSiteMode.Lax;
    cookieOptions.Cookie.Name = "SmashTourney.Auth";

    cookieOptions.ExpireTimeSpan = TimeSpan.FromDays(7);
    cookieOptions.SlidingExpiration = true;

    // An API answers with a status code. Without this, an expired session
    // redirects to a login page that does not exist here and the client sees a
    // 404 where it expected a 401.
    cookieOptions.Events.OnRedirectToLogin = redirectContext =>
    {
        redirectContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    cookieOptions.Events.OnRedirectToAccessDenied = redirectContext =>
    {
        redirectContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };

    cookieOptions.Events.OnSignedIn = async signedInContext =>
    {
        // Assigns a server-side session record for the signed-in user.
        await AppSetup.HandleUserSession(signedInContext, signedInContext.Principal?.Identity?.Name ?? string.Empty);
    };
});

// Rate limits the endpoints where guessing pays.
//
// Lockout protects one account at a time; it does nothing about somebody trying
// one common password across thousands of addresses, or hammering registration
// to enumerate which are taken. This caps the whole surface per caller.
// The numbers come from configuration because a policy can only be registered
// once — a second registration under the same name throws rather than replacing
// it — so this is the only way a test host or a deployment can dial it without
// forking the pipeline.
var authRateLimitPermits = builder.Configuration.GetValue<int?>("Auth:RateLimit:PermitLimit") ?? 10;
var authRateLimitWindowSeconds = builder.Configuration.GetValue<int?>("Auth:RateLimit:WindowSeconds") ?? 60;

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Partitioned by caller address, not global.
    //
    // A single shared bucket would mean one person flooding sign-in exhausts the
    // allowance for everybody, so an attacker could lock the whole room out of
    // their own tournament without ever guessing a password. Per-caller buckets
    // make the limit cost only the caller who tripped it.
    rateLimiterOptions.AddPolicy(AppConstants.AuthRateLimiterPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(authRateLimitWindowSeconds),
                PermitLimit = authRateLimitPermits,

                // Reject rather than queue. Queueing an excess sign-in holds the
                // connection open until the window rolls over — up to a minute —
                // so a flood would exhaust connections instead of being turned
                // away, and somebody merely retyping their password would sit
                // watching a spinner. An immediate 429 is cheaper and more honest.
                QueueLimit = 0
            }));
});

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

var app = builder.Build();

// Schema first: every startup step below this line queries tables.
await AppSetup.ApplyDatabaseMigrationsAsync(app.Services);
await AppSetup.ClearDevelopmentGamesForDummyProfileAsync(app.Services, app.Environment, app.Configuration);
await AppSetup.SeedDevelopmentUsersAsync(app.Services, app.Environment, app.Configuration);

// Configures the HTTP request pipeline.

// HTTPS is enforced everywhere except a developer machine.
//
// This was commented out, which meant the authentication cookie travelled in
// cleartext — anyone on the same network could read it and become that user.
// It stays off in Development because the server binds a plain http:// URL
// there and redirecting would send the client to a port nothing is listening on.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors();

// Ahead of authentication, so that rejecting a flood costs a counter increment
// rather than a password hash.
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
// The hub carries player lists and game-start events, so it is game data and is
// protected like everything else. The browser client already negotiates with
// withCredentials, so the identity cookie rides along with the connection.
app.MapHub<ConnectionHub>(AppConstants.HubURL).RequireAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();


// Maps identity and feature routes.

// The whole identity surface is rate limited: /register, /login,
// /resendConfirmationEmail and /forgotPassword are the endpoints where guessing
// or mail-flooding pays, and they all arrive from here.
app.MapIdentityApi<ApplicationUser>()
    .RequireRateLimiting(AppConstants.AuthRateLimiterPolicy);

// Extends identity API routes with domain-specific endpoints.
UserRouter.Map(app);
PlayerRouter.Map(app);
GameRouter.Map(app);


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



