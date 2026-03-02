using System.Runtime.CompilerServices;
using System.Text.Json;
using Entities;
using Microsoft.AspNetCore.SignalR;
using Services;

public class ConnectionHub : Hub
{
    private readonly IGameService _gameService;

    public ConnectionHub(IGameService gameService)
    {
        _gameService = gameService;
    }
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "GameServiceHub");
        await Clients.Caller.SendAsync("Successfully Joined");

    }

    public async Task JoinGameGroup(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
    }

    public async Task UpdatePlayers(string gameId)
    {
        var players = await _gameService.GetPlayersInGame(Guid.Parse(gameId));

        await Clients.Others.SendAsync("PlayersUpdated", players);


    }

    public async Task NotifyGameStarted(string gameId)
    {
        await Clients.Group(gameId).SendAsync("GameStarted", gameId);
    }

}