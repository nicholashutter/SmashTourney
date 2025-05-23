namespace Routers;

using Entities;
using Microsoft.EntityFrameworkCore;
using Services;
using System;
public static class UserRouter
{

    public static void Map(WebApplication app)
    {
        var UserRoutes = app.MapGroup("/Users");


        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("UserRouter");


        UserRoutes.MapGet("/", async (AppDBContext db, HttpContext context) =>
        {

            logger.LogInformation("Request Type: Get \n URL: '/Users' \n Time: {Timestamp}", DateTime.UtcNow);

            try
            {
                var Users = await db.Users.ToListAsync();


                context.Response.StatusCode = StatusCodes.Status200OK;
                logger.LogInformation("Users Retrieved Successfully \n Time: {Timestamp}", DateTime.UtcNow);
                await context.Response.WriteAsJsonAsync(Users);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                logger.LogError("Unknown Error Occured: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                await context.Response.WriteAsJsonAsync(e.ToString());
            }
        });
        UserRoutes.MapPost("/", async (AppDBContext db, HttpContext context, User newUser) =>
        {
            logger.LogInformation("Request Type: Post \n URL: '/Users' \n Time: {Timestamp}", DateTime.UtcNow);

            try
            {
                if (newUser is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    logger.LogWarning("Warning: newUser is null\n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.BadRequest("Invalid JSON Payload");
                }
                else
                {
                    db.Add(newUser);
                    await db.SaveChangesAsync();
                    context.Response.StatusCode = StatusCodes.Status201Created;
                    logger.LogInformation("User Created Successfully. \n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.Created();
                }


            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                logger.LogError("Unknown Error Occured: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                return Results.BadRequest("Unknown Error Occured");
            }

        });

        UserRoutes.MapPut("/", async (AppDBContext db, HttpContext context, User updateUser) =>
        {
            logger.LogInformation("Request Type: Put \n URL: '/Users \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {

                if (updateUser is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    logger.LogWarning("Warning: updateUser is null\n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.BadRequest("Invalid JSON Payload");
                }
                else
                {
                    db.Update(updateUser);

                    await db.SaveChangesAsync();
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    logger.LogInformation("User Updated Successfully. \n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.Accepted();
                }

            }
            catch (Exception e)
            {
                logger.LogError("Unknown Error Occured: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                return Results.BadRequest("Unknown Error Occured"); ;
            }

        });

        app.MapDelete("/", async (AppDBContext db, HttpContext context, User deleteUser) =>
        {
            try
            {
                if (deleteUser is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    logger.LogWarning("Warning: deleteUser is null\n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.BadRequest("Invalid JSON Payload");
                }
                else
                {
                    db.Users.Remove(deleteUser);
                    context.Response.StatusCode = StatusCodes.Status202Accepted;
                    await db.SaveChangesAsync();
                    return Results.Accepted("User Delete Success");
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                logger.LogError("Unknown Error Occured: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
            }
        });


    }
}
