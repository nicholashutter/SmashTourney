namespace ApiTests;

using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Hosting;
// The non-generic IEmailSender lives in Identity.UI.Services and the generic
// IEmailSender<TUser> in Identity. Both are needed, because the application
// registers its sender under both.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Entities;
using Helpers;
using Services;
using System.Data.Common;

// Creates a test web host that uses an isolated in-memory database for API integration tests.
public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    // Captures the email the application sends, so tests can read the links a
    // real user would have received rather than minting tokens behind the
    // server's back.
    public RecordingEmailSender SentEmail { get; } = new();

    // Configures test-only service overrides for database isolation.
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replaces whichever sender the application chose. Registered as the
            // same instance for both interfaces so a test reads one mailbox, not
            // two, whichever interface the calling code happens to depend on.
            RemoveAll(services, typeof(IEmailSender));
            RemoveAll(services, typeof(IEmailSender<ApplicationUser>));

            services.AddSingleton<IEmailSender>(SentEmail);
            services.AddSingleton<IEmailSender<ApplicationUser>>(SentEmail);

            // Remove the existing ApplicationDbContext registration only when the running tests
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbConnection));

            if (dbConnectionDescriptor != null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            // Create open SqliteConnection so EF won't automatically close it.
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                return connection;
            });

            services.AddDbContext<ApplicationDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });

        });

        builder.UseEnvironment(EnvironmentName);
    }

    // Which appsettings.{Environment}.json this host runs with.
    //
    // The rate limit has to come from such a file rather than from a
    // ConfigureAppConfiguration callback here: Program.cs reads it while building
    // the host, which happens before this factory's callbacks run, so anything
    // injected that way arrives too late to be seen. Overriding the environment
    // name is how a derived factory selects different settings.
    protected virtual string EnvironmentName => "Testing";

    // Removes every registration for one service type.
    //
    // SingleOrDefault is not safe here: the application registers a sender for two
    // interfaces and could register more, and a second registration would silently
    // win over the recorder.
    private static void RemoveAll(IServiceCollection services, Type serviceType)
    {
        var existingDescriptors = services
            .Where(descriptor => descriptor.ServiceType == serviceType)
            .ToList();

        foreach (var descriptor in existingDescriptors)
        {
            services.Remove(descriptor);
        }
    }
}