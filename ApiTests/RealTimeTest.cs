using System.Net;
using ApiTests;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Services;

public class RealTimeTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    private const string IN_MEMORY_HUB_URL = "wss://localhost/hubs/GameServiceHub";

    public RealTimeTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;

    }

    private HubConnection CreateClient()
    {
        var client = _factory.CreateClient();
        var server = _factory.Server;
        return new HubConnectionBuilder()
        .WithUrl(IN_MEMORY_HUB_URL, options =>
        {
            options.HttpMessageHandlerFactory = _ => server.CreateHandler();
        }).Build();
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
    public async Task UpdateOthersOnPlayerConnection()
    {

        const string EXPECTED_RESULT = "230232d3-832f-410a-b51e-61611c039b56";
        const string RECIEVED_UPDATE = "RECIEVED_UPDATE";

        TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();

        var testGameId = Guid.NewGuid().ToString();

        var client1 = CreateClient();
        var client2 = CreateClient();

        client1.On<string>(RECIEVED_UPDATE, (recievedUpdateResponse) =>
        {
            taskCompletionSource.TrySetResult(recievedUpdateResponse);
        });

        await client1.StartAsync();
        await client2.StartAsync();

        await client2.InvokeAsync("UpdatePlayers", EXPECTED_RESULT);

        string recieved = await taskCompletionSource.Task;

        Assert.Equal(EXPECTED_RESULT, recieved);
    }

    [Fact]
    public async Task NotifyOthersOnPlayerConnection()
    {
        const string playerName = "Nicholas";
        const string EXPECTED_MESSAGE = $"{playerName} Joined!";
        const string RECIEVED_UPDATE = "PlayerJoinedNotification";

        var taskCompletionSource = new TaskCompletionSource<string>();

        var client1 = CreateClient();
        var client2 = CreateClient();

        client1.On<string>(RECIEVED_UPDATE, (recievedUpdateResponse) =>
        {
            taskCompletionSource.TrySetResult(recievedUpdateResponse);
        });

        await client1.StartAsync();
        await client2.StartAsync();

        await client2.InvokeAsync("NotifyOthers", playerName);

        var receivedMessage = await taskCompletionSource.Task;

        Assert.Equal(EXPECTED_MESSAGE, receivedMessage);
    }

}