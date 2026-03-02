using Microsoft.AspNetCore.SignalR;
using Services;

// Handles realtime game notifications and group membership over SignalR.
public class ConnectionHub : Hub
{
    private readonly IGameService _gameService;

    // Creates a new hub instance with access to game state services.
    public ConnectionHub(IGameService gameService)
    {
        _gameService = gameService;
    }

    // Adds a new connection to the default game service hub group.
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "GameServiceHub");
        await Clients.Caller.SendAsync("Successfully Joined");

    }

    // Adds a connection to a specific game group.
    public async Task JoinGameGroup(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
    }

    // Broadcasts updated player lists to a game group.
    public async Task UpdatePlayers(string gameId)
    {
        Guid parsedGameId;
        try
        {
            parsedGameId = new Guid(gameId);
        }
        catch (FormatException)
        {
            return;
        }

        var players = await _gameService.GetPlayersInGame(parsedGameId);

        await Clients.Group(gameId).SendAsync("PlayersUpdated", players);


    }

    // Broadcasts that a game has started to a game group.
    public async Task NotifyGameStarted(string gameId)
    {
        await Clients.Group(gameId).SendAsync("GameStarted", gameId);
    }

}