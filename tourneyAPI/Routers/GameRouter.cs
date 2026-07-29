namespace Routers;

using Contracts;
using Entities;
using Services;
using System;
using Serilog;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

// Maps game lifecycle and bracket progression endpoints.
public static class GameRouter
{
    // Registers all game API endpoints.
    public static void Map(WebApplication app)
    {
        // Authorization is applied to the whole group rather than per route.
        //
        // Every tournament action belongs to a signed-in user, so the default
        // has to be closed: a route added later is protected because it joined
        // this group, not because someone remembered to protect it. Handlers
        // that need the caller's identity still read it from claims — this only
        // guarantees there is an identity to read.
        var gameRoutes = app.MapGroup("/Games").RequireAuthorization();

        gameRoutes.MapPost("/CreateGameWithMode", async (HttpContext context, IGameService gameService, CreateGameOptions options) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/CreateGameWithMode' \n Time:{Timestamp}", DateTime.UtcNow);

            // The creator is recorded as host from their claims, not from the
            // request body, so host-ness is something the server knows rather
            // than something a client can assert on its way back in.
            var hostUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Guid gameId = await gameService.CreateGame(options, hostUserId);

            var response = new { GameId = gameId, options.BracketMode };

            return Results.Ok(response);
        });

        // Lists every tournament on the server, as this caller sees it.
        //
        // Until now there was no way to find out what existed: joining meant
        // being told a GUID and typing it into a phone. Any signed-in user may
        // see this list — it is the room's noticeboard, and the payload is
        // deliberately thin enough that being on it gives nothing away beyond
        // "this game exists and has this many people in it".
        //
        // Retiring stale games is not this route's job. It used to sweep here,
        // which made drawing a menu delete rows and left cleanup dependent on
        // somebody happening to open the browser. StaleGameSweeper runs it on a
        // timer instead, so this stays a read.
        gameRoutes.MapGet("/GetActiveGames", async (HttpContext context, IGameService gameService) =>
        {
            Log.Information("Request Type: Get \n URL: '/Games/GetActiveGames' \n Time:{Timestamp}", DateTime.UtcNow);

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var gameSummaries = await gameService.GetGameSummariesAsync(userId);

            return Results.Ok(gameSummaries);
        });

        // Ends a tournament and deletes it along with its players.
        //
        // Group authorization only guarantees somebody is signed in, which is
        // nowhere near enough for a route that destroys a game other people are
        // still playing. The host check is done by the service against the
        // stored host id, so the caller's claim is the only thing that can
        // authorize it — a shared game id cannot.
        gameRoutes.MapPost("/EndGame/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/EndGame' \n Time:{Timestamp}", DateTime.UtcNow);

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var endGameStatus = await gameService.EndGameAsync(gameId, userId);

            return endGameStatus switch
            {
                EndGameStatus.ENDED => Results.Ok($"Game {gameId} ended"),
                EndGameStatus.GAME_NOT_FOUND => Results.NotFound(),
                EndGameStatus.NOT_HOST => Results.Json("Only the host can end this game.", statusCode: 403),
                _ => Results.Problem("Internal Server Error")
            };
        });

        gameRoutes.MapPost("/GetPlayersInGame/{gameId}", async (HttpContext context, Guid gameId, IGameService gameService) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/GetPlayersInGame' \n Time:{Timestamp}", DateTime.UtcNow);

            Game? game = await gameService.GetGameByIdAsync(gameId);

            if (game is null)
            {
                return Results.NotFound();
            }

            var response = new
            {
                game.currentPlayers
            };

            return Results.Ok(response);
        });



        gameRoutes.MapPost("/AddPlayer/{gameId}", (HttpContext context, IGameService gameService, Guid gameId, Player player) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/AddPlayer' \n Time:{Timestamp}", DateTime.UtcNow);

            // Resolves the authenticated user id from auth claims.
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            bool success = gameService.AddPlayerToGame(player, gameId, userId);
            if (!success)
            {
                return Results.Problem("Internal Server Error");
            }

            return Results.Ok($"Players Added to Game {gameId}");
        });

        gameRoutes.MapPost("/StartGame/{gameId}", async (HttpContext context, IGameService gameService, IHubContext<ConnectionHub> hubContext, Guid gameId) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/StartGame' \n Time:{Timestamp}", DateTime.UtcNow);

            bool success = await gameService.StartGameAsync(gameId);

            if (!success)
            {
                return Results.Problem("Internal Server Error");
            }

            await hubContext.Clients.Group(gameId.ToString()).SendAsync("GameStarted", gameId.ToString());

            return Results.Ok($"Game {gameId} successfully started");
        });

        gameRoutes.MapGet("/GetBracket/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Get \n URL: '/Games/GetBracket' \n Time:{Timestamp}", DateTime.UtcNow);

            var snapshot = await gameService.GetBracketSnapshotAsync(gameId);
            if (snapshot is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(snapshot);
        });

        gameRoutes.MapGet("/GetCurrentMatch/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Get \n URL: '/Games/GetCurrentMatch' \n Time:{Timestamp}", DateTime.UtcNow);

            var currentMatch = await gameService.GetCurrentMatchAsync(gameId);
            if (currentMatch is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(currentMatch);
        });

        // Rebuilds a dropped player's session from the game id in their URL.
        //
        // This is the whole reconnect story: a phone that lost its tab still has
        // the identity cookie, and the player row is already stored against the
        // game, so the client can ask "who am I here and where do I belong" and
        // be told. A 404 means this user is genuinely not in this game, which
        // the client treats as an invitation to join rather than an error.
        gameRoutes.MapGet("/GetPlayerSession/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Get \n URL: '/Games/GetPlayerSession' \n Time:{Timestamp}", DateTime.UtcNow);

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var playerSession = await gameService.GetPlayerSessionAsync(gameId, userId);
            if (playerSession is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(playerSession);
        });

        gameRoutes.MapGet("/GetFlowState/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Get \n URL: '/Games/GetFlowState' \n Time:{Timestamp}", DateTime.UtcNow);

            var flowState = await gameService.GetGameStateAsync(gameId);
            if (flowState is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(flowState);
        });
        gameRoutes.MapPost("/SubmitMatchVote/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId, SubmitMatchVoteRequest request) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/SubmitMatchVote' \n Time:{Timestamp}", DateTime.UtcNow);

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var voteResult = await gameService.SubmitMatchVoteAsync(gameId, userId, request);

            return voteResult.Status switch
            {
                SubmitMatchVoteStatus.PENDING => Results.Ok(voteResult),
                SubmitMatchVoteStatus.COMMITTED => Results.Ok(voteResult),
                SubmitMatchVoteStatus.GAME_NOT_FOUND => Results.NotFound(voteResult),
                SubmitMatchVoteStatus.VOTER_NOT_PARTICIPANT => Results.Json(voteResult, statusCode: 403),
                SubmitMatchVoteStatus.DUPLICATE_VOTE => Results.Conflict(voteResult),
                SubmitMatchVoteStatus.CONFLICT => Results.Conflict(voteResult),
                SubmitMatchVoteStatus.MATCH_NOT_ACTIVE => Results.Conflict(voteResult),
                SubmitMatchVoteStatus.INVALID_WINNER => Results.BadRequest(voteResult),
                SubmitMatchVoteStatus.BRACKET_NOT_STARTED => Results.BadRequest(voteResult),
                SubmitMatchVoteStatus.APPLY_FAILED => Results.Problem("Vote consensus reached but match could not be applied", statusCode: 500),
                _ => Results.BadRequest(voteResult)
            };
        });

    }
}
