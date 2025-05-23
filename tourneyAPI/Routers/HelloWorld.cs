namespace Routers;

using Entities;
using Services;
using System;
public static class HelloWorld
{

    public static void Map(WebApplication app)
    {
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("HelloWorld");


        app.MapGet("/", async context =>
        {
            logger.LogInformation("Request Type: Get \n URL: '/' \n Time: {Timestamp}", DateTime.UtcNow);

            await context.Response.WriteAsJsonAsync(new { Message = "tourneyAPI running" });
        });

        app.MapPost("/", async context =>
        {
            logger.LogInformation("Request Type: Post \n URL: '/' \n Time: {Timestamp}", DateTime.UtcNow);

            using var db = new AppDBContext();

            db.Add(new User
            {
                Username = "User ${DateTime.UtcNow}",
                Email = "User${DateTime.UtcNow}@email.com",
                AllTimeMatches = 0,
                AllTimeWins = 0,
                AllTimeLosses = 0,
                RegistrationDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            logger.LogInformation("User Created Successfully. \n Time: {Timestamp}", DateTime.UtcNow);
            await context.Response.WriteAsJsonAsync(new { Message = "Successful User Creation" });
        });
    }
}
