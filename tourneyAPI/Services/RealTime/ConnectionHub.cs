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

    public async Task UpdatePlayers(string gameId)
    =>
        await Clients.Others.SendAsync(JsonSerializer.Serialize(await _gameService.GetPlayersInGame(Guid.Parse(gameId))));



    public async Task NotifyOthers(string playerName)
=> await Clients.Others.SendAsync($"{playerName} Joined!");


}