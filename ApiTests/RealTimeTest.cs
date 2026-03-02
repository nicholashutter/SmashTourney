using System.Net;
using ApiTests;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System.Threading.Tasks;

public class RealTimeTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private const string IN_MEMORY_HUB_URL = "wss://localhost/hubs/GameServiceHub";
    public RealTimeTest(CustomWebApplicationFactory<Program> factory) => _factory = factory;

    private HubConnection CreateClient()
    {
        var server = _factory.Server;
        return new HubConnectionBuilder()
            .WithUrl(IN_MEMORY_HUB_URL, options => options.HttpMessageHandlerFactory = _ => server.CreateHandler())
            .Build();
    }

    [Fact]
    public async Task HubRouteShouldBeReachable()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("wss://localhost/hubs/GameServiceHub");
        Assert.False(response.StatusCode == HttpStatusCode.NotFound, "Hub route is not registered");
    }


    [Fact]
    public async Task SuccessMessageOnPlayerConnection()
    {

        const string EXPECTED_RESULT = "Success";

        TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();


        var _connection = CreateClient();

        _connection.On("Successfully Joined", () =>
        {
            taskCompletionSource.TrySetResult(EXPECTED_RESULT);
        });

        await _connection.StartAsync();

        string result = await taskCompletionSource.Task;

        Assert.Equal(EXPECTED_RESULT, result);
    }
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

        Assert.Equal(gameId, hostGameStarted);
        Assert.Equal(gameId, guestGameStarted);

        await hostConnection.DisposeAsync();
        await guestConnection.DisposeAsync();
    }




}