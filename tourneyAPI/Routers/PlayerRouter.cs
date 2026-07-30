namespace Routers;

using Entities;
using Validators;
using Services;
using System;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

[ExcludeFromCodeCoverage]

// Maps player CRUD endpoints, scoped to the calling user's own player records.
public static class PlayerRouter
{
    // Registers all player API endpoints.
    public static void Map(WebApplication app)
    {
        // Authorization covers the group, but being signed in was the entire
        // gate here and these are unscoped CRUD endpoints over the whole player
        // table. Any account that registered could read every player in every
        // tournament, rewrite any of them, or delete them all — and the client
        // never calls these routes at all, so nothing legitimate wanted that
        // reach.
        //
        // Every route below now works only on rows the caller owns, identified
        // by the UserId already stored on each player. That keeps these usable
        // for the administrative and test flows they exist for without leaving a
        // way to vandalise other people's games.
        var playerRoutes = app.MapGroup("/Players").RequireAuthorization();

        // Reads the caller's own player records.
        playerRoutes.MapGet("/", async (HttpContext context, IPlayerManager playerManager) =>
        {
            Log.Information("Request Type: Get \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var players = await playerManager.GetAllPlayersAsync() ?? new List<Player>();

                var ownPlayers = players
                    .Where(player => string.Equals(player.UserId, userId, StringComparison.Ordinal))
                    .ToList();

                return Results.Ok(ownPlayers);
            }
            catch (Exception)
            {
                return Results.Problem("Internal Server Error");
            }
        });

        // Creates a player owned by the caller.
        //
        // The owning user id comes from claims and overwrites whatever the body
        // said, so this cannot be used to plant a player belonging to someone
        // else.
        playerRoutes.MapPost("/", async (HttpContext context, IPlayerManager playerManager, Player player) =>
        {
            Log.Information("Request Type: Post \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            try
            {
                PlayerValidator.Validate(player, "CreatePlayerRoute");

                player.UserId = userId;

                await playerManager.CreateAsync(player);

                return Results.Created();
            }
            catch (Exception)
            {
                return Results.BadRequest("Invalid JSON Payload");
            }
        });

        // Updates one of the caller's own player records.
        playerRoutes.MapPut("/", async (HttpContext context, IPlayerManager playerManager, Player player) =>
        {
            Log.Information("Request Type: Put \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            try
            {
                PlayerValidator.Validate(player, "UpdatePlayerRoute");
            }
            catch (Exception)
            {
                return Results.BadRequest("Invalid JSON Payload");
            }

            // Ownership is read from storage, never from the payload. Trusting a
            // body field here would let anyone claim a row by asserting it.
            var storedPlayer = await playerManager.GetByIdAsync(player.Id);
            if (storedPlayer is null || !string.Equals(storedPlayer.UserId, userId, StringComparison.Ordinal))
            {
                Log.Warning("Refused player update for {PlayerId} requested by user {UserId}", player.Id, userId);
                return Results.NotFound();
            }

            try
            {
                player.UserId = userId;

                await playerManager.UpdateAsync(player);
                return Results.Ok();
            }
            catch (Exception)
            {
                return Results.BadRequest("Invalid JSON Payload");
            }
        });

        // Deletes one of the caller's own player records.
        playerRoutes.MapDelete("/{Id}", async (HttpContext context, IPlayerManager playerManager, string Id) =>
        {
            Log.Information("Request Type: Delete \n URL: '/Players' \n Time:{Timestamp}", DateTime.UtcNow);

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            if (!Guid.TryParse(Id, out var playerId))
            {
                return Results.BadRequest("Invalid JSON Payload");
            }

            var storedPlayer = await playerManager.GetByIdAsync(playerId);
            if (storedPlayer is null || !string.Equals(storedPlayer.UserId, userId, StringComparison.Ordinal))
            {
                Log.Warning("Refused player delete for {PlayerId} requested by user {UserId}", playerId, userId);
                return Results.NotFound();
            }

            try
            {
                await playerManager.DeleteAsync(playerId);
                return Results.Accepted("Player Delete Success");
            }
            catch (Exception)
            {
                return Results.BadRequest("Invalid JSON Payload");
            }
        });
    }
}
