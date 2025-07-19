namespace Routers;

using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services;
using System;
using Serilog;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class PlayerRouter
{
    public static void Map(WebApplication app)
    {

        var PlayerRoutes = app.MapGroup("/Players");

        PlayerRoutes.MapGet("/", async (HttpContext context, ApplicationDbContext db) =>
        {
            Log.Information("Request Type: Get \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                var Players = await db.Players.ToListAsync();


                context.Response.StatusCode = StatusCodes.Status200OK;
                Log.Information("Players Retrieved Successfully \n Time: {Timestamp}", DateTime.UtcNow);
                await context.Response.WriteAsJsonAsync(Players);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                Log.Error("Unknown Error Occured: {e} Time: {Timestamp}", e.ToString(), DateTime.UtcNow);
                await context.Response.WriteAsJsonAsync(e.ToString());
            }
        });

        PlayerRoutes.MapPost("/", async (HttpContext context, ApplicationDbContext db) =>
        {
            Log.Information("Request Type: Post \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                var newPlayer = await context.Request.ReadFromJsonAsync<Player>();

                if (newPlayer is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    Log.Warning("Warning: newPlayer is null\n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.BadRequest("Invalid JSON Payload");
                }
                else
                {
                    db.Add(newPlayer);
                    await db.SaveChangesAsync();
                    context.Response.StatusCode = StatusCodes.Status201Created;
                    Log.Information("Player Created Successfully. \n Time: {Timestamp}", DateTime.UtcNow);
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

        PlayerRoutes.MapPut("/", async (HttpContext context, ApplicationDbContext db) =>
        {
            Log.Information("Request Type: Put \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                var updatePlayer = await context.Request.ReadFromJsonAsync<Player>();

                if (updatePlayer is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    Log.Warning("Warning: updatePlayer is null\n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.BadRequest("Invalid JSON Payload");
                }
                else
                {
                    db.Add(updatePlayer);
                    await db.SaveChangesAsync();
                    context.Response.StatusCode = StatusCodes.Status202Accepted;
                    Log.Information("Player Updated Successfully. \n Time: {Timestamp}", DateTime.UtcNow);
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

        PlayerRoutes.MapDelete("/", async (HttpContext context, ApplicationDbContext db) =>
        {
            Log.Information("Request Type: Delete \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);
            try
            {
                var deletePlayer = await context.Request.ReadFromJsonAsync<Player>();

                if (deletePlayer is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    Log.Warning("Warning: deletePlayer is null\n Time: {Timestamp}", DateTime.UtcNow);
                    return Results.BadRequest("Invalid JSON Payload");
                }
                else
                {
                    db.Players.Remove(deletePlayer);
                    context.Response.StatusCode = StatusCodes.Status202Accepted;
                    await db.SaveChangesAsync();
                    return Results.Accepted("Player Delete Success");
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