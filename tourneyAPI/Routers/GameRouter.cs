namespace Routers;

using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services;
using System;
using Serilog; 

public static class GameRouter
{
    public static void Map(WebApplication app)
    {
        var GameRoutes = app.MapGroup("/Games");

        GameRoutes.MapGet("/", async (HttpContext context, AppDBContext db) =>
        {
            Log.Information("Request Type: Get \n URL: '/Games' \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                var Games = await db.Games.ToListAsync();

                context.Response.StatusCode = StatusCodes.Status200OK;
                Log.Information("Games Retrieved Successfully \n Time: {Timestamp}", DateTime.UtcNow);
                await context.Response.WriteAsJsonAsync(Games);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                Log.Error("Unknown Error Occurred: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                await context.Response.WriteAsJsonAsync(e.ToString());
            }
        });

        GameRoutes.MapPost("/", async (HttpContext context, AppDBContext db) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games' \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                var newGame = await context.Request.ReadFromJsonAsync<Game>();

                if (newGame is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    Log.Warning("Warning: newGame is null\n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.BadRequest("Invalid JSON Payload");
                }
                else
                {
                    db.Add(newGame);
                    await db.SaveChangesAsync();
                    context.Response.StatusCode = StatusCodes.Status201Created;
                    Log.Information("Game Created Successfully. \n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.Created();
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                Log.Error("Unknown Error Occurred: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                return Results.BadRequest("Unknown Error Occurred");
            }
        });


        GameRoutes.MapPut("/", async (HttpContext context, AppDBContext db) =>
        {
            Log.Information("Request Type: Put \n URL: '/Games' \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                var updateGame = await context.Request.ReadFromJsonAsync<Game>(); // Assuming you have a 'Game' model

                if (updateGame is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    Log.Warning("Warning: updateGame is null\n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.BadRequest("Invalid JSON Payload");
                }
                else
                {
                    db.Update(updateGame);
                    await db.SaveChangesAsync();
                    context.Response.StatusCode = StatusCodes.Status202Accepted;
                    Log.Information("Game Updated Successfully. \n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.Accepted();
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                Log.Error("Unknown Error Occurred: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                return Results.BadRequest("Unknown Error Occurred");
            }
        });


        GameRoutes.MapDelete("/", async (HttpContext context, AppDBContext db) =>
        {
            Log.Information("Request Type: Delete \n URL: '/Games' \n Time:{Timestamp}", DateTime.UtcNow);
            try
            {
                var deleteGame = await context.Request.ReadFromJsonAsync<Game>();

                if (deleteGame is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    Log.Warning("Warning: deleteGame is null\n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.BadRequest("Invalid JSON Payload");
                }
                else
                {
                    db.Games.Remove(deleteGame);
                    await db.SaveChangesAsync();
                    context.Response.StatusCode = StatusCodes.Status202Accepted;
                    Log.Information("Game Deleted Successfully. \n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.Accepted("Game Delete Success");
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                Log.Error("Unknown Error Occurred: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                return Results.BadRequest("Unknown Error Occurred");
            }
        });
    }
}