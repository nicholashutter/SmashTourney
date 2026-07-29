namespace Helpers;

using Serilog;
using Microsoft.AspNetCore.Authentication.Cookies;
using Helpers; 
using Services;
using CustomExceptions;
using Microsoft.AspNetCore.Identity;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

public class AppSetup
{
    public static void SetupLogging()
    {
        //setup logger system using serlog library
        //typical implementation for writing to file and console
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console()
            //WriteTo.File provides a path inside the logs folder filename is today's date
            .WriteTo.File($"logs/{DateOnly.FromDateTime(DateTime.Now).ToString("MMMM dd yyyy")}")
            .CreateLogger();
    }

    public static void LogServerStart()
    {
        Log.Information($"\nServer Running on Port: {AppConstants.ServerURL}. Press CTRL C to Stop...");
    }

    public static void LogServerStop()
    {
        Log.Information("\nShutting Down...");
    }

    public static async Task HandleUserSession(CookieSignedInContext context, string username)
    {
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
                    throw new UserNotFoundException("HandleSession: TILT - should not get here");
                }
            }
        }
    }

    public static async Task SeedDevelopmentUsersAsync(IServiceProvider services, IHostEnvironment environment, IConfiguration configuration)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        using var scope = services.CreateScope();
        var identityUserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var existingDemoUser = await identityUserManager.FindByNameAsync(AppConstants.DemoUserName);

        if (existingDemoUser is not null)
        {
            Log.Information("Demo user '{DemoUserName}' is available for development login.", AppConstants.DemoUserName);
        }
        else
        {
            var demoUser = new ApplicationUser
            {
                UserName = AppConstants.DemoUserName,
                Email = AppConstants.DemoUserEmail,
                EmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
            };

            var creationResult = await identityUserManager.CreateAsync(demoUser, AppConstants.DemoUserPassword);

            if (creationResult.Succeeded)
            {
                Log.Information("Demo login seeded. Username: {DemoUserName} Password: {DemoUserPassword}", AppConstants.DemoUserName, AppConstants.DemoUserPassword);
            }
            else
            {
                foreach (var error in creationResult.Errors)
                {
                    Log.Error("Failed to seed demo user. Code={ErrorCode}, Description={ErrorDescription}", error.Code, error.Description);
                }
            }
        }

        var enableDummyUsers = configuration.GetValue<bool>(AppConstants.EnableDummyUsersConfigKey);
        if (!enableDummyUsers)
        {
            int removedCount = 0;
            for (int i = 1; i <= AppConstants.DummyUserSeedCount; i++)
            {
                var suffix = i.ToString("00");
                var userName = $"{AppConstants.DummyUserNamePrefix}{suffix}";
                var existingDummyUser = await identityUserManager.FindByNameAsync(userName);
                if (existingDummyUser is null)
                {
                    continue;
                }

                var deleteResult = await identityUserManager.DeleteAsync(existingDummyUser);
                if (deleteResult.Succeeded)
                {
                    removedCount++;
                    continue;
                }

                foreach (var error in deleteResult.Errors)
                {
                    Log.Error(
                        "Failed to remove dummy user {DummyUserName}. Code={ErrorCode}, Description={ErrorDescription}",
                        userName,
                        error.Code,
                        error.Description);
                }
            }

            Log.Information(
                "Development dummy-user seed is disabled. Removed={RemovedCount}. Set '{ConfigKey}' to true to enable.",
                removedCount,
                AppConstants.EnableDummyUsersConfigKey);
            return;
        }

        int seededCount = 0;
        int existingCount = 0;

        for (int i = 1; i <= AppConstants.DummyUserSeedCount; i++)
        {
            var suffix = i.ToString("00");
            var userName = $"{AppConstants.DummyUserNamePrefix}{suffix}";
            var password = $"{AppConstants.DummyUserPasswordPrefix}{suffix}";

            var existingDummyUser = await identityUserManager.FindByNameAsync(userName);
            if (existingDummyUser is not null)
            {
                existingCount++;
                continue;
            }

            var dummyUser = new ApplicationUser
            {
                UserName = userName,
                Email = $"{userName}@smashtourney.local",
                EmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
            };

            var dummyCreationResult = await identityUserManager.CreateAsync(dummyUser, password);
            if (dummyCreationResult.Succeeded)
            {
                seededCount++;
                continue;
            }

            foreach (var error in dummyCreationResult.Errors)
            {
                Log.Error(
                    "Failed to seed dummy user {DummyUserName}. Code={ErrorCode}, Description={ErrorDescription}",
                    userName,
                    error.Code,
                    error.Description);
            }
        }

        Log.Information(
            "Development dummy-user seed complete. Seeded={SeededCount}, Existing={ExistingCount}, Pattern={UserPattern}/{PasswordPattern}",
            seededCount,
            existingCount,
            "dummy01..dummy16",
            "DummyPass!01..DummyPass!16");
    }

    // Brings the database up to the current schema, creating it if absent.
    //
    // Migrations cannot be generated here — writing one is a design-time job for
    // `dotnet ef migrations add`, which diffs the model against a snapshot and
    // emits C#. What startup can do is apply the ones already committed, which
    // is what makes "if not exists" work for a schema that keeps changing rather
    // than only for an empty folder.
    //
    // This replaces an EnsureCreated call. EnsureCreated builds today's schema
    // and then never touches it again, so the first added column would have left
    // every existing database quietly wrong.
    public static async Task ApplyDatabaseMigrationsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await BaselineExistingSchemaAsync(dbContext);

        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

        if (pendingMigrations.Count == 0)
        {
            return;
        }

        Log.Information("Applying {MigrationCount} pending database migration(s).", pendingMigrations.Count);

        await dbContext.Database.MigrateAsync();
    }

    // Adopts a database whose tables were created before migrations existed.
    //
    // A database built by the previous EnsureCreated call has the right tables
    // but no migration history, so EF believes nothing has been applied and
    // tries to create those tables again — which fails on "table already
    // exists" and takes startup down with it. Recording the migrations as
    // applied, without running them, is the standard way to adopt an existing
    // schema. It only fires when the schema is genuinely already there.
    private static async Task BaselineExistingSchemaAsync(ApplicationDbContext dbContext)
    {
        if (!await dbContext.Database.CanConnectAsync())
        {
            return;
        }

        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
        if (appliedMigrations.Any())
        {
            return;
        }

        var databaseCreator = dbContext.GetService<IRelationalDatabaseCreator>();
        if (!await databaseCreator.HasTablesAsync())
        {
            return;
        }

        var historyRepository = dbContext.GetService<IHistoryRepository>();
        await dbContext.Database.ExecuteSqlRawAsync(historyRepository.GetCreateIfNotExistsScript());

        var migrationsAssembly = dbContext.GetService<IMigrationsAssembly>();
        var productVersion = typeof(DbContext).Assembly.GetName().Version?.ToString() ?? "8.0.0";

        foreach (var migrationId in migrationsAssembly.Migrations.Keys)
        {
            var insertHistoryRow = historyRepository.GetInsertScript(
                new HistoryRow(migrationId, productVersion));

            await dbContext.Database.ExecuteSqlRawAsync(insertHistoryRow);
        }

        Log.Warning(
            "Adopted an existing database that predates migrations. Recorded {MigrationCount} migration(s) as already applied.",
            migrationsAssembly.Migrations.Count);
    }

    public static async Task ClearDevelopmentGamesForDummyProfileAsync(IServiceProvider services, IHostEnvironment environment, IConfiguration configuration)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        var enableDummyUsers = configuration.GetValue<bool>(AppConstants.EnableDummyUsersConfigKey);
        if (!enableDummyUsers)
        {
            return;
        }

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var existingGames = await dbContext.Games.ToListAsync();

        if (existingGames.Count == 0)
        {
            Log.Information("Development dummy-user profile startup: no existing games to clear.");
            return;
        }

        dbContext.Games.RemoveRange(existingGames);
        await dbContext.SaveChangesAsync();

        Log.Information("Development dummy-user profile startup: cleared {GameCount} existing games.", existingGames.Count);
    }



}