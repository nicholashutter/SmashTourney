namespace Routers;

using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services;
using System;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Validators;

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

        GameRoutes.MapPost("/AddPlayers", (HttpContext context, IGameService gameService, Guid gameId, List<Player> players) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/AllPlayersIn' \n Time:{Timestamp}", DateTime.UtcNow);

            PlayersValidator.Validate(players, "AddPlayersRoute");

            bool success = gameService.AddPlayersToGame(players, gameId);

            if (!success)
            {
                return Results.Problem("Internal Server Error");
            }

            return Results.Ok($"Players Added to Game {gameId}");
        });

        GameRoutes.MapPost("/StartGame", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/StartGame' \n Time:{Timestamp}", DateTime.UtcNow);

            bool success = await gameService.StartGameAsync(gameId);

            if (!success)
            {
                return Results.Problem("Internal Server Error");
            }

            return Results.Ok($"Game {gameId} successfully started");
        });

        GameRoutes.MapPost("/LoadGame", async(HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/LoadGame' \n Time:{Timestamp}", DateTime.UtcNow);

            bool success = await gameService.LoadGameAsync(gameId);

            if (!success)
            {
                return Results.Problem("Internal Server Error");
            }

            return Results.Ok($"Game {gameId} successfully loaded");
        });

        GameRoutes.MapPost("/SaveGame", async (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/SaveGame' \n Time:{Timestamp}", DateTime.UtcNow);

            await gameService.UpdateGameAsync(gameId);

            return Results.Ok($"Game {gameId} saved");
        });

        GameRoutes.MapPost("/StartRound", (HttpContext context, IGameService gameService, Guid gameId) =>
        {
            Log.Information("Request Type: Post \n URL: '/Games/StartRound' \n Time:{Timestamp}", DateTime.UtcNow);

            List<Player>? players = gameService.StartRound(gameId);

            if (players is null)
            {
                return Results.Problem("Internal Server Error");
            }

            var response = new { Players = players };

            return Results.Ok(response);
        });


        GameRoutes.MapPost("/StartMatch", (HttpContext context, IGameService gameService, Guid gameId) =>
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

        GameRoutes.MapPost("/EndMatch", async (HttpContext context, IGameService gameService, Guid gameId, Player MatchWinner) =>
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

    }
}