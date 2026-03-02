namespace Routers;

using Contracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services;
using System;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Validators;
using System.Security.Claims;

public static class GameRouter
{
    public static void Map(WebApplication app)
    {
        var GameRoutes = app.MapGroup("/Games");


        GameRoutes.MapGet("/getAllGames", async (HttpContext context, IGameService gameService) =>
        {
            Log.Information("Request Type: Get \n URL: '/Games/getAllGames' \n Time:{Timestamp}", DateTime.UtcNow);

            List<Game>? games = await gameService.GetAllGamesAsync();

            if (games is null)
            {
                return Results.Problem("Internal Server Error");
            }

            var response = new { Games = games };

            return Results.Ok(response);
        });

        GameRoutes.MapPost("/CreateGame", async (HttpContext context, IGameService gameService) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/CreateGame' \n Time:{Timestamp}", DateTime.UtcNow);

            Guid gameId = await gameService.CreateGame();

            var response = new { GameId = gameId };

            return Results.Ok(response);
        });

        GameRoutes.MapPost("/CreateGameWithMode", async (HttpContext context, IGameService gameService, CreateGameOptions options) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/CreateGameWithMode' \n Time:{Timestamp}", DateTime.UtcNow);

            Guid gameId = await gameService.CreateGame(options);

            var response = new { GameId = gameId, options.BracketMode };

            return Results.Ok(response);
        });


        GameRoutes.MapGet("/GetGameById/{gameId}", async (HttpContext context, Guid gameId, IGameService gameService) =>
        {
            Log.Information("Request Type: Get \n URL: '/Games/GetGameById' \n Time:{Timestamp}", DateTime.UtcNow);

            Game? game = await gameService.GetGameByIdAsync(gameId);

            if (game is null)
            {
                return Results.NotFound();
            }

            var response = new { Game = game };

            return Results.Ok(response);
        });

        GameRoutes.MapPost("/GetPlayersInGame/{gameId}", async (HttpContext context, Guid gameId, IGameService gameService) =>
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


        GameRoutes.MapGet("/EndGame/{gameId}", (HttpContext context, Guid gameId, IGameService gameService) =>
        {
            Log.Information("Request Type: Get \n URL: '/Games/EndGame' \n Time:{Timestamp}", DateTime.UtcNow);

            bool success = gameService.EndGame(gameId);

            if (!success)
            {
                return Results.Problem("Internal Server Error");
            }

            return Results.Ok();
        });



        GameRoutes.MapPost("/AddPlayer/{gameId}", (HttpContext context, IGameService gameService, Guid gameId, Player player) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/AddPlayer' \n Time:{Timestamp}", DateTime.UtcNow);

            // this should get the userId from the auth token
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            // PlayerValidator.Validate(player, "AddPlayerRoute");

            bool success = gameService.AddPlayerToGame(player, gameId, userId);
            if (!success)
            {
                return Results.Problem("Internal Server Error");
            }

            return Results.Ok($"Players Added to Game {gameId}");
        });


        GameRoutes.MapPost("/CreateUserSession", (HttpContext context, IGameService gameService, ApplicationUser user) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/AllPlayersIn' \n Time:{Timestamp}", DateTime.UtcNow);

            UserValidator.Validate(user, "CreateUserSessionRoute");

            bool success = gameService.CreateUserSession(user);

            if (!success)
            {
                return Results.Problem("Internal Server Error");
            }

            return Results.Ok($"User Session Created for {user.UserName}");
        });

        GameRoutes.MapPost("/StartGame/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/StartGame' \n Time:{Timestamp}", DateTime.UtcNow);

            bool success = await gameService.StartGameAsync(gameId);

            if (!success)
            {
                return Results.Problem("Internal Server Error");
            }

            return Results.Ok($"Game {gameId} successfully started");
        });

        GameRoutes.MapPost("/LoadGame/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/LoadGame' \n Time:{Timestamp}", DateTime.UtcNow);

            bool success = await gameService.LoadGameAsync(gameId);

            if (!success)
            {
                return Results.Problem("Internal Server Error");
            }

            return Results.Ok($"Game {gameId} successfully loaded");
        });

        GameRoutes.MapPost("/SaveGame/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/SaveGame' \n Time:{Timestamp}", DateTime.UtcNow);

            await gameService.UpdateGameAsync(gameId);

            return Results.Ok($"Game {gameId} saved");
        });

        GameRoutes.MapPost("/StartRound/{gameId}", (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/StartRound' \n Time:{Timestamp}", DateTime.UtcNow);

            gameService.StartRound(gameId);

            return Results.Ok();
        });


        GameRoutes.MapPost("/StartMatch/{gameId}", (HttpContext context, IGameService gameService, Guid gameId) =>
                {
                    Log.Information("Request Type: Post \n URL: '/Games/StartMatch' \n Time:{Timestamp}", DateTime.UtcNow);

                    List<Player>? players = gameService.StartMatch(gameId);

                    if (players is null)
                    {
                        return Results.Problem("Internal Server Error");
                    }

                    var response = new { Players = players };

                    return Results.Ok(response);
                });

        GameRoutes.MapPost("/EndMatch/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId, Player MatchWinner) =>
                {
                    Log.Information("Request Type: Post \n URL: '/Games/EndMatch' \n Time:{Timestamp}", DateTime.UtcNow);

                    PlayerValidator.Validate(MatchWinner, "EndMatchRoute");

                    bool success = await gameService.EndMatchAsync(gameId, MatchWinner);

                    if (!success)
                    {
                        return Results.Problem("Internal Server Error");
                    }

                    return Results.Ok($"Current match for game {gameId} ended");
                });

        GameRoutes.MapGet("/GetBracket/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Get \n URL: '/Games/GetBracket' \n Time:{Timestamp}", DateTime.UtcNow);

            var snapshot = await gameService.GetBracketSnapshotAsync(gameId);
            if (snapshot is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(snapshot);
        });

        GameRoutes.MapGet("/GetCurrentMatch/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Get \n URL: '/Games/GetCurrentMatch' \n Time:{Timestamp}", DateTime.UtcNow);

            var currentMatch = await gameService.GetCurrentMatchAsync(gameId);
            if (currentMatch is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(currentMatch);
        });

        GameRoutes.MapPost("/ReportMatch/{gameId}", async (HttpContext context, IGameService gameService, Guid gameId, ReportMatchRequest request) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/ReportMatch' \n Time:{Timestamp}", DateTime.UtcNow);

            var success = await gameService.ReportMatchResultAsync(gameId, request);
            if (!success)
            {
                return Results.Problem("Match report could not be applied", statusCode: 501);
            }

            return Results.Ok();
        });

    }
}
