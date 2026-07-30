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
    // Returns a rejection when the caller does not belong to the game, or null
    // when they do and the handler should carry on.
    //
    // Group authorization only establishes that somebody is signed in. That was
    // the whole gate on every read of a game, which meant one registered account
    // could walk any game id and read the bracket, the live match and the roster
    // of every tournament on the server. Belonging to the game is the actual
    // requirement, and it is checked per request rather than trusted from the
    // client.
    //
    // Non-membership answers 404, not 403. A 403 would confirm the game exists,
    // which is exactly the fact an outsider should not be able to harvest by
    // walking ids.
    private static async Task<IResult?> RejectNonMembersAsync(
        HttpContext context,
        IGameService gameService,
        Guid gameId)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        if (!await gameService.IsUserInGameAsync(gameId, userId))
        {
            Log.Warning("Refused access to game {GameId} for user {UserId} who is not in it", gameId, userId);
            return Results.NotFound();
        }

        return null;
    }

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

        // Returns the roster of a game the caller belongs to.
        //
        // This used to answer for any game id any signed-in account asked about,
        // handing out the display names of everyone in a stranger's tournament.
        gameRoutes.MapPost("/GetPlayersInGame/{gameId}", async (HttpContext context, Guid gameId, IGameService gameService) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/GetPlayersInGame' \n Time:{Timestamp}", DateTime.UtcNow);

            var membershipFailure = await RejectNonMembersAsync(context, gameService, gameId);
            if (membershipFailure is not null)
            {
                return membershipFailure;
            }

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

        // Starts a tournament. Host only.
        //
        // This had no check of any kind beyond being signed in, so any account
        // that knew a game id could start somebody else's tournament — locking
        // the lobby and seeding the bracket around whoever happened to have
        // joined at that moment. Starting is the host's call.
        gameRoutes.MapPost("/StartGame/{gameId}", async (HttpContext context, IGameService gameService, IHubContext<ConnectionHub> hubContext, Guid gameId) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/StartGame' \n Time:{Timestamp}", DateTime.UtcNow);

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            if (!await gameService.IsUserHostOfGameAsync(gameId, userId))
            {
                // A non-member learns only that there is nothing here for them;
                // a member who simply is not the host is told plainly.
                if (!await gameService.IsUserInGameAsync(gameId, userId))
                {
                    return Results.NotFound();
                }

                Log.Warning("Refused StartGame for game {GameId} because user {UserId} does not host it", gameId, userId);
                return Results.Json("Only the host can start this game.", statusCode: 403);
            }

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

            var membershipFailure = await RejectNonMembersAsync(context, gameService, gameId);
            if (membershipFailure is not null)
            {
                return membershipFailure;
            }

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

            var membershipFailure = await RejectNonMembersAsync(context, gameService, gameId);
            if (membershipFailure is not null)
            {
                return membershipFailure;
            }

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

            var membershipFailure = await RejectNonMembersAsync(context, gameService, gameId);
            if (membershipFailure is not null)
            {
                return membershipFailure;
            }

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
