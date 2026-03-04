namespace ApiTests;

using System.Net;
using Microsoft.AspNetCore.SignalR.Client;

// Verifies realtime broadcast behavior that keeps clients synchronized during tournament play.
public class RealTimeTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private const string InMemoryHubUrl = "wss://localhost/hubs/GameServiceHub";

    // Initializes realtime test host access.
    public RealTimeTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
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

        connection.On("Successfully Joined", () =>
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
}
