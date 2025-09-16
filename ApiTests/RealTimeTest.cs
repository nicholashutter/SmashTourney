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
        var _connection = CreateClient();

        await _connection.StartAsync();

        var received = false;

        _connection.On("Successfully Joined", () =>
        {
            received = true;
        });

        await Task.Delay(500);

        Assert.True(received, "Client did not receive 'Successfully Joined' message.");
    }

    [Fact]
    public async Task UpdateOthersOnPlayerConnection()
    {

        var receivedPayload = string.Empty;
        var testGameId = Guid.NewGuid().ToString();

        var client1 = CreateClient();
        var client2 = CreateClient();

        client1.On<string>("ReceiveMessage", payload =>
        {
            receivedPayload = payload;
        });

        await client1.StartAsync();
        await client2.StartAsync();

        await client2.InvokeAsync("UpdatePlayers", testGameId);

        await Task.Delay(500);

        Assert.Equal(testGameId, receivedPayload);
    }

    [Fact]
public async Task NotifyOthersOnPlayerConnection()
{
    var receivedMessage = string.Empty;
    var playerName = "Nicholas";

    var client1 = CreateClient();

    var client2 = CreateClient();

    await client1.StartAsync();
    await client2.StartAsync();

    await client2.InvokeAsync("NotifyOthers", playerName);

    await Task.Delay(500);

    Assert.Equal($"{playerName} Joined!", receivedMessage);
}

}