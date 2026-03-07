using System.Security.Claims;
using Contracts;
using Entities;
using Microsoft.AspNetCore.SignalR;
using Services;

// Handles realtime game management and authoritative state broadcasts over SignalR.
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
        await Clients.Caller.SendAsync("SuccessfullyJoined");
    }

    // Adds a connection to a specific game group and sends current state snapshot to caller.
    public async Task JoinGameGroup(string gameId)
    {
        var parsedGameId = ParseGameIdOrThrow(gameId);

        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        await SendCurrentGameStateToCallerAsync(parsedGameId);
    }

    // Creates a game with selected bracket options.
    public async Task<CreateGameRealtimeResponse> CreateGameWithMode(CreateGameOptions options)
    {
        var gameId = await _gameService.CreateGame(options);
        return new CreateGameRealtimeResponse(gameId, options.BracketMode);
    }

    // Adds one authenticated player to a game and broadcasts updated roster.
    public async Task<bool> AddPlayer(string gameId, Player player)
    {
        var parsedGameId = ParseGameIdOrThrow(gameId);
        var userId = ResolveAuthenticatedUserIdOrThrow();

        var success = _gameService.AddPlayerToGame(player, parsedGameId, userId);
        if (!success)
        {
            return false;
        }

        var players = await _gameService.GetPlayersInGame(parsedGameId);
        await Clients.Group(gameId).SendAsync("PlayersUpdated", players);
        return true;
    }

    // Starts game progression and broadcasts initial state updates.
    public async Task<bool> StartGame(string gameId)
    {
        var parsedGameId = ParseGameIdOrThrow(gameId);
        var success = await _gameService.StartGameAsync(parsedGameId);
        if (!success)
        {
            return false;
        }

        await Clients.Group(gameId).SendAsync("GameStarted", gameId);
        await BroadcastGameStateToGroupAsync(parsedGameId, gameId);
        return true;
    }

    // Returns players currently assigned to a game.
    public async Task<List<Player>> GetPlayersInGame(string gameId)
    {
        var parsedGameId = ParseGameIdOrThrow(gameId);
        return await _gameService.GetPlayersInGame(parsedGameId);
    }

    // Returns a full bracket snapshot for client rendering.
    public async Task<BracketSnapshotResponse?> GetBracket(string gameId)
    {
        var parsedGameId = ParseGameIdOrThrow(gameId);
        return await _gameService.GetBracketSnapshotAsync(parsedGameId);
    }

    // Returns the currently active match for a game.
    public async Task<CurrentMatchResponse?> GetCurrentMatch(string gameId)
    {
        var parsedGameId = ParseGameIdOrThrow(gameId);
        return await _gameService.GetCurrentMatchAsync(parsedGameId);
    }

    // Returns the high-level flow state for route decisions.
    public async Task<GameStateResponse?> GetFlowState(string gameId)
    {
        var parsedGameId = ParseGameIdOrThrow(gameId);
        return await _gameService.GetGameStateAsync(parsedGameId);
    }

    // Submits one authenticated vote and broadcasts resulting game state transitions.
    public async Task<SubmitMatchVoteResponse> SubmitMatchVote(string gameId, SubmitMatchVoteRequest request)
    {
        var parsedGameId = ParseGameIdOrThrow(gameId);
        var voterUserId = ResolveAuthenticatedUserIdOrThrow();

        var voteResponse = await _gameService.SubmitMatchVoteAsync(parsedGameId, voterUserId, request);

        await Clients.Group(gameId).SendAsync("VoteSubmitted", voteResponse);
        await BroadcastGameStateToGroupAsync(parsedGameId, gameId);

        return voteResponse;
    }

    // Broadcasts updated players list to a game group.
    public async Task UpdatePlayers(string gameId)
    {
        var parsedGameId = ParseGameIdOrThrow(gameId);
        var players = await _gameService.GetPlayersInGame(parsedGameId);
        await Clients.Group(gameId).SendAsync("PlayersUpdated", players);
    }

    // Broadcasts game-start event to a game group.
    public async Task NotifyGameStarted(string gameId)
    {
        ParseGameIdOrThrow(gameId);
        await Clients.Group(gameId).SendAsync("GameStarted", gameId);
    }

    // Sends current game state payloads to the caller.
    private async Task SendCurrentGameStateToCallerAsync(Guid parsedGameId)
    {
        var players = await _gameService.GetPlayersInGame(parsedGameId);
        await Clients.Caller.SendAsync("PlayersUpdated", players);

        var flowState = await _gameService.GetGameStateAsync(parsedGameId);
        await Clients.Caller.SendAsync("FlowStateUpdated", flowState);

        var currentMatch = await _gameService.GetCurrentMatchAsync(parsedGameId);
        await Clients.Caller.SendAsync("CurrentMatchUpdated", currentMatch);

        var bracket = await _gameService.GetBracketSnapshotAsync(parsedGameId);
        await Clients.Caller.SendAsync("BracketUpdated", bracket);
    }

    // Broadcasts full game state payloads to one game group.
    private async Task BroadcastGameStateToGroupAsync(Guid parsedGameId, string gameId)
    {
        var flowState = await _gameService.GetGameStateAsync(parsedGameId);
        await Clients.Group(gameId).SendAsync("FlowStateUpdated", flowState);

        var currentMatch = await _gameService.GetCurrentMatchAsync(parsedGameId);
        await Clients.Group(gameId).SendAsync("CurrentMatchUpdated", currentMatch);

        var bracket = await _gameService.GetBracketSnapshotAsync(parsedGameId);
        await Clients.Group(gameId).SendAsync("BracketUpdated", bracket);
    }

    // Parses game identifier string into GUID or throws hub exception.
    private static Guid ParseGameIdOrThrow(string gameId)
    {
        if (Guid.TryParse(gameId, out var parsedGameId))
        {
            return parsedGameId;
        }

        throw new HubException("INVALID_GAME_ID");
    }

    // Resolves authenticated user identifier from current hub context.
    private string ResolveAuthenticatedUserIdOrThrow()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return userId;
        }

        throw new HubException("UNAUTHORIZED");
    }
}