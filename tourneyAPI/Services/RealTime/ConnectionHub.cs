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

    public async Task UpdatePlayers(gameId)
    {

        //current behavior is just to work out the kinks in testing environment
        //code commented out below is actual method intended behavior
        var players = await _gameService.GetPlayersInGame(Guid.Parse(gameId))

        await Clients.Others.SendAsync("PlayersUpdated", players);


    }



}