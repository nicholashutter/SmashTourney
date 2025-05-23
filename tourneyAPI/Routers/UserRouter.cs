namespace Routers;

using Entities;
using Microsoft.EntityFrameworkCore;
using Services;
using System;
public static class UserRouter
{

    public static void Map(WebApplication app)
    {
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("HelloWorld");


        app.MapGet("/Users", async (AppDBContext db) =>
        {

            logger.LogInformation("Request Type: Get \n URL: '/Users' \n Time: {Timestamp}", DateTime.UtcNow);

            var Users = await db.Users.ToListAsync();

            return TypedResults.Ok(Users);
        });

        app.MapPost("/Users", async (User newUser, AppDBContext db) =>
        {
            logger.LogInformation("Request Type: Post \n URL: '/' \n Time: {Timestamp}", DateTime.UtcNow);


            db.Add(newUser);
            await db.SaveChangesAsync();

            logger.LogInformation("User Created Successfully. \n Time: {Timestamp}", DateTime.UtcNow);
            return TypedResults.Ok();
        });


    }
}
