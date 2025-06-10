namespace Routers;

using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services;
using System;
using Serilog;

/*

This class was written prior to integrating my User model into efcore as a IdentityUser
This class may be removed at some point

*/
public static class UserRouter
{

    public static void Map(WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AppLog");

        var UserRoutes = app.MapGroup("/Users");


        UserRoutes.MapGet("/", async (HttpContext context, ApplicationDbContext db) =>
        {

            Log.Information("Request Type: Get \n URL: '/Users' \n Time: {Timestamp}", DateTime.UtcNow);

            try
            {
                var Users = await db.Users.ToListAsync();


                context.Response.StatusCode = StatusCodes.Status200OK;
                Log.Information("Users Retrieved Successfully \n Time: {Timestamp}", DateTime.UtcNow);
                await context.Response.WriteAsJsonAsync(Users);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                Log.Error("Unknown Error Occured: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                await context.Response.WriteAsJsonAsync(e.ToString());
            }
        });
        UserRoutes.MapPost("/", async (HttpContext context, ApplicationDbContext db) =>
        {
            Log.Information("Request Type: Post \n URL: '/Users' \n Time: {Timestamp}", DateTime.UtcNow);

            try
            {
                var newUser = await context.Request.ReadFromJsonAsync<ApplicationUser>();

                if (newUser is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    Log.Warning("Warning: newUser is null\n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.BadRequest("Invalid JSON Payload");
                }
                else
                {
                    db.Add(newUser);
                    await db.SaveChangesAsync();
                    context.Response.StatusCode = StatusCodes.Status201Created;
                    Log.Information("User Created Successfully. \n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.Created();
                }


            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                Log.Error("Unknown Error Occured: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                return Results.BadRequest("Unknown Error Occured");
            }

        });

        UserRoutes.MapPut("/", async (HttpContext context, ApplicationDbContext db) =>
        {
            Log.Information("Request Type: Put \n URL: '/Users \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                var updateUser = await context.Request.ReadFromJsonAsync<ApplicationUser>();

                if (updateUser is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    Log.Warning("Warning: updateUser is null\n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.BadRequest("Invalid JSON Payload");
                }
                else
                {
                    db.Update(updateUser);

                    await db.SaveChangesAsync();
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    Log.Information("User Updated Successfully. \n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.Accepted();
                }

            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                Log.Error("Unknown Error Occured: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                return Results.BadRequest("Unknown Error Occured");
            }

        });

        app.MapDelete("/", async (HttpContext context, ApplicationDbContext db) =>
        {
            Log.Information("Request Type: Put \n URL: '/Users \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                var deleteUser = await context.Request.ReadFromJsonAsync<ApplicationUser>();

                if (deleteUser is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    Log.Warning("Warning: deleteUser is null\n Time: {Timestamp}", DateTime.UtcNow);
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
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                Log.Error("Unknown Error Occured: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                return Results.BadRequest("Unknown Error Occured");
            }
        });


    }
}
