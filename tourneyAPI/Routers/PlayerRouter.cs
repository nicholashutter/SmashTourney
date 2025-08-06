namespace Routers;

using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Validators;
using Services;
using System;
using Serilog;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]

/* this playerRouter class may not need to be exposed and may be deleted */
public static class PlayerRouter
{
    public static void Map(WebApplication app)
    {

        var PlayerRoutes = app.MapGroup("/Players");

        PlayerRoutes.MapGet("/", async (HttpContext context, ApplicationDbContext db, IPlayerManager playerManager) =>
        {
            Log.Information("Request Type: Get \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                var Players = await playerManager.GetAllPlayersAsync();

                return Results.Ok(Players);
            }
            catch (Exception)
            {
                return Results.Problem("Internal Server Error");
            }
        });

        PlayerRoutes.MapPost("/", async (HttpContext context, ApplicationDbContext db, IPlayerManager playerManager, Player player) =>
        {
            Log.Information("Request Type: Post \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                PlayerValidator.Validate(player, "CreatePlayerRoute");

                await playerManager.CreateAsync(player);

                return Results.Created();
            }
            catch (Exception)
            {
                return Results.BadRequest("Invalid JSON Payload");
            }

        });

        PlayerRoutes.MapPut("/", async (HttpContext context, ApplicationDbContext db, IPlayerManager playerManager, Player player) =>
        {
            Log.Information("Request Type: Put \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                PlayerValidator.Validate(player, "UpdatePlayerRoute");

                await playerManager.UpdateAsync(player);
                return Results.Ok();

            }
            catch (Exception)
            {
                return Results.BadRequest("Invalid JSON Payload");
            }
        });

        PlayerRoutes.MapDelete("/{Id}", async (HttpContext context, ApplicationDbContext db, IPlayerManager playerManager, string Id) =>
        {
            Log.Information("Request Type: Delete \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);
            try
            {
                await playerManager.DeleteAsync(Guid.Parse(Id));
                return Results.Accepted("Player Delete Success");

            }
            catch (Exception)
            {
                return Results.BadRequest("Invalid JSON Payload");
            }
        });
    }
}