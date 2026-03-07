namespace ApiTests;

using System.Net;
using System.Text.Json;
using Contracts;
using Enums;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies realtime broadcast behavior that keeps clients synchronized during tournament play.
public class RealTimeTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private const string InMemoryHubUrl = "wss://localhost/hubs/GameServiceHub";

    // Initializes realtime test host access.
    public RealTimeTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;

        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.Database.EnsureCreated();
    }

    // Creates a SignalR client connected to the in-memory test server.
    private HubConnection CreateClient()
    {
        var server = _factory.Server;

        return new HubConnectionBuilder()
            .WithUrl(InMemoryHubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();
    }

    // Confirms the hub route is registered and reachable.
    [Fact]
    public async Task HubRouteShouldBeReachable()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(InMemoryHubUrl);

        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Confirms a newly connected client receives the join acknowledgment event.
    [Fact]
    public async Task SuccessMessageOnPlayerConnection()
    {
        const string expectedResult = "Success";

        var completionSource = new TaskCompletionSource<string>();
        var connection = CreateClient();

        connection.On("SuccessfullyJoined", () =>
        {
            completionSource.TrySetResult(expectedResult);
        });

        await connection.StartAsync();

        var actualResult = await completionSource.Task;

        Assert.Equal(expectedResult, actualResult);
    }

    // Confirms game-start notifications are broadcast to all clients in the same game group.
    [Fact]
    public async Task NotifyGameStartedBroadcastsToAllClientsInGroup()
    {
        var gameId = Guid.NewGuid().ToString();

        var hostConnection = CreateClient();
        var guestConnection = CreateClient();

        var hostReceived = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var guestReceived = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        hostConnection.On<string>("GameStarted", receivedGameId =>
        {
            hostReceived.TrySetResult(receivedGameId);
        });

        guestConnection.On<string>("GameStarted", receivedGameId =>
        {
            guestReceived.TrySetResult(receivedGameId);
        });

        await hostConnection.StartAsync();
        await guestConnection.StartAsync();

        await hostConnection.InvokeAsync("JoinGameGroup", gameId);
        await guestConnection.InvokeAsync("JoinGameGroup", gameId);

        await hostConnection.InvokeAsync("NotifyGameStarted", gameId);

        var hostGameStarted = await hostReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var guestGameStarted = await guestReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var bothClientsReceivedExpectedGameId = hostGameStarted == gameId && guestGameStarted == gameId;
        Assert.True(bothClientsReceivedExpectedGameId);

        await hostConnection.DisposeAsync();
        await guestConnection.DisposeAsync();
    }

    // Confirms game creation can be invoked through SignalR contract.
    [Fact]
    public async Task CreateGameWithModeReturnsGameId()
    {
        var connection = CreateClient();
        await connection.StartAsync();

        var response = await connection.InvokeAsync<JsonElement>(
            "CreateGameWithMode",
            new CreateGameOptions(BracketMode.SINGLE_ELIMINATION, 4));

        var resolvedGameId = Guid.Empty;
        if (response.TryGetProperty("gameId", out var camelCaseGameIdProperty))
        {
            Guid.TryParse(camelCaseGameIdProperty.GetString(), out resolvedGameId);
        }
        else if (response.TryGetProperty("GameId", out var pascalCaseGameIdProperty))
        {
            Guid.TryParse(pascalCaseGameIdProperty.GetString(), out resolvedGameId);
        }

        var responseContainsGameId = resolvedGameId != Guid.Empty;
        Assert.True(responseContainsGameId);

        await connection.DisposeAsync();
    }
}
