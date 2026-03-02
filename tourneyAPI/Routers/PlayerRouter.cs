namespace Routers;

using Entities;
using Validators;
using Services;
using System;
using Serilog;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]

// Maps player CRUD endpoints used by administrative and testing flows.
public static class PlayerRouter
{
    // Registers all player API endpoints.
    public static void Map(WebApplication app)
    {
        var playerRoutes = app.MapGroup("/Players");

        playerRoutes.MapGet("/", async (HttpContext context, ApplicationDbContext db, IPlayerManager playerManager) =>
        {
            Log.Information("Request Type: Get \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);

            try
            {
                var players = await playerManager.GetAllPlayersAsync();

                return Results.Ok(players);
            }
            catch (Exception)
            {
                return Results.Problem("Internal Server Error");
            }
        });

        playerRoutes.MapPost("/", async (HttpContext context, ApplicationDbContext db, IPlayerManager playerManager, Player player) =>
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

        playerRoutes.MapPut("/", async (HttpContext context, ApplicationDbContext db, IPlayerManager playerManager, Player player) =>
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

        playerRoutes.MapDelete("/{Id}", async (HttpContext context, ApplicationDbContext db, IPlayerManager playerManager, string Id) =>
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